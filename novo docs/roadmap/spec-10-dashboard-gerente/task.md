---
name: Dashboard do Gerente
status: auditada
---

# Tasks — Spec 10: Dashboard do Gerente

## Checklist

### 1. Tipos e API
- [ ] **T1.1** — Adicionar tipos `ViagemResumo` e `DashboardData` em `lib/api/types.ts`
- [ ] **T1.2** — **Verificar CORS**: o endpoint `GET api/gerente/viagens` já foi validado nas specs anteriores (20 e 50). Apenas confirmar que a chamada com token JWT funciona do dashboard
- [ ] **T1.3** — Criar função `listarViagensGerente()` em `lib/api/viagens.ts` que chama `GET api/gerente/viagens` com auth

### 2. Componente StatCard
- [ ] **T2.1** — Criar `components/gerente/dashboard/StatCard.tsx`
  - Props: `title`, `value`, `description?`, `href?`, `icon?`
  - Estilo: card dark com borda, valor em destaque (texto grande, cor âmbar)
  - Se `href` presente, envolver em `<Link>`
- [ ] **T2.2** — Criar `StatCardSkeleton.tsx` (pulsing placeholder)

### 3. Componente ViagensRecentes
- [ ] **T3.1** — Criar `components/gerente/dashboard/ViagensRecentes.tsx`
  - Props: `viagens: ViagemResumo[]`
  - Tabela responsiva com scroll horizontal em mobile
  - Colunas: Evento, Data Partida, Van, Assentos, Status
  - Cada linha é um `<Link>` para `/gerente/viagens/{id}/relatorio`

### 4. Página do Dashboard
- [ ] **T4.1** — Criar `app/gerente/dashboard/page.tsx` (server component com metadata)
- [ ] **T4.2** — Criar `app/gerente/dashboard/DashboardClient.tsx` (client component)
  - Fetch `GET api/gerente/viagens` no useEffect
  - Derivar indicadores (viagensAtivas, totalReservas, ocupacaoMedia, receitaTotal)
  - Renderizar 4 StatCards + ViagensRecentes
  - Estados: loading (skeleton), empty (CTA "Criar primeira viagem"), error (retry)

### 5. Navegação e Header
- [ ] **T5.1** — Atualizar `Header.tsx` adicionando link "Dashboard" visível para perfil Gerente
- [ ] **T5.2** — Redirecionar gerente para `/gerente/dashboard` após login (atualizar `AuthProvider.tsx` ou `LoginPageClient.tsx`)

### 6. Testes manuais
- [ ] **T6.1** — Login como gerente → redireciona para dashboard
- [ ] **T6.2** — Dashboard sem viagens → estado vazio com CTA
- [ ] **T6.3** — Dashboard com viagens → cards preenchidos + tabela
- [ ] **T6.4** — Clique nos cards → navega para tela correta
- [ ] **T6.5** — Token expirado → redireciona para `/entrar`
