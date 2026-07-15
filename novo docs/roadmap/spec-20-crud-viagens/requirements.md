---
name: Gerenciamento de Viagens
status: auditada
references:
  - visão.md: Item 6 — Viagens
  - roadmap.md: Spec 20
  - arch.md: ViagemController (api/gerente/viagens), Viagem entity, ViagemService
---

# Spec 20 — Gerenciamento de Viagens

## O que é?

Conjunto de telas que permitem ao **Vendedor (Gerente/Frotista)** criar, visualizar, editar e cancelar viagens — que são os "anúncios" que os compradores veem na plataforma.

## Problema que resolve para o usuário final

O gerente precisa **publicar viagens** para que compradores encontrem e reservem assentos. Sem essa tela, o produto inteiro não funciona — é o core do marketplace. A tela mock atual (`/motorista/nova-viagem`) usa dados falsos de jogos do Botafogo e não se comunica com o backend. Esta spec substitui o mock por uma tela real integrada.

---

## Functional Requirements

### FR-001 — Listar viagens do gerente

**Descrição:** Tela com tabela listando todas as viagens do gerente autenticado.

**Critérios de Aceite:**
- [ ] Colunas da tabela: Evento, Data/Hora Partida, Alocação (van/motorista), Assentos (vendidos/capacidade), Status, Ações
- [ ] Ordenação padrão: data de partida (mais próxima primeiro)
- [ ] Cada linha tem ações: `Ver` (detalhe/resumo), `Editar` (se status = Agendada), `Cancelar` (se status = Agendada)
- [ ] Status exibido como badge colorido: Agendada = verde, Cancelada = vermelho, Concluída = cinza, EmAndamento = azul
- [ ] Coluna "Alocação": se houver ViagemVan vinculada → "Van {modelo} — Motorista {nome}". Se não houver → badge "Pendente alocação" (amarelo)

### FR-002 — Criar viagem

**Descrição:** Formulário para o gerente publicar uma nova viagem.

**Critérios de Aceite:**
- [ ] Campos do formulário:
  - `nomeEvento` (text, obrigatório, máx 150 caracteres)
  - `dataEvento` (datetime-local, obrigatório, data futura)
  - `localEvento` (text, obrigatório, máx 200 caracteres)
  - `origemDescricao` (text, obrigatório, máx 200 caracteres)
  - `origemCidade` (text, obrigatório, máx 100 caracteres)
  - `origemEstado` (text, obrigatório, 2 caracteres, UF válida)
  - `destinoDescricao` (text, obrigatório, máx 200 caracteres)
  - `destinoCidade` (text, obrigatório, máx 100 caracteres)
  - `destinoEstado` (text, obrigatório, 2 caracteres, UF válida)
  - `dataSaida` (datetime-local, obrigatório, data futura)
  - `dataChegada` (datetime-local, obrigatório, posterior a dataSaida)
  - `precoAssento` (number, obrigatório, > 0, step 0.01, exibido com prefixo R$)
  - `possuiIngresso` (checkbox, default false)
- [ ] Validação client-side antes do submit:
  - `dataSaida` > agora (horário local do navegador)
  - `dataChegada` > `dataSaida`
  - `dataEvento` no futuro
  - Origem e destino NÃO são iguais: erro se `origemCidade === destinoCidade && origemEstado === destinoEstado`
  - Todos os campos obrigatórios preenchidos
  - `precoAssento` > 0
- [ ] Ao submeter com sucesso → `POST /api/gerente/viagens` → redirecionar para `/gerente/viagens?sucesso=criada`
- [ ] Botão submit desabilitado durante requisição, texto muda para "Salvando…"

### FR-003 — Editar viagem

**Descrição:** Mesmo formulário do FR-002, pré-preenchido com os dados atuais da viagem.

