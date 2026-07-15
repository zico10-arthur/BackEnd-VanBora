---
name: Gerenciamento de Viagens
status: auditada
---

# Design — Spec 20: Gerenciamento de Viagens

## 1. Arquitetura de Arquivos

```
app/gerente/viagens/
├── page.tsx                  # Server wrapper — metadata + renderiza ViagensListClient
├── ViagensListClient.tsx     # Client component — tabela + ações + estados
├── nova/
│   └── page.tsx              # Server wrapper — renderiza ViagemForm em modo criar
├── [viagemId]/
│   ├── page.tsx              # Detalhe → redireciona para Spec 60 (placeholder até lá)
│   └── editar/
│       └── page.tsx          # Server wrapper — fetch viagem + renderiza ViagemForm
└── components/
    ├── ViagemForm.tsx        # Formulário compartilhado (criar + editar)
    ├── ViagemRowSkeleton.tsx  # Skeleton para linha da tabela (3 linhas)
    ├── ViagemRow.tsx         # Linha individual da tabela
    └── CancelarViagemModal.tsx # Modal de confirmação de cancelamento
```

Arquivos existentes que serão modificados:

| Arquivo | Tipo de mudança |
|---------|-----------------|
| `lib/api/types.ts` | Adicionar tipos `CriarViagemRequest`, `AtualizarViagemRequest`, `ViagemGerenteResponse`, `ViagemVanGerenteInfo` |
| `lib/api/viagens.ts` | Adicionar funções `criarViagemGerente`, `listarViagensGerente`, `atualizarViagemGerente`, `cancelarViagemGerente`, `obterViagemGerente` |

---

## 2. Stack

| Dependência | Versão | Uso |
|------------|--------|-----|
| Next.js | 14.2.18 (Pages Router NÃO — é App Router confirmado) | Roteamento, server/client components |
| React | ^18 | UI |
| Tailwind CSS | ^3.4.1 | Estilização dark theme |
| TypeScript | ^5 | Tipagem |

Componentes reutilizáveis existentes (já implementados nas specs 30/40):
- `Header` — barra superior do portal
- `VbButton` — botão padronizado (variant: primary/secondary/danger)
- `ToastBanner` — toast no canto superior direito, autodismiss 5s + botão X
- `GerenteGuard` — client component que verifica JWT + claim `tipos` contendo "Gerente"
- `ApiError` — classe de erro com `status`, `message`, `code`

---

## 3. Tipos TypeScript

Adicionar em `lib/api/types.ts`:

```ts
// ── Spec 20 — Viagens (Gerente) ──────────────────────────────────

export type CriarViagemRequest = {
  nomeEvento: string;
  dataEvento: string;       // ISO 8601 (datetime-local serializado)
  localEvento: string;
  origemDescricao: string;
  origemCidade: string;
  origemEstado: string;     // 2 chars, UF (ex: "RJ")
  destinoDescricao: string;
  destinoCidade: string;
  destinoEstado: string;    // 2 chars, UF
  dataSaida: string;        // ISO 8601
  dataChegada: string;      // ISO 8601
  precoAssento: number;
  possuiIngresso: boolean;
};

export type AtualizarViagemRequest = CriarViagemRequest;
// (idêntico — backend aceita o mesmo corpo para PUT)

export type ViagemVanGerenteInfo = {
  viagemVanId: string;
  vanModelo: string;
  vanPlaca: string;
  capacidade: number;
  assentosVendidos: number;
  motoristaNome: string | null;
};

export type ViagemGerenteResponse = {
  viagemId: string;
  nomeEvento: string;
  dataEvento: string;
  dataPartida: string;
  origem: string;           // string concatenada pelo backend, ex: "Rodoviária, Rio de Janeiro - RJ"
  destino: string;
  precoAssento: number;
  possuiIngresso: boolean;
  status: string;           // "Agendada" | "EmAndamento" | "Concluida" | "Cancelada"
  vans: ViagemVanGerenteInfo[];
};
```

---

## 4. API Service (`lib/api/viagens.ts`)

Adicionar novas funções (manter as existentes `listarViagens` e `obterDetalheViagemVan`):

