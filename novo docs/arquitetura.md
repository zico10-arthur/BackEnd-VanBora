# Documento de Arquitetura — Vanbora Backend

## 1. Visão Geral da Arquitetura

O backend do Vanbora adota **Clean Architecture** com 4 camadas, seguindo os princípios de **Domain-Driven Design (DDD)** tático. A regra de dependência aponta para dentro: camadas externas dependem das internas, nunca o contrário.

```
┌──────────────────────────────────────────────────────┐
│                     Api (ASP.NET Core 9)              │
│  Controllers · Middleware · Background Services       │
├──────────────────────────────────────────────────────┤
│                VanBora.Application                    │
│  Services · DTOs · Interfaces · Validators · Mappings │
├──────────────────────────────────────────────────────┤
│                  VanBora.Domain                        │
│  Entities · Value Objects · Enums · Common (Result)   │
│  Interfaces (Repository contracts)                    │
├──────────────────────────────────────────────────────┤
│              VanBora.Infrastructure                    │
│  EF Core · Repositories · External Services           │
│  (MercadoPago, Email, JWT Token)                      │
└──────────────────────────────────────────────────────┘
```

### Regra de Dependência

| Camada | Depende de |
|---|---|
| **Api** | Application, Domain, Infrastructure |
| **Application** | Domain |
| **Domain** | Nenhuma (pura) |
| **Infrastructure** | Domain, Application (interfaces) |

---

## 2. Camada de Domínio (`VanBora.Domain`)

É o coração do sistema. Não depende de nenhuma biblioteca externa (apenas .NET runtime). Contém toda a lógica de negócio.

### 2.1 Entidades

#### Diagrama de Entidades

```
┌──────────┐      ┌──────────┐      ┌──────────────┐
│  Usuario  │      │   Van    │      │    Viagem     │
│          │      │          │      │               │
│ Id       │      │ Id       │      │ Id            │
│ Nome     │      │ Nome     │      │ NomeEvento    │
│ CPF (VO) │      │ Placa(VO)│      │ DataEvento    │
│ Email(VO)│      │ Modelo   │      │ LocalEvento   │
│ Telefone │      │ Capacidade│     │ DataPartida   │
│ Tipo     │      │ Ativo    │      │ LocalPartida  │
│ Slug*    │      │ CriadoEm │      │ PrecoAssento  │
│ Taxa*    │      └────┬─────┘      │ PossuiIngresso│
│ CNH(VO)* │           │            │ QuorumMinimo  │
│ ChavePix*│           │            │ Status        │
└────┬─────┘           │            └───────┬───────┘
     │                 │                    │
     │          ┌──────┴──────────┐         │
     │          │   ViagemVan     │─────────┘
     │          │                 │
     │          │ Id              │
     ├──────────│ ViagemId        │
     │          │ VanId           │
     │          │ MotoristaUserId*│
     │          └────────┬────────┘
     │                   │
     │          ┌────────┴────────┐
     │          │    Reserva      │
     │          │                 │
     └──────────│ Id              │
                │ UsuarioId       │
                │ ViagemVanId     │
                │ Status          │
                │ ValorTotal      │
                │ TaxaPlataforma  │
                │ CodigoPix       │
                │ TransacaoId     │
                │ ExpiraEm        │
                └────────┬────────┘
                         │
                ┌────────┴─────────┐
                │   ItemReserva    │
                │                  │
                │ Id               │
                │ NumeroAssento    │
                │ PrecoAssento(VO) │
                │ NomePassageiro   │
                │ EmailPassageiro(VO)│
                │ TelefonePassageiro│
                │ CPFPassageiro(VO) │
                └──────────────────┘
```

#### Descrição das Entidades

