---
name: Gerenciamento de Vans
status: pendente
---

# Tasks — Spec 30: Gerenciamento de Vans

## Checklist

### 1. Tipos e API
- [ ] **T1.1** — Adicionar tipos `VanResponse`, `CriarVanRequest` em `lib/api/types.ts`
- [ ] **T1.2** — **Configurar CORS no backend**: verificar `Program.cs` e garantir que a origin do frontend (`http://localhost:3000`) está liberada nos endpoints `api/gerente/vans`. Se necessário, adicionar política CORS para permitir métodos GET/POST/PUT/DELETE com header `Authorization`
- [ ] **T1.3** — Criar funções em `lib/api/vans.ts` (novo arquivo):
  - `listarVans()` → `GET api/gerente/vans`
  - `criarVan(body)` → `POST api/gerente/vans`
  - `atualizarVan(id, body)` → `PUT api/gerente/vans/{id}`
  - `removerVan(id)` → `DELETE api/gerente/vans/{id}`

### 2. Componente VanForm (compartilhado)
- [ ] **T2.1** — Criar `app/gerente/vans/components/VanForm.tsx`
  - Props: `van?: VanResponse`, `onSubmit: (data) => Promise<void>`
  - Campos: modelo, placa, ano, capacidade
  - Validação client-side:
    - placa: regex Mercosul `/^[A-Z]{3}[0-9][A-Z][0-9]{2}$/`
    - ano: entre 1990 e ano atual
    - capacidade: entre 8 e 25
  - Mensagens de erro inline por campo
  - Botão submit com loading state
  - Erro da API: exibe abaixo do formulário

### 3. Card de Van
- [ ] **T3.1** — Criar `app/gerente/vans/components/VanCard.tsx`
  - Props: `van: VanResponse`, `onEdit`, `onRemove`
  - Exibe: Modelo (título), Placa (badge), Ano, Capacidade, Status (badge Ativa/Inativa)
  - Botões: Editar (ícone lápis), Remover (ícone lixeira)
  - Estilo: card dark com borda, hover effect

### 4. Lista de vans
- [ ] **T4.1** — Criar `app/gerente/vans/VansListClient.tsx`
  - Fetch `GET api/gerente/vans`
  - Grid responsivo: 1 col (mobile) → 2 col (tablet) → 3 col (desktop)
  - Renderiza `<VanCard>` para cada van
  - Botão "Nova van" no topo → `router.push(/gerente/vans/nova)`
  - Estados: loading (3 cards skeleton), empty ("Nenhuma van"), error

### 5. Página de cadastro
- [ ] **T5.1** — Criar `app/gerente/vans/nova/page.tsx`
  - Header + título "Nova van"
  - Usa `<VanForm>` com `van={undefined}`
  - onSubmit → `criarVan()` → toast + redirect para `/gerente/vans`

### 6. Página de edição
- [ ] **T6.1** — Criar `app/gerente/vans/[vanId]/editar/page.tsx`
  - Fetch van atual via `obterVan(id)` (adicionar em T1.2)
  - Usa `<VanForm>` com `van={data}`
  - onSubmit → `atualizarVan()` → toast + redirect

### 7. Modal de remoção
- [ ] **T7.1** — Criar `RemoverVanModal.tsx`
  - Props: `van: VanResponse`, `open`, `onClose`
  - Exibe modelo e placa
  - Confirmação → `removerVan(id)` → fecha + recarrega
  - Erro 400 (van em uso) → mensagem "Esta van está alocada em viagens futuras"

### 8. Lista principal
- [ ] **T8.1** — Criar `app/gerente/vans/page.tsx` (server component)

### 9. Testes manuais
- [ ] **T9.1** — Cadastrar van com dados válidos → aparece na lista
- [ ] **T9.2** — Cadastrar van com placa inválida → erro de validação
- [ ] **T9.3** — Cadastrar van com capacidade fora do range → erro
- [ ] **T9.4** — Editar van → campos pré-preenchidos → salvar
- [ ] **T9.5** — Remover van sem reservas → sucesso
- [ ] **T9.6** — Remover van com reservas → mensagem de erro
- [ ] **T9.7** — Placa duplicada → erro 409