```ts
import { apiGet, apiPost, apiPut } from "./http";
import type {
  CriarViagemRequest,
  AtualizarViagemRequest,
  ViagemGerenteResponse,
} from "./types";

export function listarViagensGerente(): Promise<ViagemGerenteResponse[]> {
  return apiGet<ViagemGerenteResponse[]>("/api/gerente/viagens", true);
}

export function obterViagemGerente(id: string): Promise<ViagemGerenteResponse> {
  return apiGet<ViagemGerenteResponse>(`/api/gerente/viagens/${id}`, true);
}

export function criarViagemGerente(
  body: CriarViagemRequest
): Promise<ViagemGerenteResponse> {
  return apiPost<ViagemGerenteResponse>("/api/gerente/viagens", body, true);
}

export function atualizarViagemGerente(
  id: string,
  body: AtualizarViagemRequest
): Promise<ViagemGerenteResponse> {
  return apiPut<ViagemGerenteResponse>(`/api/gerente/viagens/${id}`, body, true);
}

export function cancelarViagemGerente(id: string): Promise<void> {
  return apiPost<void>(`/api/gerente/viagens/${id}/cancelar`, {}, true);
}
```

---

## 5. Fluxo de Dados — Lista (`ViagensListClient`)

```
ViagensListClient (use client)
  ├── GerenteGuard envolve todo o conteúdo
  ├── useEffect → listarViagensGerente()
  ├── State: viagens[] | loading | error | toastMessage
  ├── Renderiza condicional:
  │     loading  → 3× <ViagemRowSkeleton>
  │     error    → mensagem + botão "Tentar novamente"
  │     empty    → "Nenhuma viagem cadastrada" + botão "Criar primeira viagem"
  │     data     → tabela com <ViagemRow> por item
  ├── Toast: <ToastBanner message={toastMessage} onDismiss={...} />
  ├── URL params: lê ?sucesso=criada|editada para exibir toast inicial
  │     Após exibir, limpa param com router.replace
  └── Ações por linha:
        ├── "Ver"   → router.push(`/gerente/viagens/${id}`) → redireciona p/ relatório (Spec 60)
        ├── "Editar" → router.push(`/gerente/viagens/${id}/editar`)
        │             (só habilitado se status === "Agendada")
        └── "Cancelar" → abre <CancelarViagemModal>
                        (só habilitado se status === "Agendada")
```

---

## 6. Fluxo de Dados — Formulário (`ViagemForm`)

```
ViagemForm (use client, compartilhado criar/editar)
  ├── GerenteGuard envolve todo o conteúdo
  ├── Props:
  │     mode: "create" | "edit"
  │     viagem?: ViagemGerenteResponse    // undefined no modo create
  │     onSubmit: (data: CriarViagemRequest) => Promise<void>
  │     submitLabel: string               // "Criar viagem" | "Salvar alterações"
  ├── State local para cada campo (inicializado de viagem?.xxx ou "")
  ├── State: loading, errors: FieldErrors
  │
  ├── Validação (validate() antes do submit):
  │     nomeEvento: !trim() → "Informe o nome do evento."
  │     dataEvento: !value → obrigatório; new Date(value) <= now → "Data do evento deve ser futura."
  │     localEvento: !trim() → "Informe o local do evento."
  │     origemDescricao: !trim() → obrigatório
  │     origemCidade: !trim() → obrigatório
  │     origemEstado: !trim() → "UF inválida." (se !== 2 chars)
  │     destinoDescricao: !trim() → obrigatório
  │     destinoCidade: !trim() → obrigatório
  │     destinoEstado: !trim() → "UF inválida." (se !== 2 chars)
  │     dataSaida: !value → obrigatório; new Date(value) <= now → "Data de saída deve ser futura."
  │     dataChegada: !value → obrigatório; new Date(value) <= new Date(dataSaida) → "Data de chegada deve ser posterior à saída."
  │     precoAssento: !value || Number(value) <= 0 → "Preço deve ser maior que zero."
  │     origem × destino: origemCidade === destinoCidade && origemEstado === destinoEstado
  │        → "Origem e destino não podem ser iguais."
  │
  ├── Submit:
  │     setLoading(true), limpa errors
  │     Monta body: CriarViagemRequest com valores dos campos
  │     await onSubmit(body)
  │       → sucesso: tratado pelo caller (redirect com toast param)
  │       → erro ApiError:
  │           409 + code "VIAGEM_DUPLICADA" → errors.api = err.message
  │           400 → errors.api = err.message (erro de validação do backend)
  │           outros → errors.api = err.message
  │       → erro genérico: errors.api = "Erro ao salvar. Tente novamente."
  │     finally: setLoading(false)
  │
  └── UI:
        Cada campo: <label> + <input> + erro inline
        Estilo: mesma classe inputClass do VanForm (border-zinc-700, bg-zinc-900, focus:van-amber)
        Erro API: banner vermelho abaixo de todos os campos
        Botão submit: VbButton type="submit", disabled={loading}, texto condicional
```

