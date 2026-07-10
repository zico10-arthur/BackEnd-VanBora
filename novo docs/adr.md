# Architecture Decision Records — Vanbora

Registro das decisões arquiteturais do projeto, documentando contexto, decisão tomada e consequências.

---

## ADR-001: Clean Architecture com 4 camadas

**Status:** Implementado

**Contexto:**  
Precisávamos de uma arquitetura que permitisse evoluir o domínio sem acoplamento a frameworks, ORMs ou detalhes de infraestrutura. O sistema tem regras de negócio complexas (reservas, pagamentos, máquinas de estado) que não podem ficar espalhadas em controllers ou repositórios.

**Decisão:**  
Adotamos **Clean Architecture** com 4 camadas: `Api` → `Application` → `Domain` → `Infrastructure`. A regra de dependência aponta para dentro — camadas externas conhecem as internas, nunca o contrário.

**Alternativas consideradas:**
- **N-Layer tradicional (Controllers → Services → Repositories):** Rejeitada por acoplar domínio à infraestrutura e dificultar testes.
- **Vertical Slice:** Rejeitada para o MVP por adicionar complexidade desnecessária. Pode ser reavaliada se o sistema crescer muito.

**Consequências:**
- ✅ Domínio 100% testável (sem dependência de EF Core, ASP.NET, etc.)
- ✅ Troca de infraestrutura (banco, gateway de pagamento) sem afetar regras de negócio
- ❌ Mais projetos e pastas — curva de aprendizado inicial maior
- ❌ Mapeamento entre camadas necessário (AutoMapper)

---

## ADR-002: Result Pattern para erros de domínio

**Status:** Implementado

**Contexto:**  
O domínio precisa comunicar falhas de forma explícita e tipada. Exceções são caras (stack trace) e não fazem parte do fluxo normal de negócio. Precisávamos de um mecanismo que permitisse ao chamador decidir o que fazer com cada tipo de erro, sem try/catch.

**Decisão:**  
Toda operação que pode falhar retorna `Result<T>` (sucesso com valor) ou `Result` (sucesso sem valor). Erros são modelados como `Error` com `Code`, `Message` e `ErrorType`. Value Objects usam `Result<T>` como retorno de factory. Services usam `Result<T>` como retorno de métodos.

**Alternativas consideradas:**
- **Lançar exceções de domínio:** Rejeitada. Exceções são para situações excepcionais (invariantes quebradas, bugs), não para fluxos esperados como "CPF inválido" ou "assento ocupado".
- **Maybe/Option pattern:** Rejeitada por não carregar informação suficiente sobre o tipo de erro.
- **OneOf library:** Rejeitada para manter dependências mínimas.

**Consequências:**
- ✅ Erros de negócio são cidadãos de primeira classe — explícitos na assinatura do método
- ✅ Sem efeito colateral de stack trace para fluxos normais
- ✅ `ResultFilter` converte automaticamente `ErrorType` → HTTP status code
- ❌ Propagações manuais de `Result` entre camadas
- ❌ Não captura exceções inesperadas automaticamente (tratadas pelo `ExceptionMiddleware`)

---

## ADR-003: Usuario como tabela única (Single Table)

**Status:** Implementado

**Contexto:**  
O sistema tem 4 perfis: Passageiro, Gerente, Motorista e Admin. Eles compartilham campos base (Nome, CPF, Email, SenhaHash) mas têm atributos específicos (Gerente tem Slug/TaxaPlataforma, Motorista tem CNH, Admin não tem campos extras). Precisávamos decidir entre herança (TPH/TPT/TPC) ou tabela única com campos nullable.

**Decisão:**  
Usamos **tabela única** (`Usuario`) com `TipoUsuario` como discriminador. Campos específicos de perfil são **nullable** no banco. A criação é feita por **factory methods estáticos** (`CriarPassageiro()`, `CriarGerente()`, `CriarMotorista()`, `CriarAdmin()`) que garantem que os campos obrigatórios de cada perfil sejam preenchidos.