| Entidade | Responsabilidade | Detalhes |
|---|---|---|
| **Usuario** | Identidade de todos os perfis | Tabela única com `TipoUsuario` (Passageiro, Gerente, Motorista, Admin). Usa **Factory Methods** estáticos: `CriarPassageiro()`, `CriarGerente()`, `CriarMotorista()`, `CriarAdmin()`. Campos específicos de perfil são nullable. |
| **Van** | Veículo do gerente | Capacidade mínima de 2 (motorista + 1 passageiro). Método `ObterQuantidadeAssentosDisponiveis()` retorna `Capacidade - 1`. Pode ser ativada/desativada (soft delete). |
| **Viagem** | Anúncio de viagem para um evento | Conecta Gerente → Evento. Status: Agendada → EmAndamento → Concluída (ou Cancelada). Máquina de estados com validações de transição (`Iniciar()`, `Concluir()`, `Cancelar()`). |
| **ViagemVan** | Join entity: Viagem ↔ Van + Motorista | Aloca uma Van a uma Viagem e opcionalmente um Motorista. Métodos `AlocarMotorista()` e `DesalocarMotorista()`. |
| **Reserva** | Reserva de assentos feita pelo passageiro | Máquina de estados: PendentePagamento → Confirmada → EmAndamento → Concluída (ou Cancelada / Expirada). Expira em 10 minutos se não paga. |
| **ItemReserva** | Um assento reservado | Cada item representa 1 passageiro com seus dados completos (Nome, Email, Telefone, CPF). Vinculado à Reserva. |

### 2.2 Value Objects

Todos são **imutáveis** e possuem validação própria via método `Criar()` que retorna `Result<T>`.

| VO | Validação | Uso |
|---|---|---|
| **CPF** | 11 dígitos + algoritmo de dígitos verificadores (Receita Federal) | Usuario.CPF, ItemReserva.CPFPassageiro |
| **CNH** | 11 dígitos + algoritmo SEFAZ | Usuario.CNH (apenas Motorista) |
| **Email** | Regex Source Generator, máx 254 caracteres, lowercase invariante | Usuario.Email, ItemReserva.EmailPassageiro |
| **Telefone** | Hashset de DDDs válidos (67 códigos), 8-9 dígitos (celular começa com 9) | Usuario.Telefone, ItemReserva.TelefonePassageiro |
| **Placa** | Regex Source Generator: Mercosul (ABC1D23) ou cinza antigo (ABC-1234) | Van.Placa |
| **Dinheiro** | Moedas suportadas (BRL, USD, EUR), valor ≥ 0, arredondamento 2 casas decimais. Operações: `Somar()`, `Subtrair()`, `Multiplicar()`, `Percentual()` | ItemReserva.PrecoAssento |

### 2.3 Enums

| Enum | Valores |
|---|---|
| **TipoUsuario** | `Passageiro`, `Gerente`, `Motorista`, `Admin` |
| **StatusReserva** | `PendentePagamento`, `Confirmada`, `EmAndamento`, `Concluida`, `Cancelada`, `Expirada` |
| **StatusViagem** | `Agendada`, `EmAndamento`, `Concluida`, `Cancelada` |

### 2.4 Padrões de Domínio

#### Result Pattern

Todas as operações que podem falhar retornam `Result<T>` ou `Result` (sem valor). O domínio **não lança exceções** para erros de negócio — apenas para invariantes de construção (via `Guard`).

```csharp
// Value Object factory
public static Result<CPF> Criar(string? valor)

// Service
Task<Result<ReservaResponse>> CriarReservaAsync(...)

// Erro tipado
Error.Validation("CPF_INVALIDO", "CPF deve ter 11 dígitos.")
Error.NotFound("VIAGEM_NAO_ENCONTRADA", "...")
Error.Conflict("ASSENTO_OCUPADO", "...")
Error.Unauthorized("CREDENCIAIS_INVALIDAS", "...")
Error.Forbidden("ACESSO_NEGADO", "...")
```

#### Guard Clauses

Validações de invariantes no construtor usando exceções (para erros de programação, não de negócio):

| Método | Quando usar |
|---|---|
| `AgainstNull(value, name)` | Parâmetro nulo |
| `AgainstNullOrWhiteSpace(value, name)` | String vazia |
| `AgainstNegativeOrZero(value, name)` | decimal ≤ 0 |
| `AgainstNegative(value, name)` | decimal < 0 |
| `AgainstLessThan(value, min, name)` | int abaixo do mínimo |
| `AgainstFutureDate(value, name)` | Data no futuro |
| `AgainstPastDate(value, name)` | Data no passado |
| `AgainstInvalidState(condition, message)` | Estado inválido (via `InvalidOperationException`) |
| `AgainstEmptyGuid(value, name)` | Guid vazio |

#### Factory Methods (Usuario)

Em vez de herança, `Usuario` usa **factory methods estáticos** para criar cada perfil:

