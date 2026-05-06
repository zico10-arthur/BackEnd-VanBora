# 📋 Relatório Completo — VanBora

> **Data:** 06/05/2026
> **Documentos de referência:** [`plans/inicial.md`](plans/inicial.md), [`plans/technical-plan.md`](plans/technical-plan.md), [`plans/user-stories.md`](plans/user-stories.md)
> **Total de User Stories:** 29 (US01–US29, sem US27)

---

## Índice

- [📋 Relatório Completo — VanBora](#-relatório-completo--vanbora)
  - [Índice](#índice)
  - [1. Resumo Executivo](#1-resumo-executivo)
    - [Arquitetura](#arquitetura)
  - [2. Modelo de Negócio](#2-modelo-de-negócio)
  - [3. Atores do Sistema (Usuario + Perfil)](#3-atores-do-sistema-usuario--perfil)
  - [4. Regras de Negócio (30 RNs)](#4-regras-de-negócio-30-rns)
  - [5. Entidades de Domínio](#5-entidades-de-domínio)
    - [5.1. Usuario (pessoa física)](#51-usuario-pessoa-física)
    - [5.2. Perfil (papel do Usuario)](#52-perfil-papel-do-usuario)
    - [5.3. Van](#53-van)
    - [5.4. Viagem (Trip)](#54-viagem-trip)
    - [5.5. ViagemVan (junction)](#55-viagemvan-junction)
    - [5.6. Perfil Motorista](#56-perfil-motorista)
    - [5.7. Reserva](#57-reserva)
    - [5.8. ItemReserva](#58-itemreserva)
  - [6. Value Objects](#6-value-objects)
  - [7. Enums](#7-enums)
  - [8. Endpoints da API (45+)](#8-endpoints-da-api-45)
    - [8.1. Autenticação e Perfil](#81-autenticação-e-perfil)
    - [8.2. Viagens (Público)](#82-viagens-público)
    - [8.3. Gerente — Vans](#83-gerente--vans)
    - [8.4. Gerente — Motoristas](#84-gerente--motoristas)
    - [8.5. Gerente — Viagens](#85-gerente--viagens)
    - [8.6. Reservas](#86-reservas)
    - [8.7. Gerente — Ingressos](#87-gerente--ingressos)
    - [8.8. Admin](#88-admin)
  - [9. User Stories (29)](#9-user-stories-29)
  - [10. Estrutura de Projeto (Clean Architecture)](#10-estrutura-de-projeto-clean-architecture)
  - [11. Plano de Implementação (5 Fases)](#11-plano-de-implementação-5-fases)
    - [Fase 1 — Setup e Domain](#fase-1--setup-e-domain)
    - [Fase 2 — Infraestrutura](#fase-2--infraestrutura)
    - [Fase 3 — Application](#fase-3--application)
    - [Fase 4 — API](#fase-4--api)
    - [Fase 5 — Integrações e Testes](#fase-5--integrações-e-testes)

---

## 1. Resumo Executivo

O **VanBora** é uma plataforma **SaaS multi-tenant** que conecta passageiros a vans para transporte em eventos (jogos, shows, passeios turísticos). Cada **Gerente** opera independentemente como um inquilino, criando viagens e gerenciando vans. Os usuários reservam assentos, com opção de solicitar que o gerente compre ingressos oficiais dos eventos.

### Arquitetura

- **Stack:** .NET 9, ASP.NET Core, PostgreSQL, Entity Framework Core
- **Padrão:** Clean Architecture (4 camadas: Api, Application, Domain, Infrastructure)
- **Autenticação:** Login único (email + senha do Usuario). JWT com claims (`sub`, `email`, `perfis[]`, `nome`). Operações de gerente usam header `X-Perfil-Id`
- **Pagamento:** Assento → Pix VanBora (QR Code). Ingresso → Pix direto ao gerente (fora da plataforma)
- **Modelo de dados:** Usuario (CPF único, Email + SenhaHash) → N Perfis (Passageiro, Gerente, Motorista, Admin)

---

## 2. Modelo de Negócio

| Característica | Detalhe |
|---|---|
| 🏢 **Modelo** | Multi-tenant — cada gerente é um inquilino independente |
| 💰 **Receita** | Taxa por reserva (comissão percentual) |
| 🆓 **Primeiros clientes** | 2 primeiros gerentes cadastrados são gratuitos (taxa = 0) |
| 👥 **Público** | Qualquer tipo de evento: jogos, shows, passeios turísticos |
| 📱 **Pagamento assento** | QR Code Pix (processado pelo VanBora) |
| 💸 **Pagamento ingresso** | Pix direto passageiro → gerente (fora da plataforma) |
| 🎫 **Ingressos** | VanBora apenas facilita a solicitação. Gerente compra no portal do evento |

---

## 3. Atores do Sistema (Usuario + Perfil)

O modelo unificado **Usuario + Perfil** é a base do sistema:

```
┌──────────────────────────────────────────────────────────┐
│                      USUARIO                             │
│          (Pessoa Física, CPF Único)                      │
│          Email + SenhaHash (login único)                  │
│                                                          │
│  ┌─────────────┬──────────────┬──────────────────────┐   │
│  │  Perfil     │  Perfil      │  Perfil              │   │
│  │  Passageiro │  Gerente     │  Motorista           │   │
│  │  automático │  slug, taxa  │  CNH, sem login      │   │
│  │  faz reserva│  cria viagem │  ativável depois     │   │
│  └─────────────┴──────────────┴──────────────────────┘   │
└──────────────────────────────────────────────────────────┘
```

| Ator | Descrição |
|---|---|
| **👤 Usuário** | Pessoa física identificada por **CPF único**. Possui **Email** e **Senha** para login único. Pode ter **múltiplos perfis** |
| **👤 Passageiro (Perfil)** | Perfil padrão criado automaticamente ao registrar um Usuario. Permite reservar assentos |
| **👨‍💼 Gerente (Perfil)** | Perfil de tenant. Cria viagens, gerencia vans, define preços. Também pode reservar assentos |
| **🔧 Motorista (Perfil)** | Perfil sem login inicial (SenhaHash = null). Cadastrado pelo Gerente. Pode ativar conta registrando-se como Passageiro (mesmo CPF) |
| **🔧 Admin (Perfil)** | Perfil de administrador do sistema, criado diretamente no banco |

> **Login único:** O Usuario possui um único email e senha. Todos os perfis compartilham o mesmo login. **Qualquer usuário logado pode reservar assentos** (Passageiro, Gerente, Admin).

---

## 4. Regras de Negócio (30 RNs)

| # | Regra | Status |
|---|-------|--------|
| RN01 | O sistema é **multi-tenant**: cada gerente de van opera independentemente | ✅ |
| RN02 | O **gerente da van** define os preços do assento e do ingresso, e cria suas próprias viagens | ✅ |
| RN03 | O VanBora ganha uma **taxa por reserva**. Os **2 primeiros gerentes** cadastrados na plataforma são **gratuitos** (taxa = 0). O Admin pode ajustar a taxa de cada gerente individualmente | ✅ |
| RN04 | O **usuário precisa ter uma conta** para fazer uma reserva | ✅ |
| RN05 | O usuário pode reservar **1 ou mais assentos** em uma única reserva | ✅ |
| RN06 | Cada assento pode ter ou não um **ingresso** associado. A solicitação de ingresso ocorre **após** o pagamento do assento, em fluxo separado | ✅ |
| RN07 | Em uma mesma reserva, é permitido **misturar** itens com e sem ingresso solicitado | ✅ |
| RN08 | **Ingresso nunca existe sem uma reserva** — é sempre vinculado a um ItemReserva | ✅ |
| RN09 | Apenas o **responsável pela reserva** precisa estar logado; os demais passageiros informam **CPF, Nome, Telefone e Email** | ✅ |
| RN10 | O **passageiro autoriza o gerente** a comprar o ingresso em seu nome. O **gerente compra o ingresso APÓS receber o pagamento do passageiro**, informando o email do passageiro no portal do evento. O **portal do evento envia o ingresso automaticamente** por email | ✅ |
| RN11 | O pagamento do **assento** é processado via **QR Code Pix** pela plataforma VanBora. O pagamento do **ingresso** é feito **diretamente ao gerente** (fora da plataforma) | ✅ |
| RN12 | O sistema atende **qualquer tipo de evento** (jogos, shows, passeios turísticos) | ✅ |
| RN13 | O passageiro **autoriza** o cadastro do Face ID durante a tela de autorização do ingresso (checkbox 3), mas quem cadastra é o **próprio passageiro no portal do evento**. Após o gerente comprar o ingresso, o portal do evento envia o ingresso por email junto com instruções para cadastro do Face ID | ✅ |
| RN14 | Se a reserva for **somente assento**, o usuário recebe apenas a confirmação da reserva por email | ✅ |
| RN15 | A **capacidade da van** não pode ser alterada após a criação — é uma característica física fixa do veículo | ✅ |
| RN16 | O **CPF** é único e imutável. Cada pessoa física tem **um único Usuario** no sistema. Qualquer cadastro (Passageiro, Gerente, Motorista) **reutiliza o Usuario existente** pelo CPF — nunca retorna erro de CPF duplicado. O **Slug do gerente** também é imutável | ✅ |
| RN17 | A **exclusão de conta** é **soft delete** (desativação lógica). Requer **confirmação por código enviado por email**. O usuário pode desativar o **Usuario** (impede login) ou apenas **perfis específicos** (ex: desativar Gerente mas manter Passageiro ativo) | ✅ |
| RN18 | O **gerente** pode cadastrar, listar, atualizar e remover **motoristas** vinculados ao seu perfil. A remoção de motorista é **soft delete** (Ativo = false) apenas se ele **não estiver alocado em nenhuma ViagemVan futura**; caso contrário, retorna erro 422 | ✅ |
| RN19 | O **passageiro tem 10 minutos** para efetuar o pagamento da reserva após criá-la. Após esse prazo, a reserva expira automaticamente e os assentos são liberados | ✅ |
| RN20 | O **gerente pode cancelar** suas próprias viagens a qualquer momento. Se a viagem tiver **reservas confirmadas**, todas devem ser **reembolsadas integralmente via Pix (automático)** e o status alterado para "Cancelada" | ✅ |
| RN21 | Ao **remover uma van de uma viagem**, se a van tiver **reservas confirmadas**, todas devem ser **reembolsadas integralmente via Pix (automático)** antes da desalocação | ✅ |
| RN22 | Um **Usuario** pode ter **múltiplos Perfis** (Passageiro, Gerente, Motorista, Admin) associados ao mesmo CPF | ✅ |
| RN23 | O **Motorista não possui login inicialmente** — é cadastrado pelo Gerente com `SenhaHash = null`. O Motorista pode depois **ativar a conta** registrando-se como Passageiro com o mesmo CPF (define email e senha), ganhando acesso ao sistema e podendo reservar assentos | ✅ |
| RN24 | Email é único **no Usuario**. Login é feito com email + senha do Usuario. Diferente do modelo anterior, não existe mais email por Perfil | ✅ |
| RN25 | A **opção de solicitar ingresso** só aparece **após o pagamento do assento ser confirmado**. Enquanto a reserva estiver "PendentePagamento", a opção não fica disponível | ✅ |
| RN26 | O **número máximo de ingressos** que o passageiro pode solicitar é igual ao **número de assentos na reserva**. Ex: reservou 4 assentos → pode pedir até 4 ingressos | ✅ |
| RN27 | A **solicitação de ingresso** exige que o passageiro marque **3 checkboxes** de autorização: autorizar gerente a comprar, concordar com a não-devolução após recebimento, e autorizar cadastro de Face ID | ✅ |
| RN28 | Após o **ingresso ser recebido por email**, **não há direito ao reembolso** do ingresso. O reembolso do assento permanece normal (via VanBora) | ✅ |
| RN29 | O **gerente tem 24 horas** (ou prazo definido na viagem) para comprar o ingresso após receber a solicitação + pagamento do passageiro; caso não compre no prazo, o valor do ingresso deve ser **reembolsado ao passageiro** | ✅ |
| RN30 | O **VanBora não se responsabiliza** pelo ingresso — a transação é entre passageiro e gerente. O VanBora apenas facilita a solicitação, autorização e notificação | ✅ |

---

## 5. Entidades de Domínio

### 5.1. Usuario (pessoa física)

| Propriedade | Tipo | Descrição |
|---|---|---|
| Id | Guid | PK |
| Nome | string | Nome completo |
| CPF | CPF (VO) | Único, imutável |
| Email | Email (VO) | Único. Usado para login |
| SenhaHash | string | Hash da senha (nullable para Motoristas sem login) |
| Telefone | Telefone (VO)? | Opcional |
| Ativo | bool | Se o Usuario está ativo (desativar impede login) |
| CriadoEm | DateTime | Data de criação |

### 5.2. Perfil (papel do Usuario)

| Propriedade | Tipo | Descrição |
|---|---|---|
| Id | Guid | PK |
| UsuarioId | Guid | FK → Usuario |
| Tipo | TipoPerfil | Passageiro, Gerente, Motorista, Admin |
| Ativo | bool | Se o perfil está ativo |
| CriadoPorPerfilId | Guid? | FK → Perfil (Gerente que cadastrou, apenas Motorista) |
| CriadoEm | DateTime | Data de criação |

> **Perfil Passageiro** não possui campos específicos — usa apenas os dados do Usuario (Nome, CPF, Email, Telefone).

**Campos específicos por Tipo:**

| Campo | Gerente | Motorista | Admin |
|---|---|---|---|
| Slug | ✅ (único) | ❌ | ❌ |
| TaxaPlataforma | ✅ (%) | ❌ | ❌ |
| Gratuito | ✅ (bool) | ❌ | ❌ |
| CNH | ❌ | ✅ | ❌ |

> **Nota:** O **login** (Email + Senha) está no **Usuario**, não no Perfil. Slug, TaxaPlataforma e Gratuito são específicos do Gerente. CNH é específica do Motorista.

### 5.3. Van

| Propriedade | Tipo |
|---|---|
| Id | Guid PK |
| GerentePerfilId | Guid FK → Perfil (Tipo=Gerente) |
| Nome, Placa (VO), Modelo | string |
| Capacidade | int (ex: 16 = 15 assentos + motorista) |
| Ativo, CriadoEm | bool, DateTime |

### 5.4. Viagem (Trip)

| Propriedade | Tipo |
|---|---|
| Id | Guid PK |
| GerentePerfilId | Guid FK → Perfil (Gerente) |
| NomeEvento, LocalEvento, LocalPartida | string |
| DataEvento, DataPartida | DateTime |
| PrecoAssento | decimal |
| PossuiIngresso | bool |
| PrecoIngresso | decimal? |
| PrazoCompraIngresso | int (padrão: 24h) |
| Status | StatusViagem (Agendada, EmAndamento, Concluida, Cancelada) |
| CriadoEm | DateTime |

### 5.5. ViagemVan (junction)

| Propriedade | Tipo |
|---|---|
| Id | Guid PK |
| ViagemId | Guid FK → Viagem |
| VanId | Guid FK → Van |
| MotoristaPerfilId | Guid? FK → Perfil (Motorista) |
| IngressosDisponiveis | int |

> **Assentos Virtuais:** A capacidade é derivada de `Van.Capacidade - 1`. Disponível é calculado subtraindo `ItemReserva.NumeroAssento` já registrados.

### 5.6. Perfil Motorista

| Propriedade | Específica do Perfil | Descrição |
|---|---|---|
| CNH | string | Número da CNH |
| Ativo | bool | Se ainda trabalha com o gerente |
| CriadoPorPerfilId | Guid | FK → Perfil.Id do Gerente que cadastrou |

> O Motorista é criado com `Email = null` e `SenhaHash = null` no Usuario — **não possui login**. Pode ativar conta registrando-se como Passageiro com o mesmo CPF.

### 5.7. Reserva

| Propriedade | Tipo |
|---|---|
| Id | Guid PK |
| UsuarioId | Guid FK → Usuario (responsável) |
| ViagemVanId | Guid FK → ViagemVan |
| Status | StatusReserva |
| ValorTotal, TaxaPlataforma | decimal |
| CodigoPix, TransacaoId | string, string? |
| PagoEm, CriadoEm, ExpiraEm | DateTime? |

### 5.8. ItemReserva

| Propriedade | Tipo | Descrição |
|---|---|---|
| Id | Guid | PK |
| ReservaId | Guid | FK → Reserva |
| NumeroAssento | int | Número do assento |
| PrecoAssento | decimal | Snapshot no momento da reserva |
| NomePassageiro, EmailPassageiro, TelefonePassageiro, CPFPassageiro | string | Dados do passageiro |
| **Campos de Ingresso** | | |
| PossuiIngresso | bool | Se solicitou ingresso |
| PrecoIngresso | decimal? | Snapshot no momento da solicitação |
| StatusTicket | StatusTicket | NaoSolicitado, AguardandoPagamento, PagoGerente, EmCompra, Comprado, Entregue, Reembolsado |
| AutorizadoGerenteCompra | bool | Checkbox 1 |
| ConsentimentoSemReembolso | bool | Checkbox 2 |
| ConsentimentoFaceId | bool | Checkbox 3 — apenas autorização. Cadastro é no portal do evento |
| EmailParaIngresso | string? | Email para receber o ingresso |
| SolicitadoEm | DateTime? | |
| PagoGerenteEm | DateTime? | |
| CompradoEm | DateTime? | |
| EntregueEm | DateTime? | |

> **Nota:** O pagamento do ingresso é feito **diretamente ao gerente** (fora do VanBora). O campo `ConsentimentoFaceId` registra apenas a **autorização** — o cadastro do Face ID é feito pelo passageiro no portal do evento.

---

## 6. Value Objects

Definidos em `VanBora.Domain/ValueObjects/`:

| VO | Propriedades | Validações |
|---|---|---|
| **Email** | Valor: string | Formato email |
| **CPF** | Valor: string (11 dígitos) | Dígitos verificadores |
| **Telefone** | DDD, Numero, ValorCompleto | DDD 2 dígitos, número 8-9 dígitos |
| **Placa** | Valor: string | Formato Mercosul (ABC1D23) |
| **Dinheiro** | Valor: decimal, Moeda: string | Não negativo, 2 casas, operações Somar/Subtrair/Multiplicar/Percentual |

---

## 7. Enums

Definidos em `VanBora.Domain/Enums/`:

```csharp
public enum TipoPerfil { Passageiro, Gerente, Motorista, Admin }
public enum StatusViagem { Agendada, EmAndamento, Concluida, Cancelada }
public enum StatusReserva { PendentePagamento, Confirmada, EmAndamento, Concluida, Cancelada, Expirada }
public enum StatusTicket { NaoSolicitado, AguardandoPagamento, PagoGerente, EmCompra, Comprado, Entregue, Reembolsado }
```

---

## 8. Endpoints da API (45+)

### 8.1. Autenticação e Perfil

| Método | Rota | US |
|---|---|---|
| `POST` | `/api/auth/registrar` | US03 — Cadastro (Usuario + Passageiro) |
| `POST` | `/api/auth/login` | US04 — Login único |
| `POST` | `/api/auth/gerente/registrar` | US01 — Cadastro Gerente (reutiliza Usuario se CPF existir) |
| `GET` | `/api/auth/me` | — Dados do usuario + perfis |
| `PUT` | `/api/auth/usuario` | US18 — Atualizar dados do Usuario |
| `PUT` | `/api/auth/perfil/gerente` | US19 — Atualizar slug do Gerente |
| `POST` | `/api/auth/alterar-senha` | US21 — Alterar senha |
| `POST` | `/api/auth/solicitar-exclusao` | US20 — Solicitar exclusão |
| `POST` | `/api/auth/confirmar-exclusao` | US20 — Confirmar exclusão |

### 8.2. Viagens (Público)

| Método | Rota | US |
|---|---|---|
| `GET` | `/api/viagens` | US08 — Listar viagens |
| `GET` | `/api/viagens/{id}` | US08 — Detalhes da viagem |
| `GET` | `/api/viagens/{id}/vans` | US08 — Vans com assentos disponíveis |

### 8.3. Gerente — Vans

| Método | Rota | US |
|---|---|---|
| `GET` | `/api/gerente/vans` | — Listar vans |
| `POST` | `/api/gerente/vans` | US05 — Cadastrar van |
| `PUT` | `/api/gerente/vans/{id}` | US17 — Atualizar van |
| `DELETE` | `/api/gerente/vans/{id}` | — Remover van |

### 8.4. Gerente — Motoristas

| Método | Rota | US |
|---|---|---|
| `GET` | `/api/gerente/motoristas` | US24 — Listar motoristas |
| `POST` | `/api/gerente/motoristas` | US24 — Cadastrar motorista |
| `PUT` | `/api/gerente/motoristas/{id}` | US24 — Atualizar motorista |
| `DELETE` | `/api/gerente/motoristas/{id}` | US24 — Remover motorista |

### 8.5. Gerente — Viagens

| Método | Rota | US |
|---|---|---|
| `GET` | `/api/gerente/viagens` | US26 — Listar viagens |
| `POST` | `/api/gerente/viagens` | US06 — Criar viagem |
| `PUT` | `/api/gerente/viagens/{id}` | — Atualizar viagem |
| `DELETE` | `/api/gerente/viagens/{id}` | US26 — Cancelar viagem |
| `POST` | `/api/gerente/viagens/{id}/alocar-van` | US07 — Alocar van |
| `DELETE` | `/api/gerente/viagens/{id}/remover-van/{viagemVanId}` | US15 — Remover van |
| `POST` | `/api/gerente/viagens/{viagemId}/alocar-motorista/{viagemVanId}` | US25 — Alocar motorista |
| `GET` | `/api/gerente/viagens/{id}/reservas` | — Ver reservas |
| `GET` | `/api/gerente/viagens/{id}/relatorio` | US12 — Relatório financeiro |

### 8.6. Reservas

| Método | Rota | US |
|---|---|---|
| `POST` | `/api/reservas` | US09 — Criar reserva |
| `GET` | `/api/reservas/{id}` | — Detalhes |
| `GET` | `/api/reservas/minhas` | US14 — Minhas reservas |
| `POST` | `/api/reservas/{id}/pagar` | US10 — Pagar assento |
| `POST` | `/api/reservas/{id}/cancelar` | US11 — Cancelar |
| `POST` | `/api/reservas/{id}/solicitar-ingressos` | US28 — Solicitar ingressos |
| `GET` | `/api/reservas/{id}/ingressos` | US28 — Status dos ingressos |
| `POST` | `/api/reservas/{id}/ingressos/{itemReservaId}/confirmar-pagamento` | US28 — Confirmar pagamento ao gerente |

### 8.7. Gerente — Ingressos

| Método | Rota | US |
|---|---|---|
| `GET` | `/api/gerente/ingressos/solicitacoes` | US29 — Listar solicitações |
| `GET` | `/api/gerente/viagens/{viagemId}/ingressos` | US29 — Solicitações da viagem |
| `POST` | `/api/gerente/ingressos/{itemReservaId}/comprar` | US29 — Marcar como comprado |
| `POST` | `/api/gerente/ingressos/{itemReservaId}/entregue` | US29 — Marcar como entregue |
| `POST` | `/api/gerente/ingressos/{itemReservaId}/reembolsar` | US29 — Reembolsar |

### 8.8. Admin

| Método | Rota | US |
|---|---|---|
| `GET` | `/api/admin/gerentes` | US13 — Listar/buscar gerentes |
| `POST` | `/api/admin/gerentes` | US13 — Criar gerente |
| `PUT` | `/api/admin/gerentes/{id}` | US13 — Atualizar gerente |
| `GET` | `/api/admin/gerentes/{id}/reservas` | US23 — Histórico do gerente |
| `GET` | `/api/admin/usuarios` | US22 — Listar/buscar usuarios |
| `GET` | `/api/admin/usuarios/{id}/reservas` | US23 — Histórico do usuario |
| `GET` | `/api/admin/usuarios/{id}/perfis` | US22 — Perfis do usuario |

---

## 9. User Stories (29)

| US | Nome | Endpoint Principal |
|---|---|---|
| **US01** | Cadastro de Gerente (Usuario + Perfil Gerente) | `POST /api/auth/gerente/registrar` |
| **US02** | Login de Gerente | `POST /api/auth/login` (login único) |
| **US03** | Cadastro de Passageiro | `POST /api/auth/registrar` |
| **US04** | Login de Usuário | `POST /api/auth/login` |
| **US05** | Cadastrar Van | `POST /api/gerente/vans` |
| **US06** | Criar Viagem | `POST /api/gerente/viagens` |
| **US07** | Alocar Van na Viagem | `POST /api/gerente/viagens/{id}/alocar-van` |
| **US08** | Visualizar Viagens Disponíveis | `GET /api/viagens` |
| **US09** | Criar Reserva | `POST /api/reservas` |
| **US10** | Pagar Reserva | `POST /api/reservas/{id}/pagar` |
| **US11** | Cancelar Reserva | `POST /api/reservas/{id}/cancelar` |
| **US12** | Relatório Financeiro da Viagem | `GET /api/gerente/viagens/{id}/relatorio` |
| **US13** | Admin: Gerenciar Gerentes | `GET/POST/PUT /api/admin/gerentes` |
| **US14** | Ver Minhas Reservas | `GET /api/reservas/minhas` |
| **US15** | Remover Van da Viagem | `DELETE /api/gerente/viagens/{id}/remover-van/{viagemVanId}` |
| **US16** | Fluxo 0800 (Primeiros Clientes) | — (regra de negócio) |
| **US17** | Atualizar Van | `PUT /api/gerente/vans/{id}` |
| **US18** | Atualizar Usuario | `PUT /api/auth/usuario` |
| **US19** | Atualizar Perfil do Gerente | `PUT /api/auth/perfil/gerente` |
| **US20** | Excluir Conta | `POST /api/auth/solicitar-exclusao` |
| **US21** | Alterar Senha | `POST /api/auth/alterar-senha` |
| **US22** | Admin: Buscar Usuarios e Perfis | `GET /api/admin/usuarios` |
| **US23** | Admin: Ver Histórico de Reservas | `GET /api/admin/usuarios/{id}/reservas` |
| **US24** | Cadastrar Motorista | `POST /api/gerente/motoristas` |
| **US25** | Alocar Motorista na Viagem | `POST /api/gerente/viagens/{viagemId}/alocar-motorista/{viagemVanId}` |
| **US26** | Listar e Cancelar Viagens | `GET/DELETE /api/gerente/viagens` |
| **~~US27~~** | ~~Alternar Perfil Ativo~~ | ❌ **Removido** — login único, sem alternar perfil |
| **US28** | Solicitar Ingresso | `POST /api/reservas/{id}/solicitar-ingressos` |
| **US29** | Gerente: Gerenciar Ingressos | `GET/POST /api/gerente/ingressos/...` |

---

## 10. Estrutura de Projeto (Clean Architecture)

```
VanBora.sln
├── Api/                                    # Presentation Layer
│   ├── Controllers/
│   │   ├── AuthController.cs
│   │   ├── ViagensController.cs
│   │   ├── ReservasController.cs
│   │   ├── Gerente/
│   │   │   ├── VansController.cs
│   │   │   ├── MotoristasController.cs
│   │   │   ├── ViagensController.cs
│   │   │   └── IngressosController.cs       # 🆕
│   │   └── Admin/
│   │       ├── GerentesController.cs
│   │       └── UsuariosController.cs
│   ├── Middleware/
│   ├── Filters/
│   └── Program.cs
│
├── VanBora.Application/
│   ├── Interfaces/
│   │   ├── IAuthService.cs
│   │   ├── IViagemService.cs
│   │   ├── IReservaService.cs
│   │   ├── IVanService.cs
│   │   ├── IMotoristaService.cs
│   │   └── IIngressoService.cs              # 🆕
│   ├── Services/
│   │   ├── AuthService.cs
│   │   ├── ViagemService.cs
│   │   ├── ReservaService.cs
│   │   ├── VanService.cs
│   │   ├── MotoristaService.cs
│   │   └── IngressoService.cs               # 🆕
│   ├── DTOs/
│   └── Validators/
│
├── VanBora.Domain/
│   ├── Entities/
│   │   ├── Usuario.cs
│   │   ├── Perfil.cs
│   │   ├── Van.cs
│   │   ├── Viagem.cs
│   │   ├── ViagemVan.cs
│   │   ├── Reserva.cs
│   │   └── ItemReserva.cs
│   ├── ValueObjects/
│   │   ├── Email.cs
│   │   ├── CPF.cs
│   │   ├── Telefone.cs
│   │   ├── Placa.cs
│   │   └── Dinheiro.cs
│   ├── Enums/
│   │   ├── TipoPerfil.cs
│   │   ├── StatusViagem.cs
│   │   ├── StatusReserva.cs
│   │   └── StatusTicket.cs                  # 🆕
│   └── Interfaces/
│
├── VanBora.Infrastructure/
│   ├── Data/
│   │   ├── AppDbContext.cs
│   │   └── Configurations/
│   ├── Services/
│   │   ├── PixService.cs
│   │   ├── EmailService.cs
│   │   └── TokenService.cs
│   └── Migrations/
```

---

## 11. Plano de Implementação (5 Fases)

### Fase 1 — Setup e Domain
- [ ] 1.1. Configurar projeto Clean Architecture (Api, Application, Domain, Infrastructure)
- [ ] 1.2. Implementar Value Objects (Email, CPF, Telefone, Placa, Dinheiro)
- [ ] 1.3. Implementar Enums (TipoPerfil, StatusViagem, StatusReserva, StatusTicket)
- [ ] 1.4. Implementar entidades de domínio (Usuario, Perfil, Van, Viagem, ViagemVan, Reserva, ItemReserva)

### Fase 2 — Infraestrutura
- [ ] 2.1. Configurar EF Core + PostgreSQL + Migrations
- [ ] 2.2. Implementar configurações das entidades (Fluent API)
- [ ] 2.3. Implementar TokenService (JWT)
- [ ] 2.4. Implementar PixService (gateway externo)
- [ ] 2.5. Implementar EmailService

### Fase 3 — Application
- [ ] 3.1. Implementar DTOs e Validators (FluentValidation)
- [ ] 3.2. Implementar AuthService (registrar, login único, JWT, soft delete)
- [ ] 3.3. Implementar ViagemService (CRUD + alocação de vans)
- [ ] 3.4. Implementar VanService (CRUD)
- [ ] 3.5. Implementar MotoristaService (CRUD + ativação de conta)
- [ ] 3.6. Implementar ReservaService (criar, pagar, cancelar)
- [ ] 3.7. Implementar IngressoService (solicitar, confirmar pagamento, tracking)

### Fase 4 — API
- [ ] 4.1. AuthController (registrar, login, me, atualizar, alterar-senha, excluir)
- [ ] 4.2. ViagensController (público)
- [ ] 4.3. ReservasController (CRUD + solicitar ingressos + confirmar pagamento)
- [ ] 4.4. Gerente/VansController
- [ ] 4.5. Gerente/MotoristasController
- [ ] 4.6. Gerente/ViagensController
- [ ] 4.7. Gerente/IngressosController
- [ ] 4.8. Admin/GerentesController + UsuariosController

### Fase 5 — Integrações e Testes
- [ ] 5.1. Integrar webhook de pagamento (gateway Pix)
- [ ] 5.2. Implementar rotinas automáticas (expirar reservas, notificar prazo de ingresso)
- [ ] 5.3. Implementar soft delete + reembolso automático
- [ ] 5.4. Testes de integração dos principais fluxos