**Alternativas consideradas:**
- **TPH (Table Per Hierarchy) via EF Core:** Rejeitada. EF Core exigiria classes derivadas, adicionando complexidade desnecessária para a quantidade de campos específicos.
- **TPT (Table Per Type):** Rejeitada. Joins extras em toda consulta, performance pior.
- **Tabelas separadas por perfil:** Rejeitada. CPF e Email precisam ser únicos globalmente — tabelas separadas complicariam essa restrição.

**Consequências:**
- ✅ Unicidade de CPF e Email garantida naturalmente por constraint de banco
- ✅ Consultas simples sem joins
- ✅ Login unificado independente do perfil
- ✅ Upgrade de Passageiro → Gerente (`UpgradeParaGerente()`) trivial — só atualiza campos
- ❌ Campos nullable exigem verificação de perfil antes de acessar
- ❌ Se perfis divergirem muito no futuro, a tabela pode ficar com muitos campos nullable

---

## ADR-004: Value Objects imutáveis com factory method Criar()

**Status:** Implementado

**Contexto:**  
CPF, CNH, Email, Telefone, Placa e Dinheiro precisam de validação rigorosa e consistente em todo o sistema. Criar esses valores em qualquer lugar sem validação centralizada seria frágil.

**Decisão:**  
Value Objects são **sealed classes** com construtor privado e método estático `Criar()` que retorna `Result<T>`. São imutáveis (propriedades `{ get; }` sem setter). Implementam `Equals()`, `GetHashCode()`, `ToString()` e `implicit operator string`. Usam `stackalloc` e `ReadOnlySpan<char>` para evitar alocações desnecessárias durante a validação.

**Alternativas consideradas:**
- **Construtor público com validação:** Rejeitada. Forçaria try/catch em todo lugar que cria um VO e misturaria exceções com fluxo normal.
- **DataAnnotations em primitivos:** Rejeitada. Validação ficaria espalhada e não seria reutilizável.
- **FluentValidation para VOs:** Rejeitada. Adicionaria dependência externa ao domínio.

**Consequências:**
- ✅ Validação centralizada e consistente — impossível criar um CPF inválido
- ✅ `Result<T>` permite que o chamador trate o erro sem exceções
- ✅ `implicit operator` permite uso transparente em comparações e atribuições
- ✅ Performance: `stackalloc` + `ReadOnlySpan<char>` evitam alocações no heap durante parsing
- ❌ Um pouco mais verboso que primitivos simples

---

## ADR-005: FluentValidation para validação de entrada

**Status:** Implementado

**Contexto:**  
As requests HTTP precisam ser validadas antes de chegar à lógica de negócio. Precisávamos de validação rica (regras condicionais, mensagens em português) sem poluir os DTOs.

**Decisão:**  
Usamos **FluentValidation** com validators separados para cada request. Os validators são registrados no DI e injetados manualmente nos serviços (não usamos o middleware automático do ASP.NET para ter controle sobre o fluxo de erro).

**Alternativas consideradas:**
- **DataAnnotations:** Rejeitada. Regras complexas (ex: "se campo A está preenchido, campo B é obrigatório") são difíceis de expressar. Mensagens de erro difíceis de customizar.
- **Validação manual nos serviços:** Rejeitada. Duplicaria código e misturaria validação de entrada com lógica de negócio.

**Consequências:**
- ✅ Validação declarativa e legível
- ✅ Regras condicionais e cross-field triviais com `When()` / `Unless()`
- ✅ Mensagens em português customizadas
- ✅ Testáveis isoladamente
- ❌ Dependência externa na camada de Application
- ❌ Performance marginalmente pior que validação manual

---

## ADR-006: JWT Bearer para autenticação

**Status:** Implementado

**Contexto:**  
O sistema precisa autenticar 4 perfis de usuário em uma API REST. A autenticação deve ser stateless para facilitar escalabilidade horizontal e integração com frontend SPA.

**Decisão:**  
Usamos **JWT Bearer** com claims customizadas: `sub` (userId Guid), `email`, `nome`, `tipos` (TipoUsuario string). O token é gerado pelo `TokenService` e validado pelo middleware `AddJwtBearer`. A chave secreta deve ter ≥ 32 caracteres (validado na inicialização).