```csharp
Usuario.CriarPassageiro(nome, cpf, email, senhaHash, telefone)
Usuario.CriarGerente(nome, cpf, email, senhaHash, telefone, slug, taxa, gratuito, chavePix)
Usuario.CriarMotorista(nome, cpf, telefone, cnh, criadoPorUsuarioId)
Usuario.CriarAdmin(nome, cpf, email, senhaHash)
```

---

## 3. Camada de Aplicação (`VanBora.Application`)

Contém a lógica de orquestração (use cases). Não sabe nada sobre HTTP, banco de dados ou infraestrutura.

### 3.1 Serviços

| Serviço | Responsabilidade |
|---|---|
| **AuthService** | Registro de gerente e passageiro, login, atualização de perfil, alteração de senha, atualização de slug, exclusão de conta |
| **LoginService** | Validação de credenciais (email + senha hash) |
| **UsuarioService** | CRUD de usuários, busca por CPF/Email/Slug |
| **VanService** | CRUD de vans do gerente (listar, criar, atualizar, remover) |
| **ViagemService** | CRUD de viagens, alocação/remoção de vans e motoristas |
| **ViagemPublicService** | Listagem pública de viagens por slug do gerente |
| **ReservaService** | Criação de reserva com validação de assentos, listagem, contato do gerente, webhook de pagamento, expiração automática |
| **MotoristaService** | CRUD de motoristas vinculados ao gerente |
| **AdminService** | Busca de usuários, histórico de reservas (admin) |
| **RelatorioService** | Relatório financeiro de viagem com indicadores consolidados |

### 3.2 Interfaces (Contratos)

| Interface | Implementações |
|---|---|
| `IAuthService` | `AuthService` |
| `ILoginService` | `LoginService` |
| `IUsuarioService` | `UsuarioService` |
| `IVanService` | `VanService` |
| `IViagemService` | `ViagemService` |
| `IViagemPublicService` | `ViagemPublicService` |
| `IReservaService` | `ReservaService` |
| `IMotoristaService` | `MotoristaService` |
| `IAdminService` | `AdminService` |
| `IRelatorioService` | `RelatorioService` |
| `ITokenService` | `TokenService` (Infrastructure) |
| `IEmailService` | `EmailService` (Infrastructure) |
| `IPagamentoGateway` | `MercadoPagoPagamentoGateway` (Infrastructure) |

### 3.3 DTOs

Organizados por domínio:

| Namespace | DTOs |
|---|---|
| `DTOs.Auth` | `LoginRequest/Response`, `RegistrarGerenteRequest/Response`, `RegistrarPassageiroRequest/Response`, `RegistrarMotoristaRequest/Response`, `AtualizarUsuarioRequest/Response`, `AlterarSenhaRequest`, `AtualizarSlugRequest`, `ConfirmarExclusaoRequest/Response`, `SolicitarExclusaoResponse`, `GerenteResponse` |
| `DTOs.Vans` | `CriarVanRequest`, `AtualizarVanRequest`, `VanResponse` |
| `DTOs.Viagens` | `CriarViagemRequest`, `AtualizarViagemRequest`, `ViagemResponse`, `ViagemPublicaResponse`, `AlocarVanRequest`, `AlocarMotoristaRequest`, `RelatorioResponse` |
| `DTOs.Reservas` | `CriarReservaRequest`, `ReservaResponse`, `ContatoGerenteResponse` |
| `DTOs.Admin` | `UsuarioAdminResponse`, `GerenteAdminResponse`, `CriarGerenteAdminRequest`, `AtualizarGerenteAdminRequest`, `ReservaHistoricoResponse`, `ViagemGerenteHistoricoResponse` |

### 3.4 Validators (FluentValidation)

Toda request é validada antes de chegar ao serviço. Validators implementam `IValidator<T>` do FluentValidation:

| Validator | Request validada |
|---|---|
| `RegistrarGerenteValidator` | `RegistrarGerenteRequest` |
| `RegistrarPassageiroRequestValidator` | `RegistrarPassageiroRequest` |
| `RegistrarMotoristaValidator` | `RegistrarMotoristaRequest` |
| `LoginValidator` | `LoginRequest` |
| `CriarVanValidator` | `CriarVanRequest` |
| `AtualizarVanValidator` | `AtualizarVanRequest` |
| `CriarViagemValidator` | `CriarViagemRequest` |
| `AtualizarViagemValidator` | `AtualizarViagemRequest` |
| `AlocarVanValidator` | `AlocarVanRequest` |
| `AlocarMotoristaValidator` | `AlocarMotoristaRequest` |
| `CriarReservaValidator` | `CriarReservaRequest` |
| `CriarGerenteAdminValidator` | `CriarGerenteAdminRequest` |
| `AtualizarGerenteAdminValidator` | `AtualizarGerenteAdminRequest` |

