---
name: Gerenciamento de Viagens
status: auditada
---

# Tasks — Spec 20: Gerenciamento de Viagens

## Pré-requisitos

- [x] **P0** — Verificar CORS: testar `curl -H "Authorization: Bearer <token>" http://localhost:5151/api/gerente/viagens` do terminal. Se retornar erro CORS, ajustar `Api/Program.cs` para incluir a origin `http://localhost:3000`. Este passo é **bloqueante** para todas as tasks seguintes.

---

## Checklist

### T1 — Tipos TypeScript

- [x] **T1.1** — Adicionar tipos em `lib/api/types.ts` (após linha 131):
  - `CriarViagemRequest` (13 campos conforme design §3)
  - `AtualizarViagemRequest` (= `CriarViagemRequest`)
  - `ViagemVanGerenteInfo` (5 campos)
  - `ViagemGerenteResponse` (10 campos)
  - **Validação:** `npx tsc --noEmit` passa sem erros

### T2 — API Service

- [x] **T2.1** — Adicionar funções em `lib/api/viagens.ts` (manter funções existentes):
  - `listarViagensGerente()` → `apiGet("/api/gerente/viagens", true)`
  - `obterViagemGerente(id)` → `apiGet("/api/gerente/viagens/${id}", true)`
  - `criarViagemGerente(body)` → `apiPost("/api/gerente/viagens", body, true)`
  - `atualizarViagemGerente(id, body)` → `apiPut("/api/gerente/viagens/${id}", body, true)`
  - `cancelarViagemGerente(id)` → `apiPost("/api/gerente/viagens/${id}/cancelar", {}, true)`
  - **Validação:** `npx tsc --noEmit` passa sem erros

### T3 — ViagemForm (componente compartilhado)

- [x] **T3.1** — Criar `app/gerente/viagens/components/ViagemForm.tsx`:
  - Seguir o padrão de `VanForm.tsx` (mesma estrutura de estados, validação, tratamento de erro)
  - Props: `mode: "create" | "edit"`, `viagem?: ViagemGerenteResponse`, `onSubmit`, `submitLabel`
  - 13 campos controlados com `useState` (inicializados de `viagem?.xxx` ou `""`)
  - Função `validate()` com todas as regras do design §6
  - Validação origem ≠ destino: `origemCidade === destinoCidade && origemEstado === destinoEstado` → erro
  - Input `datetime-local` para campos de data (dataEvento, dataSaida, dataChegada)
  - Input `number` com step `0.01` para precoAssento
  - Checkbox para possuiIngresso
  - Botão submit com loading state + texto "Salvando…"
  - Erro da API exibido como banner abaixo do formulário
  - Erros de validação inline abaixo de cada campo (borda vermelha + texto)
  - Estilo: mesma classe `inputClass` do VanForm (zinc-900, border-zinc-700, focus:van-amber)
  - **Validação:** `npx tsc --noEmit` passa sem erros

### T4 — ViagemRow + Skeleton

- [x] **T4.1** — Criar `app/gerente/viagens/components/ViagemRow.tsx`:
  - Props: `viagem: ViagemGerenteResponse`, callbacks: `onEdit`, `onCancel`, `onView`
  - Colunas renderizadas como `<tr>`:
    - Evento: `<td>{viagem.nomeEvento} — {formattedDate(viagem.dataEvento)}</td>`
    - Data Partida: `<td>{formattedDateTime(viagem.dataPartida)}</td>`
    - Alocação: `<td>` se `viagem.vans.length > 0` → "Van {m.modelo} — {m.motoristaNome ?? 'Sem motorista'}" senão → badge "Pendente alocação" (text-yellow-500)
    - Assentos: `<td>{vendidos}/{capacidade}</td>` (soma sobre `viagem.vans`)
    - Status: badge colorido (Agendada=verde, EmAndamento=azul, Concluída=cinza, Cancelada=vermelho)
    - Ações: `<td>` com botões:
      - "Ver" — `VbButton variant="secondary" size="sm"` → `onView()`
      - "Editar" — `VbButton variant="secondary" size="sm"` → `onEdit()`, **disabled** se `status !== "Agendada"`
      - "Cancelar" — `VbButton variant="danger" size="sm"` → `onCancel()`, **disabled** se `status !== "Agendada"`
  - **Validação:** `npx tsc --noEmit` passa sem erros

- [x] **T4.2** — Criar `app/gerente/viagens/components/ViagemRowSkeleton.tsx`:
  - 1 linha `<tr>` com `<td>` contendo `<div className="animate-pulse ...">` simulando a altura da row real
  - Usado em triplicata (3×) no estado de loading da lista
  - **Validação:** `npx tsc --noEmit` passa sem erros

### T5 — CancelarViagemModal

- [x] **T5.1** — Criar `app/gerente/viagens/components/CancelarViagemModal.tsx`:
  - Props: `viagem: ViagemGerenteResponse`, `isOpen: boolean`, `onClose: () => void`, `onCancelled: () => void`
  - Implementação: `<dialog>` nativo HTML ou div fixa com backdrop (usar padrão do `RemoverVanModal` existente)
  - Conteúdo: título + texto de confirmação mencionando `viagem.nomeEvento`
  - Botões: "Não, manter viagem" (secondary, fecha) + "Sim, cancelar viagem" (danger, executa)
  - Loading state no botão de confirmação, erro exibido dentro do modal
  - Fecha com Esc, clica fora para fechar, trap focus
  - **Validação:** `npx tsc --noEmit` passa sem erros

### T6 — ViagensListClient (lista)

