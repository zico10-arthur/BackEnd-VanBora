---
name: Correções na Criação de Viagens — Design
status: auditada
---

# Spec 21 — Design de Implementação

## 1. Arquivos modificados

| Arquivo | Mudança |
|---------|---------|
| `frontend/lib/api/types.ts` | Reescrever `CriarViagemRequest`, `AtualizarViagemRequest`, `ViagemGerenteResponse` |
| `frontend/app/gerente/viagens/components/ViagemForm.tsx` | Substituir 6 campos origem/destino → `localPartida`, remover `dataChegada`, adicionar `quorumMinimo`, renomear `dataSaida`→`dataPartida` |
| `frontend/app/gerente/viagens/[viagemId]/editar/page.tsx` | Extrair apenas `AtualizarViagemRequest` no submit |

**Arquivos que NÃO mudam:**
- `ViagensListClient.tsx`, `ViagemRow.tsx`, `ViagemRowSkeleton.tsx`, `CancelarViagemModal.tsx`, `ToastBanner.tsx` — as colunas da tabela e os cabeçalhos não exibem origem/destino atualmente
- `nova/page.tsx` — mesma assinatura de `onSubmit`

## 2. Novos tipos TypeScript (`lib/api/types.ts`)

```typescript
// ── Spec 20 — Viagens (Gerente) ──────────────────────────────────

export type CriarViagemRequest = {
  nomeEvento: string;
  dataEvento: string;       // ISO 8601
  localEvento: string;
  dataPartida: string;      // ISO 8601 (renomeado de dataSaida)
  localPartida: string;     // NOVO — substitui os 6 campos origem/destino
  precoAssento: number;
  possuiIngresso: boolean;
  quorumMinimo: number;     // NOVO — obrigatório no backend
};

export type AtualizarViagemRequest = {
  nomeEvento: string;
  dataEvento: string;
  localEvento: string;
  dataPartida: string;
  localPartida: string;
  possuiIngresso: boolean;
};
// NÃO inclui precoAssento nem quorumMinimo

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
  localEvento: string;      // NOVO
  dataPartida: string;
  localPartida: string;     // NOVO — substitui origem/destino
  precoAssento: number;
  quorumMinimo: number;     // NOVO
  possuiIngresso: boolean;
  status: string;
  vans: ViagemVanGerenteInfo[];
};
// REMOVIDOS: origem, destino
```

## 3. ViagemForm — Mudanças detalhadas

### Props (ViagemFormProps)

```typescript
type ViagemFormProps = {
  viagem?: ViagemGerenteResponse;  // undefined = modo criação, presente = modo edição
  onSubmit: (data: CriarViagemRequest) => Promise<void>;
  submitLabel: string;
};
```

> `onSubmit` sempre recebe `CriarViagemRequest` completo. No modo edição, a página `editar/page.tsx` extrai o subconjunto `AtualizarViagemRequest` antes de enviar ao backend. O ViagemForm em si não sabe distinguir create vs edit na submissão.

### Estados (useState) — Antes × Depois

**Antes (13 estados):**
```
nomeEvento, dataEvento, localEvento,
origemDescricao, origemCidade, origemEstado,
destinoDescricao, destinoCidade, destinoEstado,
dataSaida, dataChegada, precoAssento, possuiIngresso
```

**Depois (9 estados):**
```
nomeEvento, dataEvento, localEvento,
localPartida,            // 1 campo substitui 6
dataPartida,             // renomeado
precoAssento, quorumMinimo, possuiIngresso
```

### Inicialização

```typescript
const [localPartida, setLocalPartida] = useState(viagem?.localPartida ?? "");
const [dataPartida, setDataPartida] = useState(
  viagem?.dataPartida ? viagem.dataPartida.slice(0, 16) : ""
);
const [quorumMinimo, setQuorumMinimo] = useState(
  viagem?.quorumMinimo?.toString() ?? ""
);
```

### Validação (`validate()`)

```typescript
function validate(): FieldErrors {
  const errs: FieldErrors = {};
  const now = new Date();

  if (!nomeEvento.trim()) errs.nomeEvento = "Nome do evento é obrigatório.";
  else if (nomeEvento.trim().length > 200) errs.nomeEvento = "Nome do evento deve ter no máximo 200 caracteres.";

  if (!dataEvento) errs.dataEvento = "Data do evento é obrigatória.";
  else if (new Date(dataEvento) <= now) errs.dataEvento = "Data do evento deve ser futura.";

  if (!localEvento.trim()) errs.localEvento = "Local do evento é obrigatório.";
  else if (localEvento.trim().length > 300) errs.localEvento = "Local do evento deve ter no máximo 300 caracteres.";

  if (!dataPartida) errs.dataPartida = "Data de partida é obrigatória.";
  else if (new Date(dataPartida) <= now) errs.dataPartida = "Data de partida deve ser futura.";
  else if (dataEvento && new Date(dataPartida) >= new Date(dataEvento))
    errs.dataPartida = "Data de partida deve ser anterior à data do evento.";

  if (!localPartida.trim()) errs.localPartida = "Informe o local de partida.";
  else if (localPartida.trim().length > 300) errs.localPartida = "Local de partida deve ter no máximo 300 caracteres.";

  const preco = Number(precoAssento);
  if (!precoAssento || isNaN(preco) || preco <= 0) errs.precoAssento = "Preço deve ser maior que zero.";

  const q = Number(quorumMinimo);
  if (!quorumMinimo || isNaN(q) || q <= 0 || !Number.isInteger(q))
    errs.quorumMinimo = "Quórum mínimo deve ser um número inteiro maior que zero.";

  return errs;
}
```

