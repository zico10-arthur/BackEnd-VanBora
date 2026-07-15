---
name: Correções na Criação de Viagens — Tasks
status: auditada
---

# Spec 21 — Task Breakdown

## Pré-requisitos

- [ ] **P0:** Branch `specs-30-40` atualizado com código da Spec 20
- [ ] **P1:** Backend rodando para testes manuais (opcional para type-check)

---

## T1 — Atualizar tipos TypeScript (`lib/api/types.ts`)

- [x] T1.1 Reescrever `CriarViagemRequest` com 8 campos: `nomeEvento`, `dataEvento`, `localEvento`, `dataPartida`, `localPartida`, `precoAssento`, `possuiIngresso`, `quorumMinimo`
- [x] T1.2 Criar `AtualizarViagemRequest` independente com 6 campos (sem `precoAssento`, sem `quorumMinimo`)
- [x] T1.3 Corrigir `ViagemGerenteResponse`: adicionar `localEvento`, `localPartida`, `quorumMinimo`; remover `origem`, `destino`

## T2 — Reescrever ViagemForm (`components/ViagemForm.tsx`)

- [x] T2.1 Remover estados: `origemDescricao`, `origemCidade`, `origemEstado`, `destinoDescricao`, `destinoCidade`, `destinoEstado`, `dataChegada`
- [x] T2.2 Renomear estado `dataSaida` → `dataPartida`
- [x] T2.3 Adicionar estado `localPartida` (init: `viagem?.localPartida ?? ""`)
- [x] T2.4 Adicionar estado `quorumMinimo` (init: `viagem?.quorumMinimo?.toString() ?? ""`)
- [x] T2.5 Atualizar tipo `FieldErrors` (remover referências a campos removidos)
- [x] T2.6 Reescrever `validate()`:
  - `nomeEvento`: obrigatório, max 200 chars
  - `dataEvento`: obrigatório, data futura
  - `localEvento`: obrigatório, max 300 chars
  - `dataPartida`: obrigatório, data futura, < dataEvento
  - `localPartida`: obrigatório, max 300 chars
  - `precoAssento`: obrigatório, > 0
  - `quorumMinimo`: obrigatório, inteiro > 0
- [x] T2.7 Reescrever `handleSubmit` — body com 8 campos
- [x] T2.8 Reescrever JSX:
  - Remover fieldsets de Origem/Destino
  - Remover campo dataChegada
  - Renomear campo dataSaida → dataPartida
  - Adicionar campo localPartida (text input)
  - Adicionar campo quorumMinimo (number, step=1, min=1)
  - `precoAssento` e `quorumMinimo` visíveis só no modo criação (`!viagem`)

## T3 — Atualizar `editar/page.tsx`

- [x] T3.1 Adicionar `AtualizarViagemRequest` aos imports de `@/lib/api/types`
- [x] T3.2 No `handleSubmit`, extrair `AtualizarViagemRequest` (6 campos, sem precoAssento e quorumMinimo) antes de chamar `atualizarViagemGerente`

## T4 — Verificação

- [x] T4.1 `npx tsc --noEmit` — zero erros
- [x] T4.2 `npm run build` — compilação bem-sucedida
- [x] T4.3 Verificar ausência de imports de mocks antigos no portal do gerente: `grep -r "mockBotafogo\|tripManagementMock\|eventVehicleMock" frontend/app/gerente/` — zero resultados

---

## Mapeamento Tasks → Requisitos

| Task | FRs |
|------|-----|
| T1.1 | FR-001 |
| T1.2 | FR-002 |
| T1.3 | FR-003 |
| T2.1–T2.4 | FR-004, FR-005, FR-006, FR-007 |
| T2.6 | FR-004, FR-005, FR-008, FR-009, NFR-002 |
| T2.7 | FR-001 |
| T2.8 | FR-004, FR-005, FR-006 |
| T3.1–T3.2 | FR-002, FR-010 |
| T4 | NFR-001 |