- [x] **T6.1** — Criar `app/gerente/viagens/ViagensListClient.tsx`:
  - Seguir exatamente o padrão de `VansListClient.tsx`:
    - Props: `sucesso: string | null`
    - States: `viagens[]`, `loading`, `error`, `toastMessage`, `cancelModalOpen` + `cancelTarget`
    - `useEffect` para toast de URL param + `router.replace` para limpar
    - `useEffect` + `useCallback` para fetch com `listarViagensGerente()`
    - Renderização condicional: loading → 3× ViagemRowSkeleton, error → mensagem + retry, empty → CTA, data → tabela
    - Tabela: `<div className="overflow-x-auto"><table className="w-full ...">` com `<thead>` + `<tbody>`
    - Cabeçalho com "Nova viagem" (`VbButton` → `router.push("/gerente/viagens/nova")`)
    - Toast: `<ToastBanner message={toastMessage} onDismiss={() => setToastMessage(null)} />`
    - Modal de cancelamento condicional: `cancelTarget && <CancelarViagemModal viagem={cancelTarget} isOpen={cancelModalOpen} onClose={...} onCancelled={refetch + toast} />`
    - GerenteGuard envolve o retorno
  - **Validação:** `npx tsc --noEmit` passa sem erros

### T7 — Páginas (server wrappers)

- [x] **T7.1** — Criar `app/gerente/viagens/page.tsx`:
  - Server component com `export const metadata`
  - Renderiza `<ViagensListClient sucesso={searchParams.sucesso ?? null} />`

- [x] **T7.2** — Criar `app/gerente/viagens/nova/page.tsx`:
  - Server component com `export const metadata`
  - Renderiza client component inline: `GerenteGuard` + `Header` + título "Nova viagem" + `<ViagemForm mode="create" ...>`
  - `onSubmit`: chama `criarViagemGerente(body)` → `router.push("/gerente/viagens?sucesso=criada")`

- [x] **T7.3** — Criar `app/gerente/viagens/[viagemId]/editar/page.tsx`:
  - Server component com `export const metadata`
  - Renderiza client component inline que:
    1. `useEffect` → `obterViagemGerante(viagemId)` para pré-preencher
    2. Se status ≠ "Agendada" → `router.replace("/gerente/viagens?sucesso=nao-editavel")`
    3. Se sucesso → renderiza `<ViagemForm mode="edit" viagem={data} ...>`
    4. `onSubmit`: chama `atualizarViagemGerente(viagemId, body)` → `router.push("/gerente/viagens?sucesso=editada")`

- [x] **T7.4** — Criar `app/gerente/viagens/[viagemId]/page.tsx`:
  - Placeholder: `redirect("/gerente/viagens")` até Spec 60 ser implementada

### T8 — Build e verificação

- [x] **T8.1** — Executar `npx tsc --noEmit` e garantir zero erros TypeScript
- [x] **T8.2** — Executar `npm run build` (ou `next build`) e garantir build bem-sucedido sem erros
- [x] **T8.3** — Verificar que os arquivos mock antigos (`lib/mockBotafogoGames.ts`, `lib/tripManagementMock.ts`, `lib/eventVehicleMock.ts`) não são mais importados por nenhum componente do portal do gerente

---

## Mapeamento Tasks → Requisitos

| Task | FR coberto |
|------|-----------|
| T1 + T2 | Tipos e API (infra para todos os FRs) |
| T3 (ViagemForm) | FR-002 (Criar), FR-003 (Editar) |
| T4 (ViagemRow) | FR-001 (Listar) |
| T5 (CancelarViagemModal) | FR-004 (Cancelar) |
| T6 (ViagensListClient) | FR-001 (Listar), FR-005 (Estados visuais) |
| T7.1 | FR-001 (Lista page wrapper) |
| T7.2 | FR-002 (Criar page wrapper) |
| T7.3 | FR-003 (Editar page wrapper) |
| T7.4 | FR-001 (Detalhe placeholder) |
| T8 | NFR-001, NFR-002, NFR-003, NFR-004 |

## Testes Manuais de Verificação

- [ ] **TM-01** — Acessar `/gerente/viagens` sem login → redirecionado para `/entrar`
- [ ] **TM-02** — Acessar `/gerente/viagens` logado como Gerente → lista carrega (ou mostra empty)
- [ ] **TM-03** — Lista vazia → estado empty com CTA "Criar primeira viagem"
- [ ] **TM-04** — Criar viagem com dados válidos → sucesso, redireciona para lista com toast verde
- [ ] **TM-05** — Criar viagem com campos vazios → erros inline nos campos obrigatórios
- [ ] **TM-06** — Criar viagem com origem === destino (mesma cidade+estado) → erro de validação
- [ ] **TM-07** — Criar viagem com dataSaida no passado → erro de validação
- [ ] **TM-08** — Criar viagem com dataChegada <= dataSaida → erro de validação
- [ ] **TM-09** — Criar viagem com precoAssento <= 0 → erro de validação
- [ ] **TM-10** — Editar viagem Agendada → campos pré-preenchidos, salvar → sucesso
- [ ] **TM-11** — Tentar editar viagem Cancelada/Concluída → redirecionado para lista com toast
- [ ] **TM-12** — Cancelar viagem Agendada → modal confirmação → sucesso → lista atualizada + toast
- [ ] **TM-13** — Botões Editar/Cancelar desabilitados para viagem não-Agendada
- [ ] **TM-14** — Simular erro de rede (desligar API) → estado de erro com botão "Tentar novamente"
- [ ] **TM-15** — Simular API retornando 500 → toast de erro
