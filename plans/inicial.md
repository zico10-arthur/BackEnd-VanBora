# VanBora — Sistema SaaS de Reserva de Vans

## 1. Visão Geral

O **VanBora** é uma plataforma **SaaS (Software as a Service)** que conecta passageiros a vans para transporte em eventos. O sistema permite que usuários reservem assentos em vans para qualquer tipo de evento (jogos, shows, passeios turísticos), com a opção de adquirir também o ingresso oficial do evento quando aplicável.

> **Propósito:** Oferecer uma solução completa de transporte + ingresso para eventos, onde gerentes de van criam suas viagens e usuários reservam assentos — tudo em um único lugar.

---

## 2. Modelo de Negócio (SaaS)

| Característica | Detalhe |
|----------------|---------|
| 🏢 **Modelo** | Multi-tenant — cada gerente de van é um inquilino independente |
| 💰 **Receita** | Taxa por reserva (comissão) |
| 🆓 **Primeiros clientes** | Isentos de taxa (0800) |
| 👥 **Público** | Qualquer tipo de evento: jogos, shows, passeios turísticos |

---

## 3. Atores do Sistema

| Ator | Descrição |
|------|-----------|
| **👤 Usuário** | Pessoa física identificada por **CPF único**. Uma mesma pessoa pode ter **múltiplos perfis** no sistema (Passageiro, Gerente, Motorista, Admin) |
| **👤 Passageiro (Perfil)** | Perfil que permite reservar assentos em viagens. Um usuário pode ter este perfil |
| **👨‍💼 Gerente (Perfil)** | Perfil de tenant — responsável por criar viagens, gerenciar vans, definir preços. Cada gerente opera **independentemente** (multi-tenant) |
| **🔧 Motorista (Perfil)** | Perfil cadastrado por um Gerente, sem login próprio. Alocado nas viagens |
| **🔧 Administrador (Perfil)** | Perfil de admin do sistema, criado diretamente no banco de dados |

> **Modelo unificado:** Um **Usuario** (CPF único) pode ter múltiplos **Perfis**. Ex: João é Gerente de van para jogos do Flamengo E também pode ser Passageiro para ir a shows — ambos com o mesmo CPF.

---

## 4. Conceitos de Negócio

### 4.1. Tenant (Inquilino)
Cada **gerente de van** ou **empresa de transporte** é um tenant no sistema. Cada tenant:
- Gerencia suas próprias vans, viagens e preços
- Tem seu próprio painel administrativo
- Não enxerga os dados de outros tenants

### 4.2. Van
Veículo utilizado para o transporte. Cada van possui:
- Capacidade total de assentos
- Identificação (placa, modelo, etc.)
- Tenant proprietário

### 4.3. Viagem (Trip)
Rota programada para uma data/hora específica. Exemplos:

> **"Flamengo x Vasco — 15/06/2026 às 16:00"**
> **"Rock in Rio — 10/09/2026 às 14:00"**
> **"Tour Costa Verde — 20/07/2026 às 08:00"**

Cada viagem está associada a:
- Uma van específica
- Um evento (nome, data, local)
- Data e horário de partida
- Preço do assento (definido pelo gerente)
- Quantidade de ingressos oficiais disponíveis (comprados pelo gerente fora do sistema)
- Preço do ingresso (definido pelo gerente, quando aplicável)

### 4.4. Assento
Unidade individual dentro da van. O usuário pode reservar **um ou mais assentos** por reserva.

### 4.5. Reserva
Registro da intenção do usuário de ocupar assentos em uma viagem.

**Características:**

| Característica | Detalhe |
|----------------|---------|
| 👤 **Responsável** | Usuário logado que cria a reserva |
| 🪑 **Múltiplos assentos** | Pode conter 1 ou mais assentos |
| 🎫 **Ingresso opcional** | Cada assento pode ser: somente assento OU assento + ingresso |
| 🔀 **Mistura permitida** | Em uma reserva com 3 assentos: 2 com ingresso, 1 sem |
| 👥 **Passageiros** | Apenas o responsável precisa ter conta; os demais passageiros informam: **CPF, Nome, Telefone e Email** |

### 4.6. Ingresso (Ticket)
Ingresso oficial do evento. **O VanBora não vende ingressos diretamente** — o gerente da van compra os ingressos do clube/organizador fora do sistema e disponibiliza uma quantidade na plataforma.

**Fluxo do ingresso:**

```
Gerente compra ingressos do clube → Define preço e quantidade no sistema →
Usuário reserva assento + ingresso → Paga → Recebe link para Face ID →
Vai ao estádio e passa pelo Face ID para entrar
```

**Regras importantes:**

