# Pendências e Bugs Conhecidos — Reservas (Sprint 3)

> Documento gerado em 26/05/2026 após implementação da Sub-tarefa 3.1.4
> Última atualização: 26/05/2026 — Correções aplicadas
> Baseado no plano [`docs/dev1-sprint3.md`](docs/dev1-sprint3.md) e [`docs/technical-plan.md`](docs/technical-plan.md)

---

## ✅ Corrigido: Guards com condição invertida

### [`Reserva.cs`](VanBora.Domain/Entities/Reserva.cs) — 5 Guards corrigidos

Os 5 `Guard.AgainstInvalidState` com condição invertida foram corrigidos em [`Reserva.cs`](VanBora.Domain/Entities/Reserva.cs:62):

| Método | Condição (antes) | Condição (depois) |
|--------|------------------|-------------------|
| `ConfirmarPagamento()` | `Status == PendentePagamento` | `Status != PendentePagamento` |
| `Cancelar()` (1º) | `Status != Concluida` | `Status == Concluida` |
| `Cancelar()` (2º) | `Status != Cancelada` | `Status == Cancelada` |
| `IniciarViagem()` | `Status == Confirmada` | `Status != Confirmada` |
| `Concluir()` | `Status == EmAndamento` | `Status != EmAndamento` |

### [`Viagem.cs`](VanBora.Domain/Entities/Viagem.cs) — 6 condições corrigidas

| Método | Condição (antes) | Condição (depois) |
|--------|------------------|-------------------|
| Construtor (linha 47) | `dataPartida < dataEvento` | `dataPartida >= dataEvento` |
| `AtualizarDados()` (linha 74) | `dataPartida < dataEvento` | `dataPartida >= dataEvento` |
| `Iniciar()` (linha 86) | `Status == Agendada` | `Status != Agendada` |
| `Concluir()` (linha 93) | `Status == EmAndamento` | `Status != EmAndamento` |
| `Cancelar()` (1º, linha 100) | `Status != Concluida` | `Status == Concluida` |
| `Cancelar()` (2º, linha 101) | `Status != Cancelada` | `Status == Cancelada` |

---

## ✅ Migration Aplicada

A migration `AddReservas` foi aplicada com sucesso ao banco PostgreSQL (`localhost:5432`). Todas as 3 migrations estão no banco:

| Migration | Descrição |
|-----------|-----------|
| `20260520203848_Initial` | Criação inicial do esquema |
| `20260523214529_AddQuorumMinimo` | Campo QuorumMinimo na Viagem |
| `20260526183059_AddReservas` | Tabelas `reservas` e `itens_reserva` |

### O que a migration `AddReservas` criou

| Tabela | Descrição |
|--------|-----------|
| `reservas` | Reservas de assentos (FK → `usuarios`, FK → `viagem_vans`) |
| `itens_reserva` | Itens individuais da reserva (FK → `reservas`, cascade) |
| Índice único | `ix_itens_reserva_reserva_id_assento` — impede duplicidade de assento na mesma reserva |

---

## 2. Dependências com Outros Devs

| Dependência | Dev | Descrição |
|-------------|-----|-----------|
| `POST /api/reservas/{id}/pagar` (US10) | **Dev 2** | Webhook de pagamento e confirmação |
| `POST /api/reservas/{id}/cancelar` (US11) | **Dev 2** | Cancelamento de reserva |
| `ReservaService.GerarPagamento` | **Dev 2** | Método no ReservaService |
| Gestão de reservas no relatório financeiro | **Dev 3** | US12 depende das reservas persistidas |
