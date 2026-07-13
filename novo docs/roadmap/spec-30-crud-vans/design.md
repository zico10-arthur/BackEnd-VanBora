---
name: Gerenciamento de Vans
status: pendente
---

# Design — Spec 30: Gerenciamento de Vans

## Arquitetura

```
app/gerente/vans/
├── page.tsx                  # Lista de vans (server wrapper)
├── VansListClient.tsx        # Client component — grid + ações
├── nova/
│   └── page.tsx              # Formulário de cadastro
├── [vanId]/
│   └── editar/
│       └── page.tsx          # Formulário de edição
└── components/
    ├── VanCard.tsx           # Card individual da van
    ├── VanForm.tsx           # Formulário compartilhado (criar + editar)
    └── RemoverVanModal.tsx   # Modal de confirmação
```

## Stack

- Next.js 15 (App Router)
- Tailwind CSS (dark theme)
- Componentes: `Header`, `VbButton`
- API: `lib/api/http.ts`

## Fluxo de dados — Lista

```
VansListClient
  ├── useEffect → GET api/gerente/vans
  ├── state: vans[], loading, error
  ├── Renderiza grid responsivo de <VanCard />
  │     ├── Card: Modelo, Placa, Ano, Capacidade, Status
  │     ├── Botão "Editar" → router.push(/gerente/vans/{id}/editar)
  │     └── Botão "Remover" → abre <RemoverVanModal>
  └── Botão "Nova van" → router.push(/gerente/vans/nova)
```

## Fluxo de dados — Criar/Editar

```
VanForm (compartilhado)
  ├── Props: van? (opcional, para edição)
  ├── Campos: modelo, placa, ano, capacidade
  ├── Validação client-side:
  │     placa: regex Mercosul /^[A-Z]{3}[0-9][A-Z][0-9]{2}$/
  │     ano: 1990..anoAtual
  │     capacidade: 8..25
  ├── Submit:
  │     Criar: POST api/gerente/vans → toast + redirect lista
  │     Editar: PUT api/gerente/vans/{id} → toast + redirect lista
  └── Erro 409 (placa duplicada): exibe "Esta placa já está cadastrada"
```

## Tipos TypeScript

```ts
// lib/api/types.ts — adicionar:
type VanResponse = {
  vanId: string;
  modelo: string;
  placa: string;
  ano: number;
  capacidade: number;
  status: string;    // "Ativa" | "Inativa"
};

type CriarVanRequest = {
  modelo: string;
  placa: string;
  ano: number;
  capacidade: number;
};
```

## Decisões

1. **Grid de cards em vez de tabela**: Vans são poucas por gerente (1-10), cards com visual mais rico funcionam melhor que tabela densa.

2. **Validação de placa no client**: Regex Mercosul como primeira barreira; backend valida duplicidade.

3. **Soft delete**: Backend não exclui fisicamente — só marca como inativa. Frontend deve filtrar ou exibir badge "Inativa".

4. **Toast de feedback**: Usar um componente Toast simples (ou `window.alert` temporário) para confirmar sucesso das operações.

## Rotas

| Rota | Page | Auth |
|------|------|------|
| `/gerente/vans` | Lista | JWT + Gerente |
| `/gerente/vans/nova` | Cadastrar | JWT + Gerente |
| `/gerente/vans/{id}/editar` | Editar | JWT + Gerente |

## Estados visuais

| Estado | UI |
|--------|-----|
| Loading | 3 cards skeleton |
| Empty | "Nenhuma van" + CTA |
| Erro | Mensagem + retry |
| Sucesso cadastro | Toast verde |
| Erro placa duplicada | Borda vermelha no campo placa + mensagem |
| Erro remoção (van em uso) | Modal com mensagem explicativa |