| Regra | Descrição |
|-------|-----------|
| 🚫 Ingresso sem reserva | **Não existe.** Todo ingresso está vinculado a uma reserva |
| 🛒 Origem | O gerente da van compra os ingressos do clube **fora do sistema** |
| 💰 Preço | Definido pelo gerente da van no sistema |
| 📦 Estoque | O gerente informa quantos ingressos comprou; o sistema controla quantos foram vendidos |
| 🖥️ Face ID | O gerente **não cadastra ingresso no sistema**. O usuário recebe um link para fazer Face ID no site oficial do clube |
| 🏟️ Entrada | No estádio, o usuário passa pelo **Face ID** para entrar — o ingresso está vinculado à biometria |

### 4.7. Pagamento

Processado dentro da plataforma VanBora via **QR Code (Pix)**.

**Fluxo de pagamento:**

```
Reserva criada → QR Code gerado → Usuário paga → Confirmação →
    ↓ Se assento + ingresso: link do ingresso enviado por email
    ↓ Se somente assento: confirmação de reserva enviada por email
```

---

## 5. Fluxos Principais

### 5.1. Fluxo de Cadastro (Usuario + Perfil)

```mermaid
flowchart TD
    A[Usuário acessa o sistema] --> B{Pessoa já tem cadastro?}
    B -->|Não| C[Criar Usuario\nnome + CPF]
    B -->|Sim| D{Que perfil deseja?}
    C --> D
    D -->|Passageiro| E[Criar Perfil Passageiro\nemail + senha + telefone]
    D -->|Gerente| F[Criar Perfil Gerente\nemail + senha + slug + telefone]
    D -->|Motorista| G[Gerente cadastra Motorista\nbusca Usuario por CPF ou cria\n+ cria Perfil Motorista\nsem senha - não faz login]
    D -->|Admin| H[Criado diretamente no banco]
    
    E --> I[Usuario com Perfil Passageiro]
    F --> J[Usuario com Perfil Gerente]
    G --> K[Usuario com Perfil Motorista]
    
    I --> L[Mesmo CPF pode ter\nmúltiplos perfis]
    J --> L
    K --> L
```

### 5.2. Fluxo do Gerente da Van (Tenant)

```mermaid
flowchart TD
    A[Gerente faz login no painel] --> B[Cadastrar vans]
    B --> C[Criar viagem]
    C --> D[Definir van, data, preço do assento]
    D --> E{Evento tem ingresso oficial?}
    E -->|Sim| F[Comprar ingressos do clube fora do sistema]
    E -->|Não| I[Viagem publicada sem ingresso]
    F --> G[Definir preço e quantidade de ingressos no sistema]
    G --> H[Viagem publicada com opção de ingresso]
    H --> J[Viagem disponível para reservas]
    I --> J
```

### 5.3. Fluxo do Usuário (Passageiro)

```mermaid
flowchart TD
    A[Usuário faz login] --> B[Pesquisar viagens disponíveis]
    B --> C[Selecionar viagem]
    C --> D[Escolher van]
    D --> E[Informar quantidade de assentos]
    E --> F{Para cada assento}
    F --> G[Informar dados do passageiro\nCPF, Nome, Tel, Email]
    G --> H[Escolher: somente assento\nou assento + ingresso]
    H --> I{Possui ingresso?}
    I -->|Sim| J[Valor = assento + ingresso]
    I -->|Não| K[Valor = assento]
    J --> L[Revisar reserva]
    K --> L
    L --> M[Gerar QR Code Pix para pagamento]
    M --> N[Aguardar confirmação de pagamento]
    N --> O{Reserva inclui ingresso?}
    O -->|Sim| P[Enviar email com link do Face ID\nsite oficial do clube]
    O -->|Não| Q[Enviar email de confirmação da reserva]
```

### 5.4. Diagrama de Estados da Reserva

```mermaid
stateDiagram-v2
    [*] --> PendentePagamento: Reserva criada
    PendentePagamento --> Confirmada: Pagamento aprovado
    PendentePagamento --> Cancelada: Expirada / Cancelada
    Confirmada --> EmViagem: Data da viagem
    EmViagem --> Finalizada: Viagem concluída
    Confirmada --> Cancelada: Reembolso / Cancelamento
    Cancelada --> [*]
    Finalizada --> [*]
```

---

## 6. Regras de Negócio