---

## 7. Componente `CancelarViagemModal`

```
CancelarViagemModal
  ├── Props:
  │     viagem: ViagemGerenteResponse
  │     isOpen: boolean
  │     onClose: () => void
  │     onCancelled: () => void   // callback após cancelamento bem-sucedido
  ├── Implementação: <dialog> nativo com backdrop
  │     ou div fixa com z-50 + backdrop blur (padrão do projeto, ver outros modais)
  ├── State: loading, error
  ├── Conteúdo:
  │     Título: "Cancelar viagem"
  │     Corpo: "Tem certeza que deseja cancelar esta viagem para {nomeEvento}? Passageiros com reserva serão notificados."
  │     Botões:
  │       "Não, manter viagem" (VbButton secondary) → onClose()
  │       "Sim, cancelar viagem" (VbButton danger, disabled={loading}) → ação
  │     Se error: <p className="text-red-400 text-sm">{error}</p>
  ├── Ação confirmar:
  │     setLoading(true)
  │     try { await cancelarViagemGerente(viagem.viagemId); onCancelled(); }
  │     catch (err) { setError(err.message) }
  │     finally { setLoading(false) }
  └── Acessibilidade: foco no primeiro botão ao abrir, fecha com Esc, trap focus dentro do modal
```

---

## 8. Páginas Server Wrapper

### `app/gerente/viagens/page.tsx`
```tsx
import type { Metadata } from "next";
import { ViagensListClient } from "./ViagensListClient";

export const metadata: Metadata = {
  title: "Minhas Viagens — VanBora",
};

type Props = { searchParams: { sucesso?: string } };

export default function ViagensPage({ searchParams }: Props) {
  return <ViagensListClient sucesso={searchParams.sucesso ?? null} />;
}
```

### `app/gerente/viagens/nova/page.tsx`
```tsx
import type { Metadata } from "next";
import { NovaViagemClient } from "./NovaViagemClient"; // ou componente inline

export const metadata: Metadata = {
  title: "Nova Viagem — VanBora",
};

export default function NovaViagemPage() {
  return <NovaViagemClient />;
}
```

### `app/gerente/viagens/[viagemId]/editar/page.tsx`
```tsx
import type { Metadata } from "next";
import { EditarViagemClient } from "./EditarViagemClient";

export const metadata: Metadata = {
  title: "Editar Viagem — VanBora",
};

type Props = { params: { viagemId: string } };

export default function EditarViagemPage({ params }: Props) {
  return <EditarViagemClient viagemId={params.viagemId} />;
}
```

### `app/gerente/viagens/[viagemId]/page.tsx`
```tsx
import { redirect } from "next/navigation";

// Placeholder até Spec 60 — redireciona para o relatório (Spec 60)
// ou, se Spec 60 não implementada, redireciona para a lista com toast
type Props = { params: { viagemId: string } };

export default function ViagemDetalhePage({ params }: Props) {
  redirect(`/gerente/viagens?sucesso=redirecionado`);
  // Quando Spec 60 existir: redirect(`/gerente/viagens/${params.viagemId}/relatorio`);
}
```

---

## 9. Estados Visuais Detalhados

