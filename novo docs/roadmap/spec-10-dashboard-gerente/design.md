---
name: Dashboard do Gerente
status: auditada
---

# Design — Spec 10: Dashboard do Gerente

## Arquitetura

```
app/gerente/dashboard/
├── page.tsx                  # Server component — metadados + wrapper
├── DashboardClient.tsx       # Client component — fetch + render
├── cards/
│   ├── StatCard.tsx          # Componente reutilizável de card de indicador
│   └── StatCardSkeleton.tsx  # Skeleton loading
└── ViagensRecentes.tsx       # Tabela das próximas 5 viagens
```

## Stack

- Next.js 15 (App Router)
- Tailwind CSS (tema dark `bg-van-void` já existente)
- Componentes reutilizáveis: `Header`, `VbButton`
- Chamada API via `lib/api/http.ts` (já existe)

## Fluxo de dados

```
DashboardClient (client component)
  ├── useEffect → GET api/gerente/viagens
  ├── Deriva indicadores:
  │     viagensAtivas  = viagens.filter(s => s.status === "Agendada").length
  │     totalReservas  = sum(viagens.map(v => v.reservas))
  │     ocupacaoMedia  = avg(viagens.map(v => v.assentosVendidos / v.capacidade))
  │     receitaTotal   = sum(viagens.map(v => v.receita))
  ├── Renderiza 4 <StatCard />
  └── Renderiza <ViagensRecentes viagens={top5} />
```

## Tipos TypeScript

```ts
// lib/api/types.ts — adicionar:
type ViagemResumo = {
  viagemId: string;
  nomeEvento: string;
  dataPartida: string;
  vanModelo: string;
  vanPlaca: string;
  assentosVendidos: number;
  capacidade: number;
  status: string;
  receita: number;         // endpoint GET api/gerente/viagens já retorna no backend
  totalReservas: number;   // idem
};

type DashboardData = {
  viagensAtivas: number;
  totalReservas: number;
  ocupacaoMedia: number;
  receitaTotal: number;
  viagensRecentes: ViagemResumo[];
};
```

## Decisões

1. **Derivação no frontend**: Os indicadores são calculados no client a partir da resposta de `GET api/gerente/viagens`. Não existe endpoint separado de dashboard no backend. Se o backend adicionar um endpoint agregado no futuro, migramos.

2. **Responsividade**: Cards em grid 2 colunas (mobile) → 4 colunas (desktop). Tabela de viagens com scroll horizontal em mobile.

3. **Navegação**: Cada card é um `<Link>` do Next.js. Cada linha da tabela também. Usar `next/navigation` para SPA transitions.

4. **Header**: Reutilizar o `<Header />` já existente que mostra o menu com link para Dashboard, Vans, Motoristas, etc.

## Rotas

| Rota | Page | Auth |
|------|------|------|
| `/gerente/dashboard` | `page.tsx` | JWT + perfil Gerente |

## Estados visuais

| Estado | Componente |
|--------|-----------|
| Loading | 4x `StatCardSkeleton` + skeleton da tabela |
| Empty (sem viagens) | `TripsEmptyState` adaptado com CTA "Criar primeira viagem" |
| Erro | Mensagem + botão "Tentar novamente" |
| Dados | Cards + tabela preenchidos |