| # | Regra |
|---|-------|
| RN01 | O sistema é **multi-tenant**: cada gerente de van opera independentemente |
| RN02 | O **gerente da van** define os preços do assento e do ingresso, e cria suas próprias viagens |
| RN03 | O VanBora ganha uma **taxa por reserva**. Os **2 primeiros gerentes** cadastrados na plataforma são **gratuitos** (taxa = 0). O Admin pode ajustar a taxa de cada gerente individualmente |
| RN04 | O **usuário precisa ter uma conta** para fazer uma reserva |
| RN05 | O usuário pode reservar **1 ou mais assentos** em uma única reserva |
| RN06 | Cada assento pode ter ou não um **ingresso** associado |
| RN07 | Em uma mesma reserva, é permitido **misturar** itens com e sem ingresso |
| RN08 | **Ingresso nunca existe sem uma reserva** — é sempre vinculado a um assento reservado |
| RN09 | Apenas o **responsável pela reserva** precisa estar logado; os demais passageiros informam **CPF, Nome, Telefone e Email** |
| RN10 | O **gerente da van** compra os ingressos do clube **fora do sistema** e informa quantidade e preço no VanBora |
| RN11 | O pagamento é processado via **QR Code Pix** dentro da plataforma VanBora |
| RN12 | O sistema atende **qualquer tipo de evento** (jogos, shows, passeios turísticos) |
| RN13 | O **gerente não cadastra ingressos individuais** no sistema. O usuário recebe um **link para Face ID** no site oficial do clube |
| RN14 | Se a reserva for **somente assento**, o usuário recebe apenas a confirmação da reserva |
| RN15 | A **capacidade da van** não pode ser alterada após a criação — é uma característica física fixa do veículo |
| RN16 | O **CPF** é único e imutável. Cada pessoa física tem **um único Usuario** no sistema. Qualquer cadastro (Passageiro, Gerente, Motorista) **reutiliza o Usuario existente** pelo CPF — nunca retorna erro de CPF duplicado. O **Slug do gerente** também é imutável |
| RN17 | A **exclusão de conta** é **soft delete** (desativação lógica). Requer **confirmação por código enviado por email**. O usuário pode desativar perfis específicos ou todos os perfis |
| RN18 | O **gerente** pode cadastrar, listar, atualizar e remover **motoristas** vinculados ao seu perfil. A remoção de motorista é **soft delete** (Ativo = false) apenas se ele **não estiver alocado em nenhuma ViagemVan futura**; caso contrário, retorna erro 422 |
| RN19 | O **passageiro tem 10 minutos** para efetuar o pagamento da reserva após criá-la. Após esse prazo, a reserva expira automaticamente e os assentos são liberados |
| RN20 | O **gerente pode cancelar** suas próprias viagens a qualquer momento. Se a viagem tiver **reservas confirmadas**, todas devem ser **reembolsadas integralmente via Pix (automático)** e o status alterado para "Cancelada" |
| RN21 | Ao **remover uma van de uma viagem**, se a van tiver **reservas confirmadas**, todas devem ser **reembolsadas integralmente via Pix (automático)** antes da desalocação |
| RN22 | Um **Usuario** pode ter **múltiplos Perfis** (Passageiro, Gerente, Motorista, Admin) associados ao mesmo CPF |
| RN23 | O **Motorista não possui login** — é cadastrado pelo Gerente e não tem email/senha para acesso ao sistema |
| RN24 | Email é único **por Perfil**, não por Usuario. Um Gerente e um Passageiro (mesmo Usuario) podem ter emails diferentes |

---

## 7. Premissas Técnicas

- **Arquitetura:** Clean Architecture (.NET 9) — já iniciada
- **API:** RESTful com ASP.NET Core
- **Multi-tenant:** Isolamento por Tenant (database ou schema)
- **Banco de Dados:** **PostgreSQL**
- **ORM:** **Entity Framework Core**
- **Pagamento:** Integração com gateway Pix (QR Code)
- **Email:** Serviço de envio de emails transacionais
- **Autenticação:** JWT com claims de perfis

---

## 8. Glossário

| Termo | Significado |
|-------|-------------|
| **SaaS** | Software as a Service — modelo de assinatura/software sob demanda |
| **Tenant** | Inquilino — cada gerente/empresa de van no sistema |
| **Multi-tenant** | Múltiplos inquilinos isolados na mesma plataforma |
| **Usuario** | Entidade base — pessoa física identificada por CPF único |
| **Perfil** | Papel que um Usuario pode ter (Passageiro, Gerente, Motorista, Admin) |
| **Passageiro** | Perfil de usuário final que reserva assentos |
| **0800** | Gratuito, sem custo |
| **Face ID** | Autenticação biométrica para acesso ao estádio/evento |
| **QR Code** | Código para pagamento via Pix |

---

## 9. Próximos Passos

1. ✅ Documento base criado e revisado
2. ✅ Modelo Usuario + Perfil definido
3. ⬜ Detalhar entidades de domínio (Domain layer)
4. ⬜ Mapear relacionamentos entre entidades
5. ⬜ Definir endpoints da API
6. ⬜ Criar plano de implementação com tasks detalhadas
