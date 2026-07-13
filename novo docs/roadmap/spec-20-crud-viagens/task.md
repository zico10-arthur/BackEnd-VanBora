---
name: Gerenciamento de Viagens
status: pendente
---

# Tasks — Spec 20: Gerenciamento de Viagens

## Checklist

### 1. Tipos e API
- [ ] **T1.1** — Adicionar tipos `CriarViagemRequest`, `ViagemGerenteResponse`, `ViagemVanInfo` em `lib/api/types.ts`
- [ ] **T1.2** — Criar funções em `lib/api/viagens.ts`:
  - `criarViagem(body)` → `POST api/gerente/viagens`
  - `listarViagensGerente()` → `GET api/gerente/viagens`
  - `atualizarViagem(id, body)` → `PUT api/gerente/viagens/{id}`
  - `cancelarViagem(id)` → `POST api/gerente/viagens/{id}/cancelar`
  - `obterViagemGerente(id)` → `GET api/gerente/viagens/{id}` (se backend tiver, senão usar listagem + filter)

### 2. Componente ViagemForm (compartilhado)
- [ ] **T2.1** — Criar `app/gerente/viagens/components/ViagemForm.tsx`
  - Props: `viagem?: ViagemGerenteResponse`, `onSubmit: (data) => Promise<void>`
  - Campos: nomeEvento, dataEvento (input datetime-local), localEvento, origem*, destino*, dataSaida, dataChegada, precoAssento, possuiIngresso (checkbox)
  - Validação client-side inline
  - Botão submit com loading state
  - Exibição de erro da API abaixo do formulário

### 3. Lista de viagens
- [ ] **T3.1** — Criar `app/gerente/viagens/ViagensListClient.tsx`
  - Fetch `GET api/gerente/viagens`
  - Tabela com colunas: Evento, Data Partida, Van alocada, Assentos, Status, Ações
  - Status com badge colorido (Agendada=verde, Cancelada=vermelho, Concluída=cinza)
  - Badge "Completa"/"Pendente alocação" (integração com Spec 50)
  - Botões: Ver (relatório), Editar, Cancelar
- [ ] **T3.2** — Criar `ViagemRow.tsx` (linha individual da tabela)
- [ ] **T3.3** — Criar `ViagensListClient` com estados: loading, empty, error

### 4. Página de criação
- [ ] **T4.1** — Criar `app/gerente/viagens/nova/page.tsx`
  - Wrapper com Header + título "Nova viagem"
  - Usa `<ViagemForm>` com `viagem={undefined}`
  - onSubmit → `criarViagem()` → redireciona para `/gerente/viagens/{id}/alocar`

### 5. Página de edição
- [ ] **T5.1** — Criar `app/gerente/viagens/[viagemId]/editar/page.tsx`
  - Fetch viagem atual via `obterViagemGerente(id)`
  - Usa `<ViagemForm>` com `viagem={data}`
  - onSubmit → `atualizarViagem()` → redireciona para lista

### 6. Cancelamento
- [ ] **T6.1** — Criar `CancelarViagemModal.tsx`
  - Modal de confirmação: "Tem certeza? Passageiros serão notificados."
  - onSubmit → `cancelarViagem(id)` → fecha modal + recarrega lista
  - Erro: exibe mensagem no modal

### 7. Lista principal
- [ ] **T7.1** — Criar `app/gerente/viagens/page.tsx` (server component)
  - Metadata: "Minhas Viagens — VanBora"
  - Renderiza `<ViagensListClient>`

### 8. Testes manuais
- [ ] **T8.1** — Criar viagem com dados válidos → redireciona para alocação
- [ ] **T8.2** — Criar viagem com origem = destino → erro de validação
- [ ] **T8.3** — Criar viagem com data passada → erro de validação
- [ ] **T8.4** — Editar viagem → campos pré-preenchidos → salvar → lista atualizada
- [ ] **T8.5** — Cancelar viagem → confirmação → status muda para Cancelada
- [ ] **T8.6** — Lista vazia → estado empty com CTA
- [ ] **T8.7** — Erro de rede → mensagem + retry