**Alternativas consideradas:**
- **Session-based (cookies):** Rejeitada. Exigiria estado no servidor, complicando escalabilidade. Não funciona bem com mobile/SPA.
- **OAuth2/OpenID Connect com IdentityServer:** Rejeitada para MVP. Complexidade desnecessária para um sistema sem integração com provedores externos.

**Consequências:**
- ✅ Stateless — qualquer instância da API pode validar tokens
- ✅ Frontend armazena token no localStorage e envia via header `Authorization: Bearer`
- ❌ Revogação de token é impossível (sem blacklist)
- ❌ Chave secreta comprometida = todos os tokens são comprometidos
- ❌ Payload do token visível (apenas base64, não criptografado) — não colocar dados sensíveis

---

## ADR-007: MercadoPago como gateway de pagamento

**Status:** Implementado

**Contexto:**  
O Vanbora precisa processar pagamentos de reservas. O mercado brasileiro tem opções regionais e a integração precisa suportar PIX (pagamento instantâneo).

**Decisão:**  
Usamos **MercadoPago** como gateway, integrado via API REST. A interface `IPagamentoGateway` abstrai o provedor, permitindo troca futura. O webhook de confirmação usa validação de assinatura HMAC-SHA256.

**Alternativas consideradas:**
- **Stripe:** Rejeitada. Suporte a PIX é limitado e foco principal é mercado americano/europeu.
- **PagSeguro:** Considerada. Teria funcionalidade similar, mas MercadoPago foi escolhido por familiaridade da equipe.
- **PicPay:** Rejeitada. Ecossistema mais limitado.

**Consequências:**
- ✅ PIX nativo com QR Code
- ✅ Abstração via `IPagamentoGateway` permite trocar de provedor sem alterar domínio
- ✅ Webhook com validação de assinatura impede falsificação
- ❌ Dependência de serviço externo — se o MercadoPago estiver fora do ar, pagamentos param
- ❌ Taxas do MercadoPago (~3-4% por transação)

---

## ADR-008: PostgreSQL + Entity Framework Core

**Status:** Implementado

**Contexto:**  
Precisávamos de um banco relacional com suporte a transações ACID, constraints de unicidade e consultas complexas (disponibilidade de assentos, relatórios financeiros).

**Decisão:**  
Usamos **PostgreSQL** com **Entity Framework Core 9** e provider **Npgsql**. As configurações de mapeamento são feitas via Fluent API em classes separadas (`*Configuration.cs`). Migrations são gerenciadas pelo EF Core CLI.

**Alternativas consideradas:**
- **SQL Server:** Rejeitada por custo de licenciamento e menor suporte em ambientes Linux/containers.
- **MySQL:** Rejeitada por funcionalidades inferiores ao PostgreSQL (JSONB, índices parciais, CTEs recursivas).
- **Dapper (micro-ORM):** Rejeitada para MVP. Perderíamos migrations, change tracking e navegação entre entidades. Pode ser usada para queries de relatório no futuro.
- **MongoDB (NoSQL):** Rejeitada. O domínio é relacional por natureza (reservas, usuários, vans com relacionamentos fortes).

**Consequências:**
- ✅ PostgreSQL: robusto, gratuito, suporte nativo a JSONB e índices avançados
- ✅ EF Core: migrations, change tracking, navegação entre entidades
- ✅ Fluent API em classes separadas mantém o `AppDbContext` limpo
- ❌ EF Core tem overhead de performance vs Dapper para queries complexas
- ❌ Migrations podem causar conflitos em branches paralelas

---

## ADR-009: ResultFilter como Action Filter

**Status:** Implementado

**Contexto:**  
Todo controller verifica `result.IsFailure` e mapeia manualmente para HTTP status codes. Isso gera repetição e inconsistência entre controllers.

**Decisão:**  
Criamos um `ResultFilter` (IAsyncResultFilter) que intercepta `ObjectResult` após a execução da action. Se `Value` implementa `IAppResult`, o filtro automaticamente: `IsSuccess` → 200/204, `IsFailure` → mapeia `ErrorType` para HTTP status code.

