---
name: Relatório Financeiro da Viagem
status: pendente
---

# Design — Spec 60: Relatório Financeiro da Viagem

## Arquitetura

```
app/gerente/viagens/[viagemId]/
├── relatorio/
│   └── page.tsx                    # Server wrapper
└── relatorio/
    └── RelatorioClient.tsx         # Client component — fetch + render
```

A URL é: `/gerente/viagens/{viagemId}/relatorio`

## Stack

- Next.js 15 (App Router)
- Tailwind CSS
- Componentes: `Header`, `VbButton`
- API: `lib/api/http.ts`
- Formatação: `lib/format.ts` (formatBrl, formatDatePt — já existem)

## Fluxo de dados

```
RelatorioClient
  ├── useEffect → GET api/gerente/viagens/{id}/relatorio
  ├── state: relatorio, loading, error
  ├── Renderiza:
  │     ├── Cabeçalho: nomeEvento, data, origem→destino
  │     ├── Badge: status da viagem + "Viável"/"Não atingiu break-even"
  │     ├── Grid de indicadores (4 cards):
  │     │     Receita total (formatBrl)
  │     │     Assentos vendidos / capacidade
  │     │     Progresso break-even (barra)
  │     │     Valor por assento (formatBrl)
  │     ├── Tabela "Lista de embarque":
  │     │     Colunas: Assento, Passageiro, Status pagamento, Contato
  │     │     Assento vazio → "Disponível" em cinza
  │     └── Botão "Compartilhar link" → copia URL
  └── Botão "← Voltar" → router.push(/gerente/viagens)
```

## Tipos TypeScript

```ts
// lib/api/types.ts — adicionar:
type RelatorioResponse = {
  viagemId: string;
  nomeEvento: string;
  dataEvento: string;
  origem: string;
  destino: string;
  status: string;
  receitaTotal: number;
  assentosVendidos: number;
  capacidadeTotal: number;
  quorumMinimo: number;
  precoAssento: number;
  breakEvenAtingido: boolean;
  passageiros: PassageiroRelatorio[];
};

type PassageiroRelatorio = {
  numeroAssento: number;
  nomePassageiro: string | null;  // null = assento vazio
  telefonePassageiro: string | null;
  statusPagamento: string | null; // "Confirmado" | "Pendente" | null
};
```

## Decisões

1. **Rota filha da viagem**: `/gerente/viagens/{id}/relatorio` — mantém hierarquia lógica. A página de detalhe da viagem (`/gerente/viagens/{id}`) pode redirecionar para cá.

2. **Barra de progresso**: Componente visual `<ProgressBar value={n} max={quorumMinimo} />` — simples, com cor âmbar e animação CSS.

3. **Link de compartilhamento**: A URL pública da viagem é a do comprador (ex: `vanbora.com/reserva/{viagemVanId}`). Botão copia para clipboard com feedback visual "Link copiado!".

4. **Dados sensíveis na lista de embarque**: Telefone exibido com máscara (ex: `(21) 9****-1102`). Nome completo visível (necessário para o gerente no dia do evento).

## Rotas

| Rota | Page | Auth |
|------|------|------|
| `/gerente/viagens/{id}/relatorio` | Relatório | JWT + Gerente |

## Estados visuais

| Estado | UI |
|--------|-----|
| Loading | Skeleton cards + tabela |
| Erro | Mensagem + retry |
| Viagem sem reservas | Cards zerados + tabela "Nenhum passageiro ainda" |
| Break-even não atingido | Badge amarelo "Faltam X assentos" |
| Break-even atingido | Badge verde "Viagem viável" |
| Link copiado | Tooltip/toast "Link copiado!" |
