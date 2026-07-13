---
name: Alocação de Recursos à Viagem
status: pendente
---

# Design — Spec 50: Alocação de Recursos à Viagem

## Arquitetura

```
app/gerente/viagens/[viagemId]/
├── alocar/
│   └── page.tsx                  # Tela de alocação (server wrapper)
└── alocar/
    └── AlocacaoClient.tsx        # Client component principal
```

**Nota**: Esta spec não cria novas pastas de rota — estende a rota de viagem existente. A tela de alocação é acessível de dois modos:
1. Redirecionamento automático após criar viagem (Spec 20)
2. Botão "Gerenciar alocações" na edição da viagem (Spec 20)

A URL é: `/gerente/viagens/{viagemId}/alocar`

## Stack

- Next.js 15 (App Router)
- Tailwind CSS
- Componentes: `Header`, `VbButton`
- API: `lib/api/http.ts`

## Fluxo de dados

```
AlocacaoClient
  ├── Montagem:
  │     GET api/gerente/vans         → lista de vans do gerente
  │     GET api/auth/motoristas      → lista de motoristas do gerente
  │     GET api/gerente/viagens/{id} → viagem com vans e motoristas já alocados
  ├── state: vansDisponiveis[], motoristasDisponiveis[], alocacoes[]
  ├── Seção "Vans":
  │     ├── Dropdown com vans NÃO alocadas
  │     ├── Botão "Alocar" → POST api/gerente/viagens/{id}/alocar-van
  │     └── Lista de vans alocadas:
  │           ├── Modelo + Placa + Capacidade
  │           ├── Motorista alocado (ou "Nenhum")
  │           └── Botão "Remover van" → DELETE .../{viagemVanId}
  └── Seção "Motoristas":
        └── Para cada van alocada:
              ├── Dropdown com motoristas NÃO alocados
              ├── Botão "Alocar motorista" → POST .../{id}/alocar-motorista/{viagemVanId}
              └── Botão "Remover motorista" → DELETE .../{id}/remover-motorista/{viagemVanId}
```

## Tipos TypeScript

```ts
// lib/api/types.ts — adicionar:
type AlocarVanRequest = {
  vanId: string;
};

type AlocarMotoristaRequest = {
  motoristaId: string;
};
```

## Decisões

1. **Tudo em uma tela só**: Não faz sentido separar "alocar van" e "alocar motorista" em telas diferentes — o gerente faz as duas coisas em sequência. Uma tela com duas seções resolve.

2. **Dropdown com filtro**: As opções do dropdown de vans excluem vans já alocadas nesta viagem. O mesmo para motoristas. Filtro feito no client (comparando IDs).

3. **Sem modal para alocar**: A ação de alocar é instantânea (dropdown + botão) — não precisa de modal. Só a remoção usa modal de confirmação.

4. **Badge de completude na Spec 20**: A lista de viagens deve mostrar um indicador visual. Adicionar lógica: se `vans.length > 0 && todas têm motorista` → badge "Completa", senão → badge "Pendente".

## Rotas

| Rota | Page | Auth |
|------|------|------|
| `/gerente/viagens/{id}/alocar` | Alocação | JWT + Gerente |

## Estados visuais

| Estado | UI |
|--------|-----|
| Loading | Skeleton com 3 seções |
| Sem vans cadastradas | "Cadastre uma van primeiro" + link p/ Spec 30 |
| Sem motoristas cadastrados | "Cadastre um motorista primeiro" + link p/ Spec 40 |
| Van alocada com sucesso | Van aparece na lista + toast |
| Erro ao remover van com reservas | Modal: "Esta van possui reservas ativas" |
| Tudo completo | Badge verde "Viagem pronta para divulgação" |