### 3.5 AutoMapper Profiles

| Profile | Mapeamentos |
|---|---|
| `VanProfile` | `Van` ↔ `VanResponse`, `CriarVanRequest` → `Van` |
| `MotoristaProfile` | `Usuario` (Motorista) ↔ `RegistrarMotoristaResponse` |
| `ViagemProfile` | `Viagem` ↔ `ViagemResponse`, mapeamentos de `ViagemVan` e `Motorista` |
| `ReservaProfile` | `Reserva` ↔ `ReservaResponse`, `ItemReserva` mapeamentos |
| `AdminProfile` | Admin-specific mappings |

---

## 4. Camada de Infraestrutura (`VanBora.Infrastructure`)

Implementações concretas que dependem de tecnologias externas.

### 4.1 Banco de Dados (EF Core + PostgreSQL)

```
AppDbContext : DbContext, IUnitOfWork
├── DbSet<Usuario>
├── DbSet<Van>
├── DbSet<Viagem>
├── DbSet<ViagemVan>
├── DbSet<Reserva>
└── DbSet<ItemReserva>

Configurations (Fluent API):
├── UsuarioConfiguration
├── VanConfiguration
├── ViagemConfiguration
├── ViagemVanConfiguration
├── ReservaConfiguration
└── ItemReservaConfiguration

Migrations (5 migrações):
├── 20260520_Initial
├── 20260523_AddQuorumMinimo
├── 20260526_AddReservas
├── 20260527_AddCodigoExclusao
└── 20260602_AddReservasViagensVans
```

O `AppDbContext` implementa `IUnitOfWork`, suportando transações manuais via `BeginTransactionAsync()`, `CommitAsync()`, `RollbackAsync()`.

### 4.2 Repositórios

| Repositório | Interface |
|---|---|
| `UsuarioRepository` | `IUsuarioRepository` |
| `VanRepository` | `IVanRepository` |
| `ViagemRepository` | `IViagemRepository` |
| `ViagemVanRepository` | `IViagemVanRepository` |
| `ReservaRepository` | `IReservaRepository` |
| `UnitOfWork` | `IUnitOfWork` |

### 4.3 Serviços de Infraestrutura

| Serviço | Interface | Responsabilidade |
|---|---|---|
| **TokenService** | `ITokenService` | Geração de JWT com claims (sub, email, nome, tipos) |
| **EmailService** | `IEmailService` | Envio de emails transacionais (código de exclusão, etc.) |
| **MercadoPagoPagamentoGateway** | `IPagamentoGateway` | Geração de PIX via API Mercado Pago |
| **MercadoPagoWebhookHandler** | — | Processamento de webhooks de pagamento com validação de assinatura `x-signature` |
| **MercadoPagoWebhookSignatureValidator** | — | Validador de assinatura HMAC-SHA256 do Mercado Pago |

---

## 5. Camada de API (`Api`)

Aplicação ASP.NET Core 9 Web API. Entry point do sistema.

### 5.1 Controllers

| Controller | Rota | Autenticação | Ações |
|---|---|---|---|
| **AuthController** | `api/auth` | Mista | Registrar gerente/passageiro/motorista, login, atualizar perfil, alterar senha, atualizar slug, solicitar/confirmar exclusão, listar/atualizar/remover motorista |
| **ViagensController** | `api/viagens` | Pública | Listar viagens disponíveis, obter por ID, listar vans da viagem |
| **ReservasController** | `api/reservas` | `[Authorize]` | Criar reserva, listar minhas reservas, obter por ID, contato do gerente |
| **Gerente.VansController** | `api/gerente/vans` | `[Authorize]` | CRUD de vans do gerente |
| **Gerente.ViagensController** | `api/gerente/viagens` | `[Authorize]` | CRUD de viagens, alocar/remover van, alocar/remover motorista, relatório |
| **Admin.UsuariosController** | `api/admin/usuarios` | `[Authorize]` | Buscar usuários, histórico de reservas (valida claim `tipos == Admin`) |
| **Admin.GerentesController** | `api/admin/gerentes` | `[Authorize]` | CRUD de gerentes (admin cria gerentes com taxa e gratuidade) |
| **PaymentsController** | `api/payments` | Pública | Webhook legado Mercado Pago |
| **WebhooksController** | `api/webhooks` | Pública | Webhook Mercado Pago PIX |