**Critérios de Aceite:**
- [ ] Apenas viagens com status "Agendada" podem ser editadas
- [ ] O botão "Editar" na listagem fica **desabilitado** (cinza, cursor not-allowed) se status ≠ "Agendada"
- [ ] Ao carregar página de edição, se status não for "Agendada", redirecionar para a lista com toast de erro
- [ ] Chamar `PUT /api/gerente/viagens/{id}`
- [ ] Sucesso: redirecionar para `/gerente/viagens?sucesso=editada`
- [ ] Campos são exatamente os mesmos do FR-002; todos são editáveis

### FR-004 — Cancelar viagem

**Descrição:** Permitir que o gerente cancele uma viagem agendada.

**Critérios de Aceite:**
- [ ] O botão "Cancelar" na listagem fica **desabilitado** se status ≠ "Agendada"
- [ ] Ao clicar, abre modal de confirmação: "Tem certeza que deseja cancelar esta viagem? Passageiros com reserva serão notificados."
- [ ] Modal tem dois botões: "Não, manter viagem" (fecha modal) e "Sim, cancelar viagem" (executa ação)
- [ ] Ao confirmar → `POST /api/gerente/viagens/{id}/cancelar`
- [ ] Se cancelamento bem-sucedido: fecha modal, recarrega a lista, exibe toast "Viagem cancelada com sucesso."
- [ ] Se erro: exibe mensagem de erro dentro do modal

### FR-005 — Estados visuais e feedback

**Descrição:** A interface deve comunicar claramente cada estado ao usuário.

**Critérios de Aceite:**
- [ ] **Loading (lista):** 3 linhas skeleton com `animate-pulse` enquanto `GET /api/gerente/viagens` está em andamento
- [ ] **Empty (lista):** Ilustração ou ícone + texto "Nenhuma viagem cadastrada" + botão "Criar primeira viagem"
- [ ] **Error (lista):** Mensagem de erro em texto vermelho + botão "Tentar novamente"
- [ ] **Loading (formulário):** Botão submit mostra "Salvando…" e fica desabilitado
- [ ] **Error (formulário):** Mensagem de erro da API abaixo do formulário; erros de validação inline abaixo de cada campo com borda vermelha
- [ ] **Success:** Toast verde com mensagem descritiva, duração 5 segundos com botão de fechar (X), posição fixa no canto superior direito da viewport. Após dismiss, limpa parâmetro de URL
- [ ] **Toast de erro:** Fundo vermelho, mesma posição e duração

### FR-006 — Proteção de rota

**Descrição:** Apenas usuários autenticados com perfil "Gerente" podem acessar as páginas.

**Critérios de Aceite:**
- [ ] Todas as rotas usam `GerenteGuard` (componente cliente que verifica JWT + claim `tipos` contendo "Gerente")
- [ ] Se não autenticado → redireciona para `/entrar`
- [ ] Se autenticado mas não Gerente → redireciona para `/` com mensagem de acesso negado
- [ ] O guard exibe estado de loading enquanto verifica o token

---

## Non-Functional Requirements

### NFR-001 — Performance
- [ ] A lista de viagens deve exibir o primeiro conteúdo (skeleton) em até 2 segundos após a navegação
- [ ] O formulário deve responder ao input do usuário sem atraso perceptível (< 100ms)

### NFR-002 — Acessibilidade
- [ ] Todos os inputs têm `<label>` associado por `htmlFor`
- [ ] Botões de ação têm `aria-label` descritivo (ex: "Editar viagem para {evento}")
- [ ] Modal de cancelamento captura foco, permite fechar com Esc e fecha ao clicar fora

### NFR-003 — Segurança
- [ ] JWT enviado via header `Authorization: Bearer` em todas as chamadas autenticadas
- [ ] Se a API retornar 401, redirecionar para `/entrar` (token expirado/inválido)
- [ ] Campos de texto previnem XSS (React faz escaping por padrão; confiar no JSX)