| Estado | Local | UI |
|--------|-------|-----|
| Loading lista | ViagensListClient | 3× `<ViagemRowSkeleton>` (div com animate-pulse, mesma altura da row real) |
| Empty lista | ViagensListClient | Ícone + `<p>Nenhuma viagem cadastrada</p>` + `<VbButton>Criar primeira viagem</VbButton>` |
| Error lista | ViagensListClient | `<p className="text-red-400">{error}</p>` + `<VbButton variant="secondary">Tentar novamente</VbButton>` |
| Loading formulário | ViagemForm | Botão submit: texto "Salvando…", disabled, opacidade reduzida |
| Erro validação campo | ViagemForm | Borda do input muda para `border-red-500`, mensagem `<p className="text-xs text-red-400">` abaixo |
| Erro API formulário | ViagemForm | Banner: `<div className="rounded-lg border border-red-700/60 bg-red-950/40 px-4 py-3 text-sm text-red-300">` |
| Sucesso operação | ToastBanner | Fundo verde (`bg-green-900 border-green-700`), texto branco, 5s autodismiss, botão X |
| Erro operação | ToastBanner | Fundo vermelho (`bg-red-900 border-red-700`), texto branco, 5s autodismiss, botão X |
| Botão desabilitado | ViagemRow | Botão "Editar"/"Cancelar" com `opacity-50 cursor-not-allowed pointer-events-none` |
| Modal aberto | CancelarViagemModal | Backdrop escuro (`bg-black/60 fixed inset-0 z-40`), modal centralizado (`z-50`) |

---

## 10. Tabela de Rotas

| Rota | Componente | Guard | Descrição |
|------|-----------|-------|-----------|
| `/gerente/viagens` | `ViagensListClient` | GerenteGuard | Lista de viagens |
| `/gerente/viagens/nova` | `ViagemForm` (mode=create) | GerenteGuard | Criar viagem |
| `/gerente/viagens/[viagemId]` | redirect | GerenteGuard | Detalhe → redireciona |
| `/gerente/viagens/[viagemId]/editar` | `ViagemForm` (mode=edit) | GerenteGuard | Editar viagem |

---

## 11. Decisões de Design

1. **Formulário compartilhado `ViagemForm`:** Recebe `mode` ("create" | "edit") e `viagem?` opcional. No modo "create", `viagem` é undefined e os campos iniciam vazios. No modo "edit", `viagem` é a resposta do backend e os campos são pré-preenchidos. Esta é a mesma abordagem do `VanForm` existente.

2. **Validação client-side:** Feita no `ViagemForm.validate()` antes do submit. A validação do backend (FluentValidation) é uma segunda linha de defesa — erros 400 são exibidos como erro de API.

3. **Redirecionamento pós-criação:** Após criar viagem, redireciona para `/gerente/viagens?sucesso=criada`. O redirecionamento para alocação (Spec 50) deve ser um botão visível na lista quando Spec 50 estiver implementada.

4. **Origem vs Destino — correção da validação:** A condição correta é `origemCidade === destinoCidade && origemEstado === destinoEstado` (AND, não OR). O OR causava falso-positivo em qualquer diferença parcial.

5. **ToastBanner reutilizado:** Mesmo componente usado em Vans e Motoristas. Recebe `message: string | null` e `onDismiss: () => void`.

6. **GerenteGuard em todas as páginas:** Cada page wrapper renderiza `<GerenteGuard>` que verifica JWT e claim. Se inválido, redireciona para `/entrar`. Padrão já estabelecido nas specs 30 e 40.

7. **Detalhe da viagem (Spec 60 placeholder):** O botão "Ver" redireciona para `/gerente/viagens/{id}` que, até a Spec 60 ser implementada, redireciona de volta para a lista com um toast informativo.

8. **`obterViagemGerente`:** O backend (`Gerente.ViagensController`) expõe `GET /api/gerente/viagens/{id}` que retorna uma viagem específica. Usado na página de edição para pré-preencher o formulário.

---

## 12. Formato de Resposta da API

O backend retorna JSON no formato:

**Sucesso (200):** corpo direto (ex: `ViagemGerenteResponse[]` ou `ViagemGerenteResponse`)

**Erro (400/401/403/404/409/500):**
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "Mensagem de erro descritiva"
}
```
ou (formato legado com `error` wrapper):
```json
{
  "error": {
    "code": "VIAGEM_DUPLICADA",
    "message": "Já existe uma viagem com este nome para esta data."
  }
}
```

O `extractApiErrorMessage` do `http.ts` lida com ambos os formatos.