> Removidas: validações de `origemDescricao`, `origemCidade`, `origemEstado`, `destinoDescricao`, `destinoCidade`, `destinoEstado`, `dataChegada`, e `origemCidade === destinoCidade && origemEstado === destinoEstado`.

### Submissão (`handleSubmit`)

```typescript
const body: CriarViagemRequest = {
  nomeEvento: nomeEvento.trim(),
  dataEvento: new Date(dataEvento).toISOString(),
  localEvento: localEvento.trim(),
  dataPartida: new Date(dataPartida).toISOString(),
  localPartida: localPartida.trim(),
  precoAssento: Number(precoAssento),
  possuiIngresso,
  quorumMinimo: Number(quorumMinimo),
};
```

### Layout JSX — seções do formulário

1. Nome do evento → `<input type="text">`
2. Data do evento → `<input type="datetime-local">`
3. Local do evento → `<input type="text" placeholder="Ex: Estádio X, Rua Y, 123">`
4. Local de partida → `<input type="text" placeholder="Ex: Rodoviária Central, Rio de Janeiro - RJ">`
5. Data e hora de partida → `<input type="datetime-local">`
6. Preço do assento → `<input type="number" step="0.01" min="0.01">` — **visível apenas no modo criação**
7. Quórum mínimo → `<input type="number" step="1" min="1">` — **visível apenas no modo criação**
8. Possui ingresso → `<input type="checkbox">`

### Condicional modo criação × edição

```typescript
const isEdit = !!viagem;
```

Campos `precoAssento` e `quorumMinimo` são renderizados apenas quando `!isEdit`:

```tsx
{!isEdit && (
  <>
    {/* Preço do assento */}
    <div>...</div>
    {/* Quórum mínimo */}
    <div>...</div>
  </>
)}
```

### FieldErrors type

```typescript
type FieldErrors = Partial<Record<
  "nomeEvento" | "dataEvento" | "localEvento" | "dataPartida" | 
  "localPartida" | "precoAssento" | "quorumMinimo" | "api", 
  string
>>;
```

## 4. `editar/page.tsx` — Extração do `AtualizarViagemRequest`

```typescript
async function handleSubmit(data: CriarViagemRequest) {
  const body: AtualizarViagemRequest = {
    nomeEvento: data.nomeEvento,
    dataEvento: data.dataEvento,
    localEvento: data.localEvento,
    dataPartida: data.dataPartida,
    localPartida: data.localPartida,
    possuiIngresso: data.possuiIngresso,
  };
  await atualizarViagemGerente(viagemId, body);
  router.push("/gerente/viagens?sucesso=editada");
}
```

O ViagemForm sempre chama `onSubmit` com `CriarViagemRequest` completo. A página de edição extrai o subconjunto.

## 5. Validação client-side × server-side

| Regra | Backend (FluentValidation) | Frontend |
|-------|--------------------------|----------|
| `NomeEvento` obrigatório | `NotEmpty()` | `if (!nomeEvento.trim())` |
| `NomeEvento` max 200 | `MaximumLength(200)` | `if (nomeEvento.trim().length > 200)` |
| `DataEvento` futura | `GreaterThan(DateTime.UtcNow)` | `if (new Date(dataEvento) <= now)` |
| `LocalEvento` obrigatório | `NotEmpty()` | `if (!localEvento.trim())` |
| `LocalEvento` max 300 | `MaximumLength(300)` | `if (localEvento.trim().length > 300)` |
| `DataPartida` obrigatório | `NotEmpty()` | `if (!dataPartida)` |
| `LocalPartida` obrigatório | `NotEmpty()` | `if (!localPartida.trim())` |
| `LocalPartida` max 300 | `MaximumLength(300)` | `if (localPartida.trim().length > 300)` |
| `PrecoAssento > 0` | `GreaterThan(0)` | `if (!precoAssento \|\| preco <= 0)` |
| `QuorumMinimo > 0` | `GreaterThan(0)` | `if (!quorumMinimo \|\| q <= 0)` |
| `DataPartida < DataEvento` | `Must(x => x.DataPartida < x.DataEvento)` | `if (new Date(dataPartida) >= new Date(dataEvento))` |

## 6. Stack e dependências

| Pacote | Versão |
|--------|--------|
| Next.js | 14.2.18 |
| React | 18.x |
| TypeScript | 5.x |

Componentes reutilizados (já existentes, sem mudanças):
- `VbButton` (`@/components/vanbora/ui/VbButton`)
- `Header` (`@/components/Header`)
- `GerenteGuard` (`@/app/gerente/vans/components/GerenteGuard`)
- `ApiError` (`@/lib/api/http`)