**Alternativas consideradas:**
- **Mapeamento manual em cada controller:** Rejeitada. Código repetitivo, inconsistente.
- **BaseController com método helper:** Rejeitada. Ainda exige herança e chamada manual.
- **Middleware de resultado:** Rejeitada. Action filter tem acesso ao `ResultExecutingContext` com mais informações.

**Consequências:**
- ✅ Zero código de mapeamento nos controllers — basta retornar `Result<T>`
- ✅ Consistência garantida: todo `ErrorType.Validation` vira 400, `NotFound` vira 404, etc.
- ✅ Log automático de tipo de resultado
- ❌ "Mágica" — novo desenvolvedor pode não entender de onde vem o status code
- ❌ Depende de os controllers retornarem `ObjectResult` com o `Result<T>` como valor

---

## ADR-010: Motorista como tipo de Usuario (não entidade separada)

**Status:** Implementado

**Contexto:**  
Motoristas são cadastrados pelo Gerente e podem ser alocados em ViagemVan. A dúvida era: Motorista é uma entidade separada ou um tipo de Usuario?

**Decisão:**  
Motorista é um **tipo de Usuario** (`TipoUsuario.Motorista`), não uma entidade separada. É criado via `Usuario.CriarMotorista()` e vinculado ao Gerente via `CriadoPorUsuarioId`. Motoristas **não têm login** (Email e SenhaHash são null).

**Alternativas consideradas:**
- **Entidade Motorista separada:** Rejeitada. Motorista compartilha campos base (Nome, CPF, Telefone) com Usuario. Entidade separada duplicaria esses campos e complicaria validações de unicidade de CPF.
- **Herança (Motorista : Usuario):** Rejeitada. EF Core exigiria TPH/TPT, adicionando complexidade.

**Consequências:**
- ✅ Unicidade de CPF garantida globalmente (não pode ter Usuario e Motorista com mesmo CPF)
- ✅ Simples de listar "todos os motoristas do gerente" (WHERE CriadoPorUsuarioId = X AND Tipo = Motorista)
- ❌ Motorista não pode virar Passageiro ou Gerente sem migração manual de dados
- ❌ Campos específicos de Motorista (CNH) ficam na mesma tabela

---

## ADR-011: ViagemVan como entidade de junção explícita

**Status:** Implementado

**Contexto:**  
Uma Viagem pode ter várias Vans, e uma Van pode estar em várias Viagens. A relação tem atributos próprios: `MotoristaUsuarioId` (alocação de motorista à van naquela viagem específica).

**Decisão:**  
Modelamos `ViagemVan` como uma **entidade de junção explícita** com seu próprio `Id` e propriedades, em vez de usar o mapeamento implícito many-to-many do EF Core.

**Alternativas consideradas:**
- **EF Core many-to-many implícito (sem entidade de junção):** Rejeitada. Precisamos de `MotoristaUsuarioId` na relação — o many-to-many implícito não suporta atributos na tabela de junção.
- **Usar a tabela de junção automática e acessar via shadow properties:** Rejeitada. Shadow properties são frágeis e difíceis de consultar.

**Consequências:**
- ✅ Atributos na relação (MotoristaUsuarioId) modelados naturalmente
- ✅ Navegação explícita: `Viagem.ViagemVans`, `ViagemVan.Reservas`
- ✅ Controle total sobre a tabela de junção
- ❌ Um pouco mais verboso que many-to-many implícito
- ❌ Precisa gerenciar a coleção manualmente (`_viagemVans`)

---

## ADR-012: Background Service para expiração de reservas

**Status:** Implementado

**Contexto:**  
Reservas pendentes de pagamento expiram em 10 minutos. Precisamos liberar os assentos automaticamente se o pagamento não for confirmado.

**Decisão:**  
Usamos um **IHostedService** (`ExpirarReservasBackgroundService`) que roda um timer periódico. Ele consulta reservas com `Status = PendentePagamento` e `ExpiraEm <= DateTime.UtcNow`, e as marca como `Expirada`.

