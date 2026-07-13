---
name: Gerenciamento de Viagens
status: pendente
---

# Design — Spec 20: Gerenciamento de Viagens

## Arquitetura

```
app/gerente/viagens/
├── page.tsx                  # Lista de viagens (server wrapper)
├── ViagensListClient.tsx     # Client component — tabela + ações
├── nova/
│   └── page.tsx              # Formulário de criação (server wrapper)
├── [viagemId]/
│   ├── page.tsx              # Detalhe da viagem → redireciona p/ relatório (Spec 60)
│   └── editar/
│       └── page.tsx          # Formulário de edição (server wrapper)
└── components/
    ├── ViagemForm.tsx        # Formulário compartilhado (criar + editar)
    ├── ViagemFormSkeleton.tsx
    ├── ViagemRow.tsx         # Linha da tabela
    └── CancelarViagemModal.tsx
```

## Stack

- Next.js 15 (App Router)
- Tailwind CSS (dark theme)
- Componentes: `Header`, `VbButton`
- API: `lib/api/http.ts` (apiGet, apiPost, apiPut)

## Fluxo de dados — Lista

```
ViagensListClient
  ├── useEffect → GET api/gerente/viagens
  ├── state: viagens[], loading, error
  ├── Renderiza tabela com <ViagemRow />
  │     ├── Botão "Editar" → router.push(/gerente/viagens/{id}/editar)
  │     └── Botão "Cancelar" → abre <CancelarViagemModal>
  └── Botão "Nova viagem" → router.push(/gerente/viagens/nova)
```

## Fluxo de dados — Criar/Editar

```
ViagemForm (compartilhado)
  ├── Props: viagem? (undefined = criar, preenchido = editar)
  ├── Campos controlados: evento, origem, destino, dataSaida, dataChegada, preco, possuiIngresso
  ├── Validação client-side antes do submit:
  │     origem.cidade !== destino.cidade || origem.estado !== destino.estado
  │     dataSaida < dataChegada
  │     dataSaida > now
  ├── Submit:
  │     Criar: POST api/gerente/viagens → redireciona p/ alocação (Spec 50)
  │     Editar: PUT api/gerente/viagens/{id} → redireciona p/ lista
  └── Error: exibe mensagem abaixo do formulário
```

## Tipos TypeScript

```ts
// lib/api/types.ts — adicionar:
type CriarViagemRequest = {
  nomeEvento: string;
  dataEvento: string;      // ISO 8601
  localEvento: string;
  origemDescricao: string;
  origemCidade: string;
  origemEstado: string;
  destinoDescricao: string;
  destinoCidade: string;
  destinoEstado: string;
  dataSaida: string;       // ISO 8601
  dataChegada: string;     // ISO 8601
  precoAssento: number;
  possuiIngresso: boolean;
};

type ViagemGerenteResponse = {
  viagemId: string;
  nomeEvento: string;
  dataEvento: string;
  dataPartida: string;
  origem: string;
  destino: string;
  precoAssento: number;
  possuiIngresso: boolean;
  status: string;
  vans: ViagemVanInfo[];
};

type ViagemVanInfo = {
  viagemVanId: string;
  vanModelo: string;
  vanPlaca: string;
  capacidade: number;
  assentosVendidos: number;
  motoristaNome?: string;
};
```

## Decisões

1. **Formulário compartilhado**: `ViagemForm` recebe prop opcional `viagem` — se undefined, modo criar; se preenchido, modo editar. Reduz duplicação.

2. **Validação client-side**: Feita no `ViagemForm` antes do submit, além da validação do backend. Campos com erro têm borda vermelha + mensagem.

3. **Redirecionamento pós-criação**: Após criar viagem, redireciona para `/gerente/viagens/{id}/alocar` (Spec 50), pois o próximo passo natural é alocar van e motorista.

4. **Cancelamento com modal**: Usar `<dialog>` ou componente modal próprio para confirmar antes de chamar a API.

## Rotas

| Rota | Page | Auth |
|------|------|------|
| `/gerente/viagens` | Lista | JWT + Gerente |
| `/gerente/viagens/nova` | Criar | JWT + Gerente |
| `/gerente/viagens/{id}` | Detalhe | JWT + Gerente |
| `/gerente/viagens/{id}/editar` | Editar | JWT + Gerente |

## Estados visuais

| Estado | UI |
|--------|-----|
| Loading lista | 5 linhas skeleton |
| Empty lista | Ilustração + "Nenhuma viagem" + CTA |
| Erro lista | Mensagem + retry |
| Loading formulário | Skeleton do form |
| Erro formulário | Mensagem abaixo do campo ou geral |
| Sucesso operação | Toast verde + redirect |