### 5.2 Middleware Pipeline

```
Request → ExceptionMiddleware → HTTPS Redirection → Authentication → Authorization → Controllers → ResultFilter → Response
```

| Middleware | Ordem | Função |
|---|---|---|
| **ExceptionMiddleware** | 1º | Captura exceções não tratadas, retorna JSON com `traceId`, mostra mensagem real apenas em `Development` ou se for `DomainException` |
| **HttpsRedirection** | 2º | Redireciona HTTP → HTTPS |
| **Authentication** | 3º | Valida JWT Bearer token |
| **Authorization** | 4º | Verifica claims e políticas |
| **ResultFilter** | Action Filter | Intercepta `IAppResult` retornado dos controllers e converte automaticamente: `IsSuccess` → `200 OK` / `204 NoContent`, `IsFailure` → mapeia `ErrorType` para HTTP status code |

### 5.3 Background Services (Hosted Services)

| Serviço | Função | Execução |
|---|---|---|
| **ExpirarReservasBackgroundService** | Expira reservas pendentes que passaram dos 10 minutos | Periódica (timer) |
| **DevDataSeeder** | Popula dados iniciais no ambiente de desenvolvimento | Na inicialização |

### 5.4 Injeção de Dependência (Program.cs)

```
builder.Services
├── JWT (Authentication + Authorization)
├── Settings (JwtSettings, MercadoPagoSettings, CorsSettings)
├── AutoMapper (4 profiles)
├── Application Services (10 serviços)
├── FluentValidation Validators (13 validators)
├── Infrastructure (via AddInfrastructure extension)
├── Hosted Services (2)
├── Controllers (com ResultFilter)
└── Swagger (Swashbuckle)
```

---

## 6. Fluxos Principais

### 6.1 Fluxo de Reserva

```
Passageiro autenticado
   │
   ├─ POST /api/reservas { viagemVanId, itens[] }
   │
   ▼
ReservasController.CriarReserva()
   │
   ▼
ReservaService.CriarReservaAsync()
   ├─ Valida se ViagemVan existe e pertence a viagem ativa
   ├─ Valida se assentos estão disponíveis (sem conflito)
   ├─ Valida se CPFs não estão duplicados na mesma ViagemVan
   ├─ Cria Reserva (Status: PendentePagamento)
   ├─ Cria ItensReserva vinculados
   ├─ Gera PIX via MercadoPagoPagamentoGateway
   ├─ Salva no banco
   └─ Retorna ReservaResponse com QR Code PIX
```

### 6.2 Fluxo de Pagamento (Webhook)

```
Mercado Pago
   │
   ├─ POST /api/webhooks/pix { action, data.id }
   │
   ▼
MercadoPagoWebhookHandler.ProcessAsync()
   ├─ Valida assinatura x-signature (HMAC-SHA256)
   ├─ Obtém payment_id
   ▼
ReservaService.ProcessarWebhookPagamentoAsync()
   ├─ Busca reserva pelo payment_id
   ├─ Confirma pagamento: Status → Confirmada
   └─ Salva TransacaoId e PagoEm
```

### 6.3 Fluxo de Criação de Viagem (Gerente)

```
Gerente autenticado
   │
   ├─ POST /api/gerente/viagens { nomeEvento, dataEvento, ... }
   │
   ▼
ViagensController.Criar()
   ▼
ViagemService.CriarAsync()
   ├─ Valida request (FluentValidation → CriarViagemValidator)
   ├─ Cria entidade Viagem (Status: Agendada)
   ├─ Salva no banco
   └─ Retorna ViagemResponse
   │
   ├─ POST /api/gerente/viagens/{id}/alocar-van { vanId }
   ▼
ViagemService.AlocarVanAsync()
   ├─ Valida se Van pertence ao gerente
   ├─ Cria ViagemVan (join)
   └─ Retorna ViagemResponse atualizada
   │
   ├─ POST /api/gerente/viagens/{id}/alocar-motorista/{viagemVanId} { motoristaUsuarioId }
   ▼
ViagemService.AlocarMotoristaAsync()
   ├─ Valida se Motorista pertence ao gerente
   ├─ Aloca motorista na ViagemVan
   └─ Retorna ViagemResponse atualizada
```