### NFR-004 — Consistência visual
- [ ] Usar os mesmos componentes base do restante do portal do gerente: `Header`, `VbButton`, `ToastBanner`, `GerenteGuard`
- [ ] Estilo dark theme (zinc-900/800/700) com accent amber (`van-amber`), igual às telas de Vans e Motoristas

---

## Edge Cases

| ID | Cenário | Comportamento esperado |
|----|---------|------------------------|
| EC-01 | API retorna 401 (token expirado) | `GerenteGuard` detecta falta de token válido e redireciona para `/entrar` |
| EC-02 | API retorna 403 (não é Gerente) | `GerenteGuard` redireciona para `/` |
| EC-03 | API retorna 404 ao editar (viagem não encontrada ou não pertence ao gerente) | Exibir toast de erro "Viagem não encontrada" e redirecionar para a lista |
| EC-04 | API retorna 409 (dados duplicados/viagem já cancelada) | Exibir mensagem de erro específica no formulário ou toast |
| EC-05 | API retorna 500 | Exibir toast "Erro interno. Tente novamente mais tarde." |
| EC-06 | Rede falha (fetch lança TypeError) | Exibir estado de erro com botão "Tentar novamente" |
| EC-07 | Usuário tenta editar viagem já cancelada/concluída | Acessa diretamente a URL `/gerente/viagens/{id}/editar` → página detecta status ≠ Agendada e redireciona para lista com toast |
| EC-08 | Usuário tenta cancelar viagem já cancelada | Botão "Cancelar" desabilitado na lista; se tentar via POST direto, API retorna 409 |
| EC-09 | Formulário enviado com campos vazios | Validação client-side bloqueia submit; cada campo inválido mostra erro inline |
| EC-10 | `dataSaida` igual a `dataChegada` | Validação client-side bloqueia: erro "Data de chegada deve ser posterior à data de saída" |
| EC-11 | `precoAssento` <= 0 | Validação client-side bloqueia: erro "Preço deve ser maior que zero" |
| EC-12 | Origem e destino exatamente iguais (mesma cidade + estado) | Validação client-side bloqueia: erro "Origem e destino não podem ser iguais" |
| EC-13 | Lista vazia — gerente acabou de se cadastrar | Exibir estado empty com CTA "Criar primeira viagem" |
| EC-14 | Múltiplos toasts simultâneos | Apenas o último toast é exibido; o anterior é substituído (comportamento do ToastBanner) |
| EC-15 | Campos com caracteres especiais (ex: aspas, <, >) | React faz escaping automático; o valor chega ao backend em UTF-8 via JSON |

---

## Endpoints consumidos

| Método | Rota | Uso | Autenticação |
|--------|------|-----|--------------|
| GET | `/api/gerente/viagens` | Listar viagens do gerente | JWT (Gerente) |
| POST | `/api/gerente/viagens` | Criar nova viagem | JWT (Gerente) |
| PUT | `/api/gerente/viagens/{id}` | Editar viagem existente | JWT (Gerente) |
| POST | `/api/gerente/viagens/{id}/cancelar` | Cancelar viagem | JWT (Gerente) |

---

## Dependências entre Specs

| Spec | Relação | Impacto |
|------|---------|---------|
| Spec 30 — Vans | **auditada** | Necessária para alocar van (Spec 50), não bloqueia CRUD de viagens |
| Spec 40 — Motoristas | **pendente** | Necessária para alocar motorista (Spec 50), não bloqueia CRUD de viagens |
| Spec 50 — Alocação | **pendente** | Redirecionamento pós-criação depende da rota `/gerente/viagens/{id}/alocar` (Spec 50) |
| Spec 60 — Relatório | **pendente** | O botão "Ver" na listagem redireciona para relatório (Spec 60) |

---

## Fora do escopo

- Alocar van/motorista à viagem (Spec 50)
- Relatório financeiro da viagem (Spec 60)
- Lista de embarque (Spec 60)
- Dashboard com visão consolidada (Spec 10)