**Alternativas consideradas:**
- **Trigger de banco (pg_cron):** Rejeitada. Lógica de negócio no banco dificulta testes e versionamento.
- **Hangfire/Quartz scheduler:** Rejeitada para MVP. Complexidade desnecessária para uma tarefa simples.
- **Verificação no momento da consulta:** Considerada. Mas não libera assentos para outros compradores até que alguém tente reservar — experiência ruim.

**Consequências:**
- ✅ Simples de implementar (30 linhas)
- ✅ Sem dependência externa
- ✅ Assentos liberados automaticamente para outros compradores
- ❌ Não é distribuído — se houver múltiplas instâncias, todas executarão o timer
- ❌ Intervalo fixo, não em tempo real (pode levar até o próximo tick para expirar)

---

## ADR-013: Exclusão de conta em duas etapas (código por email)

**Status:** Implementado

**Contexto:**  
Usuários precisam poder excluir suas contas, mas exclusão acidental seria catastrófica (perde histórico de reservas, dados financeiros). Precisávamos de uma confirmação forte.

**Decisão:**  
Exclusão em **duas etapas**: (1) `POST /api/auth/solicitar-exclusao` gera código de 6 dígitos e envia por email; (2) `POST /api/auth/confirmar-exclusao` valida o código e desativa a conta (soft delete: `Ativo = false`). O código expira em 15 minutos. Bloqueia exclusão se houver reservas ativas (Confirmada/EmAndamento).

**Alternativas consideradas:**
- **Exclusão direta (um clique):** Rejeitada. Muito arriscada, sem confirmação.
- **Confirmação por senha:** Rejeitada. Se o atacante tem a senha, a confirmação não adiciona segurança.
- **Soft delete sem confirmação por email:** Rejeitada. Não prova que o dono do email solicitou.

**Consequências:**
- ✅ Segurança: prova que o solicitante tem acesso ao email cadastrado
- ✅ Bloqueio de exclusão com reservas ativas evita cenários inconsistentes
- ✅ Soft delete permite recuperação futura se necessário (via admin)
- ❌ Depende de serviço de email funcionando
- ❌ Dois requests HTTP necessários para excluir

---

## ADR-014: Slug por gerente para URL pública

**Status:** Implementado

**Contexto:**  
Cada gerente precisa de uma URL pública única para seus passageiros encontrarem suas viagens (ex: `/joao-turismo`). IDs (GUID) não são amigáveis.

**Decisão:**  
Cada Gerente tem um `Slug` único (string lowercase, sem espaços). O slug é definido no registro e pode ser alterado depois (`PUT /api/auth/slug`). A unicidade é garantida por validação em serviço + índice único no banco.

**Alternativas consideradas:**
- **Usar Nome do gerente como slug:** Rejeitada. Dois gerentes podem ter o mesmo nome.
- **Gerar slug automático sem possibilidade de alteração:** Rejeitada. Gerente pode querer mudar o nome público do seu negócio.
- **Subdomínio por gerente:** Rejeitada. Complexidade de DNS e certificados SSL.

**Consequências:**
- ✅ URLs amigáveis e memorizáveis
- ✅ Alterável pelo gerente
- ✅ Unicidade garantida
- ❌ Mudança de slug quebra links compartilhados anteriormente

---

## Resumo das Decisões

| ADR | Decisão | Status |
|---|---|---|
| ADR-001 | Clean Architecture 4 camadas | Implementado |
| ADR-002 | Result Pattern (sem exceções para domínio) | Implementado |
| ADR-003 | Usuario como tabela única | Implementado |
| ADR-004 | Value Objects imutáveis com factory | Implementado |
| ADR-005 | FluentValidation para validação | Implementado |
| ADR-006 | JWT Bearer para autenticação | Implementado |
| ADR-007 | MercadoPago como gateway | Implementado |
| ADR-008 | PostgreSQL + EF Core | Implementado |
| ADR-009 | ResultFilter como Action Filter | Implementado |
| ADR-010 | Motorista como tipo de Usuario | Implementado |
| ADR-011 | ViagemVan como entidade de junção explícita | Implementado |
| ADR-012 | Background Service para expiração | Implementado |
| ADR-013 | Exclusão de conta em duas etapas | Implementado |
| ADR-014 | Slug único por gerente | Implementado |