---

## 7. Configuração e Settings

| Setting | Seção no appsettings.json | Uso |
|---|---|---|
| **JwtSettings** | `Jwt` | SecretKey (≥ 32 chars), Issuer, Audience, ExpirationMinutes |
| **MercadoPagoSettings** | `MercadoPago` | AccessToken, WebhookSecret, UserId |
| **CorsSettings** | `Cors` | AllowedOrigins |
| **ConnectionStrings** | `ConnectionStrings:DefaultConnection` | PostgreSQL connection string (Npgsql) |

---

## 8. Segurança

| Mecanismo | Descrição |
|---|---|
| **JWT Bearer** | Autenticação stateless com claims: `sub` (Guid), `email`, `nome`, `tipos` (TipoUsuario) |
| **Autorização por Claims** | Admin validado via claim `tipos == Admin` |
| **Senhas** | Hash com BCrypt (validado pelo `LoginService`) |
| **Webhook** | Assinatura HMAC-SHA256 validada via header `x-signature` do Mercado Pago |
| **HTTPS** | Redirecionamento obrigatório |

---

## 9. Padrões de Projeto Utilizados

| Padrão | Onde | Propósito |
|---|---|---|
| **Repository Pattern** | Infrastructure/Repositories | Abstração do acesso a dados |
| **Unit of Work** | AppDbContext, UnitOfWork | Transações atômicas |
| **Result Pattern** | Domain/Common, todos os Services | Tratamento de erros sem exceções |
| **Factory Method** | Usuario.Criar*() | Criação polimórfica de tipos de usuário |
| **Guard Clauses** | Domain/Common/Guard | Validação de invariantes nos construtores |
| **Value Object** | Domain/ValueObjects | Imutabilidade e validação autocontida |
| **Strategy (Gateway)** | IPagamentoGateway | Abstração do gateway de pagamento |
| **Action Filter** | ResultFilter | Conversão automática Result → HTTP |
| **Middleware** | ExceptionMiddleware | Tratamento global de exceções |
| **Background Service** | ExpirarReservasBackgroundService | Tarefas agendadas |
| **DTO + AutoMapper** | Application/DTOs, Application/Mappings | Separação contrato API ↔ Domínio |
| **FluentValidation** | Application/Validators | Validação de request desacoplada |

---

## 10. Pontos de Extensão

| Ponto | Como estender |
|---|---|
| **Novo gateway de pagamento** | Implementar `IPagamentoGateway`, registrar no DI |
| **Novo perfil de usuário** | Adicionar valor em `TipoUsuario`, criar factory method em `Usuario` |
| **Novo serviço externo** | Criar interface em Application, implementação em Infrastructure |
| **Nova validação de request** | Criar `IValidator<T>` em Application/Validators |
| **Nova entidade** | Adicionar em Domain, criar Configuration e Repository em Infrastructure, Service em Application |

---

## 11. Diretórios e Arquivos

