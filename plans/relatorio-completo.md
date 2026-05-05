# 📋 Relatório Completo — VanBora

> **Data:** 05/05/2026
> **Documentos:** [`plans/inicial.md`](plans/inicial.md), [`plans/technical-plan.md`](plans/technical-plan.md), [`plans/user-stories.md`](plans/user-stories.md)
> **Total de User Stories:** 27 (US01–US27)

---

## Índice

- [📋 Relatório Completo — VanBora](#-relatório-completo--vanbora)
  - [Índice](#índice)
  - [1. Resumo Executivo](#1-resumo-executivo)
    - [Arquitetura](#arquitetura)
  - [2. Modelo de Negócio](#2-modelo-de-negócio)
  - [3. Atores do Sistema (Usuario + Perfil)](#3-atores-do-sistema-usuario--perfil)
  - [4. Regras de Negócio (24 RNs)](#4-regras-de-negócio-24-rns)
  - [5. Entidades de Domínio](#5-entidades-de-domínio)
    - [5.1. Usuario (pessoa física)](#51-usuario-pessoa-física)
    - [5.2. Perfil (papel do Usuario)](#52-perfil-papel-do-usuario)
    - [5.3. Van](#53-van)
    - [5.4. Viagem (Trip)](#54-viagem-trip)
    - [5.5. ViagemVan (junction)](#55-viagemvan-junction)
    - [5.6. Reserva](#56-reserva)
    - [5.7. ItemReserva](#57-itemreserva)
  - [6. Value Objects](#6-value-objects)
  - [7. Enums](#7-enums)
  - [8. Endpoints da API (40+)](#8-endpoints-da-api-40)
    - [8.1. Autenticação e Perfil](#81-autenticação-e-perfil)
    - [8.2. Viagens (Público)](#82-viagens-público)
    - [8.3. Gerente — Vans](#83-gerente--vans)
    - [8.4. Gerente — Motoristas](#84-gerente--motoristas)
    - [8.5. Gerente — Viagens](#85-gerente--viagens)
    - [8.6. Reservas](#86-reservas)
    - [8.7. Admin](#87-admin)
  - [9. User Stories (27)](#9-user-stories-27)
  - [10. Estrutura de Projeto (Clean Architecture)](#10-estrutura-de-projeto-clean-architecture)
  - [11. Plano de Implementação (5 Fases)](#11-plano-de-implementação-5-fases)
    - [Fase 1 — Setup e Domain](#fase-1--setup-e-domain)
    - [Fase 2 — Infraestrutura](#fase-2--infraestrutura)
    - [Fase 3 — Application](#fase-3--application)
    - [Fase 4 — API](#fase-4--api)
    - [Fase 5 — Integrações e Testes](#fase-5--integrações-e-testes)
  - [12. Perguntas em Aberto](#12-perguntas-em-aberto)

---

## 1. Resumo Executivo

O **VanBora** é uma plataforma **SaaS multi-tenant** que conecta passageiros a vans para transporte em eventos (jogos, shows, passeios turísticos). Cada **Gerente** opera independentemente como um inquilino, criando viagens e gerenciando vans. Os **Passageiros** reservam assentos, com opção de adquirir ingressos oficiais dos eventos.

### Arquitetura

- **Stack:** .NET 9, ASP.NET Core, PostgreSQL, Entity Framework Core
- **Padrão:** Clean Architecture (4 camadas: Api, Application, Domain, Infrastructure)
- **Autenticação:** JWT com claims de perfil (`perfil_atual`, `perfis[]`, `perfil_id`)
- **Pagamento:** Pix via QR Code (gateway externo)
- **Modelo de dados:** Usuario (CPF único) → N Perfis (Passageiro, Gerente, Motorista, Admin)

---

## 2. Modelo de Negócio

| Característica | Detalhe |
|---|---|
| 🏢 **Modelo** | Multi-tenant — cada gerente é um inquilino independente |
| 💰 **Receita** | Taxa por reserva (comissão percentual) |
| 🆓 **Primeiros clientes** | 2 primeiros clientes de cada gerente são 0800 (isentos de taxa) |
| 👥 **Público** | Qualquer tipo de evento: jogos, shows, passeios turísticos |
| 📱 **Pagamento** | QR Code Pix |
| 🎫 **Ingressos** | O gerente compra fora do sistema e revende na plataforma |

---

## 3. Atores do Sistema (Usuario + Perfil)

O modelo unificado **Usuario + Perfil** é a base do sistema:

```
┌─────────────────────────────────────────────────────┐
│                    USUARIO                          │
│              (Pessoa Física, CPF Único)             │
│  ┌─────────────┬──────────────┬──────────────────┐  │
│  │  Perfil     │  Perfil      │  Perfil          │  │
│  │  Passageiro │  Gerente     │  Motorista       │  │
│  │  email+senha│  email+senha │  sem login       │  │
│  │  faz reserva│  cria viagem │  alocado por     │  │
│  │             │  gerencia van│  gerente         │  │
│  └─────────────┴──────────────┴──────────────────┘  │
└─────────────────────────────────────────────────────┘
```

| Ator | Descrição |
|---|---|
| **👤 Usuário** | Pessoa física identificada por **CPF único**. Pode ter **múltiplos perfis** |
| **👤 Passageiro** | Perfil que permite reservar assentos. Login com email + senha |
| **👨‍💼 Gerente** | Perfil de tenant. Cria viagens, gerencia vans, define preços |
| **🔧 Motorista** | Perfil sem login, cadastrado pelo Gerente. Possui CNH |
| **🔧 Admin** | Perfil de administrador do sistema, criado diretamente no banco |

---

## 4. Regras de Negócio (24 RNs)

| # | Regra | Status |
|---|-------|--------|
| RN01 | Sistema multi-tenant: cada gerente opera independentemente | ✅ |
| RN02 | Gerente define preços e cria viagens | ✅ |
| RN03 | VanBora ganha taxa por reserva; 2 primeiros isentos | ✅ |
| RN04 | Usuário precisa ter conta para reservar | ✅ |
| RN05 | Usuário pode reservar 1 ou mais assentos | ✅ |
| RN06 | Cada assento pode ter ou não ingresso | ✅ |
| RN07 | Mistura de itens com/sem ingresso na mesma reserva | ✅ |
| RN08 | Ingresso nunca existe sem reserva | ✅ |
| RN09 | Só responsável precisa estar logado; demais informam dados | ✅ |
| RN10 | Gerente compra ingressos fora do sistema | ✅ |
| RN11 | Pagamento via QR Code Pix | ✅ |
| RN12 | Sistema atende qualquer tipo de evento | ✅ |
| RN13 | Gerente não cadastra ingressos individuais; usuário recebe link Face ID | ✅ |
| RN14 | Reserva só assento → email de confirmação | ✅ |
| RN15 | Capacidade da van não pode ser alterada após criação | ✅ |
| RN16 | CPF único e imutável; Slug do gerente também imutável | ✅ |
| RN17 | Exclusão requer código por email; pode excluir perfil específico ou conta completa | ✅ |
| RN18 | Gerente gerencia motoristas; remoção física apenas se sem viagens futuras | ✅ |
| RN19 | Passageiro tem 10 minutos para pagar; senão expira | ✅ |
| RN20 | Gerente pode cancelar viagens; reembolsa reservas confirmadas | ✅ |
| RN21 | Remover van de viagem com reservas = reembolso integral | ✅ |
| **RN22** | **Usuario pode ter múltiplos Perfis** (Passageiro, Gerente, Motorista, Admin) | ✅ **NOVO** |
| **RN23** | **Motorista não possui login** — cadastrado pelo Gerente | ✅ **NOVO** |
| **RN24** | **Email é único por Perfil**, não por Usuario | ✅ **NOVO** |

---

## 5. Entidades de Domínio

### 5.1. Usuario (pessoa física)

| Propriedade | Tipo | Descrição |
|---|---|---|
| Id | Guid | PK |
| Nome | string | Nome completo |
| CPF | CPF (VO) | Único, imutável |
| CriadoEm | DateTime | Data de criação |

### 5.2. Perfil (papel do Usuario)

| Propriedade | Tipo | Descrição |
|---|---|---|
| Id | Guid | PK |
| UsuarioId | Guid | FK → Usuario |
| Tipo | TipoPerfil | Passageiro, Gerente, Motorista, Admin |
| Email | Email (VO) | Único por Perfil (null para Motorista) |
| SenhaHash | string? | Hash (null para Motorista) |
| Telefone | Telefone (VO)? | Opcional |
| Ativo | bool | Se o perfil está ativo |
| CriadoPorPerfilId | Guid? | FK → Perfil (Gerente que cadastrou, apenas Motorista) |
| CriadoEm | DateTime | |

**Campos específicos por tipo:**

| Campo | Passageiro | Gerente | Motorista | Admin |
|---|---|---|---|---|
| Email | ✅ (login) | ✅ (login) | ❌ | ✅ |
| SenhaHash | ✅ | ✅ | ❌ | ✅ |
| Telefone | ✅ | ✅ | ✅ | ✅ |
| Slug | ❌ | ✅ único | ❌ | ❌ |
| TaxaPlataforma | ❌ | ✅ (%) | ❌ | ❌ |
| Gratuito | ❌ | ✅ (bool) | ❌ | ❌ |
| CNH | ❌ | ❌ | ✅ | ❌ |

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
| Status | StatusViagem (Agendada, EmAndamento, Concluida, Cancelada) |

### 5.5. ViagemVan (junction)

| Propriedade | Tipo |
|---|---|
| Id | Guid PK |
| ViagemId | Guid FK → Viagem |
| VanId | Guid FK → Van |
| MotoristaPerfilId | Guid? FK → Perfil (Motorista) |
| QuantidadeIngressos, IngressosDisponiveis | int? |

### 5.6. Reserva

| Propriedade | Tipo |
|---|---|
| Id | Guid PK |
| UsuarioId | Guid FK → Usuario |
| ViagemVanId | Guid FK → ViagemVan |
| PerfilPassageiroId | Guid FK → Perfil (Passageiro) |
| Status | StatusReserva |
| ValorTotal, TaxaPlataforma | decimal |
| CodigoPix, TransacaoId | string |
| PagoEm, CriadoEm, ExpiraEm | DateTime |

### 5.7. ItemReserva

| Propriedade | Tipo |
|---|---|
| Id | Guid PK |
| ReservaId | Guid FK |
| NumeroAssento | int (ex: 1 a 15) |
| PossuiIngresso | bool |
| PrecoAssento, PrecoIngresso | decimal, decimal? |
| LinkIngresso | string? |
| NomePassageiro, EmailPassageiro, TelefonePassageiro, CPFPassageiro | string |

---

## 6. Value Objects

Definidos em [`VanBora.Domain/ValueObjects/`](VanBora.Domain/ValueObjects/):

| VO | Propriedades | Validações |
|---|---|---|
| **Email** | Valor: string | Formato email |
| **CPF** | Valor: string (11 dígitos) | Dígitos verificadores |
| **Telefone** | DDD, Numero, ValorCompleto | DDD 2 dígitos, número 8-9 dígitos |
| **Placa** | Valor: string | Formato Mercosul (ABC1D23) |
| **Dinheiro** | Valor: decimal, Moeda: string | Não negativo, 2 casas, operações Somar/Subtrair/Multiplicar/Percentual |

---

## 7. Enums

Definidos em [`VanBora.Domain/Enums/`](VanBora.Domain/Enums/):

```csharp
public enum TipoPerfil { Passageiro, Gerente, Motorista, Admin }
public enum StatusViagem { Agendada, EmAndamento, Concluida, Cancelada }
public enum StatusReserva { PendentePagamento, Confirmada, EmAndamento, Concluida, Cancelada, Expirada }
```

---

## 8. Endpoints da API (40+)

### 8.1. Autenticação e Perfil

| Método | Rota | US |
|---|---|---|
| `POST` | `/api/auth/registrar` | US03 — Cadastro Passageiro |
| `POST` | `/api/auth/login` | US04 — Login Passageiro |
| `POST` | `/api/auth/gerente/registrar` | US01 — Cadastro Gerente |
| `POST` | `/api/auth/gerente/login` | US02 — Login Gerente |
| `GET` | `/api/auth/me` | — Dados do usuário + perfis |
| `PUT` | `/api/auth/usuario` | — Atualizar nome do Usuario |
| `PUT` | `/api/auth/perfil/passageiro` | US18 — Atualizar Perfil Passageiro |
| `PUT` | `/api/auth/perfil/gerente` | US19 — Atualizar Perfil Gerente |
| `POST` | `/api/auth/alterar-senha` | US21 — Alterar senha |
| `POST` | `/api/auth/alternar-perfil` | **US27 (NOVO)** — Alternar perfil ativo |
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
| `POST` | `/api/reservas/{id}/pagar` | US10 — Pagar |
| `POST` | `/api/reservas/{id}/cancelar` | US11 — Cancelar |

### 8.7. Admin

| Método | Rota | US |
|---|---|---|
| `GET` | `/api/admin/gerentes` | US13 — Listar/buscar gerentes |
| `POST` | `/api/admin/gerentes` | US13 — Criar gerente |
| `PUT` | `/api/admin/gerentes/{id}` | US13 — Atualizar gerente |
| `GET` | `/api/admin/gerentes/{id}/reservas` | US23 — Histórico do gerente |
| `GET` | `/api/admin/usuarios` | **US22** — Listar/buscar usuarios |
| `GET` | `/api/admin/usuarios/{id}/reservas` | US23 — Histórico do usuario |
| `GET` | `/api/admin/usuarios/{id}/perfis` | **US22** — Perfis do usuario |

---

## 9. User Stories (27)

| US | Nome | Endpoint Principal | Status |
|---|---|---|---|
| **US01** | Cadastro de Gerente (Usuario + Perfil Gerente) | `POST /api/auth/gerente/registrar` | ✅ Atualizado |
| **US02** | Login de Gerente | `POST /api/auth/gerente/login` | ✅ |
| **US03** | Cadastro de Passageiro (Usuario + Perfil Passageiro) | `POST /api/auth/registrar` | ✅ Atualizado |
| **US04** | Login de Usuário | `POST /api/auth/login` | ✅ |
| **US05** | Cadastrar Van | `POST /api/gerente/vans` | ✅ |
| **US06** | Criar Viagem | `POST /api/gerente/viagens` | ✅ |
| **US07** | Alocar Van na Viagem | `POST /api/gerente/viagens/{id}/alocar-van` | ✅ |
| **US08** | Visualizar Viagens Disponíveis | `GET /api/viagens` | ✅ |
| **US09** | Criar Reserva | `POST /api/reservas` | ✅ |
| **US10** | Pagar Reserva | `POST /api/reservas/{id}/pagar` | ✅ |
| **US11** | Cancelar Reserva | `POST /api/reservas/{id}/cancelar` | ✅ |
| **US12** | Relatório Financeiro da Viagem | `GET /api/gerente/viagens/{id}/relatorio` | ✅ |
| **US13** | Admin: Gerenciar Gerentes | `GET/POST/PUT /api/admin/gerentes` | ✅ |
| **US14** | Ver Minhas Reservas | `GET /api/reservas/minhas` | ✅ |
| **US15** | Remover Van da Viagem | `DELETE /api/gerente/viagens/{id}/remover-van/{viagemVanId}` | ✅ |
| **US16** | Fluxo 0800 (Primeiros Clientes) | — (regra de negócio) | ✅ |
| **US17** | Atualizar Van | `PUT /api/gerente/vans/{id}` | ✅ |
| **US18** | Atualizar Perfil do Passageiro | `PUT /api/auth/perfil/passageiro` | ✅ Atualizado |
| **US19** | Atualizar Perfil do Gerente | `PUT /api/auth/perfil/gerente` | ✅ Atualizado |
| **US20** | Excluir Conta | `POST /api/auth/solicitar-exclusao` | ✅ Atualizado |
| **US21** | Alterar Senha | `POST /api/auth/alterar-senha` | ✅ |
| **US22** | Admin: Buscar Usuarios e Perfis | `GET /api/admin/usuarios` | ✅ Atualizado |
| **US23** | Admin: Ver Histórico de Reservas | `GET /api/admin/usuarios/{id}/reservas` | ✅ |
| **US24** | Gerente: Cadastrar Motorista (Perfil Motorista) | `POST /api/gerente/motoristas` | ✅ Atualizado |
| **US25** | Gerente: Alocar Motorista na Viagem | `POST /api/gerente/viagens/{viagemId}/alocar-motorista/{viagemVanId}` | ✅ |
| **US26** | Gerente: Listar e Cancelar Viagens | `GET/DELETE /api/gerente/viagens` | ✅ |
| **US27** | **Alternar Perfil Ativo** | **`POST /api/auth/alternar-perfil`** | ✅ **NOVO** |

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
│   │   │   └── ViagensController.cs
│   │   └── Admin/
│   │       ├── GerentesController.cs
│   │       └── UsuariosController.cs       # 🆕
│   ├── Middleware/
│   └── Program.cs
│
├── VanBora.Application/
│   ├── Interfaces/
│   │   ├── IAuthService.cs
│   │   ├── IViagemService.cs
│   │   ├── IReservaService.cs
│   │   ├── IVanService.cs
│   │   └── IMotoristaService.cs
│   ├── Services/
│   ├── DTOs/
│   └── Mappings/
│
├── VanBora.Domain/
│   ├── Entities/
│   │   ├── Usuario.cs                      # 🆕
│   │   ├── Perfil.cs                       # 🆕 (substitui Gerente.cs e Motorista.cs)
│   │   ├── Van.cs
│   │   ├── Viagem.cs
│   │   ├── ViagemVan.cs
│   │   ├── Reserva.cs
│   │   └── ItemReserva.cs
│   ├── ValueObjects/
│   │   ├── Email.cs, CPF.cs, Telefone.cs, Placa.cs, Dinheiro.cs
│   ├── Enums/
│   │   ├── TipoPerfil.cs                   # 🆕
│   │   ├── StatusViagem.cs
│   │   └── StatusReserva.cs
│   └── Interfaces/
│       ├── IUsuarioRepository.cs           # 🆕
│       ├── IPerfilRepository.cs            # 🆕
│       ├── IVanRepository.cs
│       ├── IViagemRepository.cs
│       ├── IViagemVanRepository.cs
│       ├── IReservaRepository.cs
│       └── IUnitOfWork.cs
│
└── VanBora.Infrastructure/
    ├── Data/
    │   ├── AppDbContext.cs
    │   ├── Configurations/
    │   └── Migrations/
    ├── Repositories/
    ├── Services/
    │   ├── EmailService.cs
    │   └── PagamentoService.cs
    └── Extensions/
        └── ServiceCollectionExtensions.cs
```

**Mudanças na estrutura:**
- ❌ Removido: `Gerente.cs`, `Motorista.cs` (substituídos por `Perfil.cs` com `TipoPerfil`)
- ❌ Removido: `IGerenteRepository.cs`, `IMotoristaRepository.cs` (substituídos por `IPerfilRepository.cs`)
- ✅ Adicionado: `Usuario.cs`, `Perfil.cs`, `TipoPerfil.cs`
- ✅ Adicionado: `Admin/UsuariosController.cs`
- ✅ Adicionado: `IUsuarioRepository.cs`, `IPerfilRepository.cs`

---

## 11. Plano de Implementação (5 Fases)

### Fase 1 — Setup e Domain
| # | Task |
|---|------|
| 1.1 | Configurar projetos (referências entre camadas, NuGet) |
| 1.2 | Criar Value Objects (Email, CPF, Telefone, Placa, Dinheiro) |
| 1.3 | Criar Enums (TipoPerfil, StatusViagem, StatusReserva) |
| 1.4 | Criar entidades (Usuario, Perfil, Van, Viagem, ViagemVan, Reserva, ItemReserva) |
| 1.5 | Criar interfaces de repositório |

### Fase 2 — Infraestrutura
| # | Task |
|---|------|
| 2.1 | Configurar DbContext (AppDbContext + Fluent API) |
| 2.2 | Criar migrations para PostgreSQL |
| 2.3 | Implementar repositórios |
| 2.4 | Implementar UnitOfWork |

### Fase 3 — Application
| # | Task |
|---|------|
| 3.1 | DTOs + FluentValidation |
| 3.2 | AuthService (registro/login com Perfis, alternar perfis, JWT claims) |
| 3.3 | ViagemService |
| 3.4 | VanService |
| 3.5 | MotoristaService (cria Perfil Tipo=Motorista) |
| 3.6 | ReservaService |

### Fase 4 — API
| # | Task |
|---|------|
| 4.1 | AuthController |
| 4.2 | ViagensController (público) |
| 4.3 | ReservasController |
| 4.4 | Gerente/VansController |
| 4.5 | Gerente/MotoristasController |
| 4.6 | Gerente/ViagensController |
| 4.7 | Admin/GerentesController |
| 4.8 | Admin/UsuariosController |
| 4.9 | Middleware (Exception handling) |

### Fase 5 — Integrações e Testes
| # | Task |
|---|------|
| 5.1 | Mock do gateway de pagamento Pix |
| 5.2 | Serviço de Email |
| 5.3 | Webhook de pagamento |
| 5.4 | Testes dos fluxos principais |

---

## 12. Perguntas em Aberto

Abaixo estão pontos que ainda precisam de discussão sobre regras de negócio:

1. **CPF duplicado para Passageiro vs Gerente** — Para Passageiro (US03), CPF duplicado retorna erro 409. Para Gerente (US01), CPF existente reutiliza o Usuario. Esta diferença está correta?
2. **Exclusão de conta** — Quando o usuário tem múltiplos perfis e solicita exclusão de um perfil específico, o que acontece com o Usuario base? Permanece se houver outros perfis ativos?
3. **Admin criado apenas no banco** — Confirmado que não haverá endpoint para criar Admin?
4. **0800 (gratuito)** — Os 2 primeiros clientes são por gerente ou globais? Atualmente documentado como "por gerente".
5. **Reembolso** — Quando o gerente cancela uma viagem, o reembolso é processado automaticamente via Pix? Ou é manual?
6. **Capacidade da van** — RN15 diz que capacidade não pode ser alterada. Mas e se a van nunca foi usada em nenhuma viagem? Deveria permitir?

---

> 📁 **Documentos:** [`plans/inicial.md`](plans/inicial.md), [`plans/technical-plan.md`](plans/technical-plan.md), [`plans/user-stories.md`](plans/user-stories.md)