```
BackEnd-VanBora/
├── VanBora.sln                          # Solution file (4 projetos)
├── Api/                                 # Camada de apresentação
│   ├── Program.cs                       # Entry point, DI, pipeline
│   ├── Controllers/
│   │   ├── AuthController.cs            # Auth + Motorista
│   │   ├── ViagensController.cs         # Público
│   │   ├── ReservasController.cs        # Reservas do passageiro
│   │   ├── PaymentsController.cs        # Webhook legado
│   │   ├── WebhooksController.cs        # Webhook Mercado Pago
│   │   ├── Admin/
│   │   │   ├── UsuariosController.cs    # Admin: usuários
│   │   │   └── GerentesController.cs    # Admin: gerentes
│   │   └── Gerente/
│   │       ├── VansController.cs        # Gerente: vans
│   │       └── ViagensController.cs     # Gerente: viagens
│   ├── Middleware/
│   │   ├── ExceptionMiddleware.cs       # Tratamento global de exceções
│   │   └── ResultFilter.cs              # Action filter Result → HTTP
│   ├── Services/
│   │   ├── DevDataSeeder.cs             # Seed de desenvolvimento
│   │   ├── ExpirarReservasBackgroundService.cs
│   │   └── MercadoPagoWebhookHandler.cs # Handler de webhook
│   └── appsettings.json
├── VanBora.Domain/                      # Camada de domínio
│   ├── Entities/
│   │   ├── Usuario.cs
│   │   ├── Van.cs
│   │   ├── Viagem.cs
│   │   ├── ViagemVan.cs
│   │   ├── Reserva.cs
│   │   └── ItemReserva.cs
│   ├── ValueObjects/
│   │   ├── CPF.cs
│   │   ├── CNH.cs
│   │   ├── Email.cs
│   │   ├── Telefone.cs
│   │   ├── Placa.cs
│   │   └── Dinheiro.cs
│   ├── Enums/
│   │   ├── TipoUsuario.cs
│   │   ├── StatusReserva.cs
│   │   └── StatusViagem.cs
│   ├── Common/
│   │   ├── Result.cs
│   │   ├── IResult.cs
│   │   ├── Error.cs
│   │   ├── ErrorType.cs
│   │   ├── Guard.cs
│   │   └── DomainException.cs
│   ├── Services/
│   │   └── RelatorioViagem.cs           # Serviço de domínio
│   └── Interfaces/
│       ├── IUsuarioRepository.cs
│       ├── IVanRepository.cs
│       ├── IViagemRepository.cs
│       ├── IViagemVanRepository.cs
│       ├── IReservaRepository.cs
│       └── IUnitOfWork.cs
├── VanBora.Application/                 # Camada de aplicação
│   ├── Services/
│   │   ├── AuthService.cs
│   │   ├── LoginService.cs
│   │   ├── UsuarioService.cs
│   │   ├── VanService.cs
│   │   ├── ViagemService.cs
│   │   ├── ViagemPublicService.cs
│   │   ├── ReservaService.cs
│   │   ├── MotoristaService.cs
│   │   ├── AdminService.cs
│   │   └── RelatorioService.cs
│   ├── Interfaces/                      # Contratos de serviço
│   │   ├── IAuthService.cs
│   │   ├── ILoginService.cs
│   │   ├── IUsuarioService.cs
│   │   ├── IVanService.cs
│   │   ├── IViagemService.cs
│   │   ├── IViagemPublicService.cs
│   │   ├── IReservaService.cs
│   │   ├── IMotoristaService.cs
│   │   ├── IAdminService.cs
│   │   ├── IRelatorioService.cs
│   │   ├── ITokenService.cs
│   │   ├── IEmailService.cs
│   │   └── IPagamentoGateway.cs
│   ├── DTOs/                            # Data Transfer Objects
│   │   ├── Auth/
│   │   ├── Vans/
│   │   ├── Viagens/
│   │   ├── Reservas/
│   │   └── Admin/
│   ├── Validators/                      # FluentValidation
│   ├── Mappings/                        # AutoMapper profiles
│   ├── Helpers/
│   │   └── ClaimsHelper.cs
│   └── Settings/
│       ├── JwtSettings.cs
│       ├── MercadoPagoSettings.cs
│       └── CorsSettings.cs
└── VanBora.Infrastructure/              # Camada de infraestrutura
    ├── Data/
    │   ├── AppDbContext.cs
    │   └── Configurations/              # EF Core Fluent API
    │       ├── UsuarioConfiguration.cs
    │       ├── VanConfiguration.cs
    │       ├── ViagemConfiguration.cs
    │       ├── ViagemVanConfiguration.cs
    │       ├── ReservaConfiguration.cs
    │       └── ItemReservaConfiguration.cs
    ├── Repositories/
    │   ├── UsuarioRepository.cs
    │   ├── VanRepository.cs
    │   ├── ViagemRepository.cs
    │   ├── ViagemVanRepository.cs
    │   ├── ReservaRepository.cs
    │   └── UnitOfWork.cs
    ├── Services/
    │   ├── TokenService.cs
    │   ├── EmailService.cs
    │   ├── MercadoPagoPagamentoGateway.cs
    │   └── MercadoPagoWebhookSignatureValidator.cs
    ├── Migrations/                      # EF Core Migrations (5)
    ├── Extensions/
    │   └── ServiceCollectionExtensions.cs
    └── VanBora.Infrastructure.csproj
```
