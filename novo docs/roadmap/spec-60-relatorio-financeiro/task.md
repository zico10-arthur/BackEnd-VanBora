---
name: Relatório Financeiro da Viagem
status: auditada
---

# Tasks — Spec 60: Relatório Financeiro da Viagem

## Checklist

### 1. Tipos e API
- [ ] **T1.1** — Adicionar tipos `RelatorioResponse`, `PassageiroRelatorio` em `lib/api/types.ts`
- [ ] **T1.2** — **Verificar CORS**: testar chamada `GET api/gerente/viagens/{id}/relatorio` do frontend. Como a rota está sob o mesmo path de viagens já testado na Spec 20, o CORS deve funcionar — apenas validar
- [ ] **T1.3** — Criar função `obterRelatorio(viagemId)` → `GET api/gerente/viagens/{id}/relatorio` em `lib/api/viagens.ts`

### 2. Componente ProgressBar
- [ ] **T2.1** — Criar `components/ui/ProgressBar.tsx`
  - Props: `value: number`, `max: number`, `label?: string`
  - Barra horizontal com preenchimento proporcional
  - Cor âmbar (`bg-van-amber`) com animação de transição
  - Exibe texto "X / Y" dentro ou acima

### 3. Componente Lista de Embarque
- [ ] **T3.1** — Criar `app/gerente/viagens/[viagemId]/relatorio/ListaEmbarque.tsx`
  - Props: `passageiros: PassageiroRelatorio[]`
  - Tabela: Nº Assento, Passageiro, Pagamento (badge), Contato
  - Assento vazio → texto "Disponível" em cinza claro
  - Status pagamento: badge verde "Confirmado" / badge amarelo "Pendente"
  - Telefone mascarado: `(XX) 9****-XXXX`

### 4. Página do relatório
- [ ] **T4.1** — Criar `app/gerente/viagens/[viagemId]/relatorio/page.tsx`
  - Server component com metadata dinâmica
  - Breadcrumb: Viagens > {nomeEvento} > Relatório
  - Renderiza `<RelatorioClient>`

### 5. Componente principal
- [ ] **T5.1** — Criar `app/gerente/viagens/[viagemId]/relatorio/RelatorioClient.tsx`
  - Fetch `GET api/gerente/viagens/{id}/relatorio`
  - state: relatorio, loading, error
  - Renderiza:
    - **Cabeçalho**: nomeEvento (h1), data, origem→destino
    - **Badge status**: Agendada/Cancelada/Concluída
    - **Badge break-even**: verde "Viável" ou amarelo "Faltam X assentos"
    - **Cards indicadores** (grid 2x2 ou 4 colunas):
      1. Receita total (`formatBrl`)
      2. Assentos: "X / Y vendidos"
      3. Progresso break-even: `<ProgressBar value={vendidos} max={quorum}>`
      4. Valor por assento (`formatBrl`)
    - **Lista de embarque**: `<ListaEmbarque passageiros={...}>`
    - **Botão "Compartilhar link"**: copia URL pública para clipboard + feedback
    - **Link "← Voltar"**: para `/gerente/viagens`

### 6. Ações
- [ ] **T6.1** — Botão "Compartilhar link":
  - Constrói URL: `${window.origin}/reserva/{viagemVanId}` (usar primeira van)
  - `navigator.clipboard.writeText(url)`
  - Toast/tooltip "Link copiado!" por 2 segundos

### 7. Integração com Spec 20
- [ ] **T7.1** — Na lista de viagens, botão "Ver" → link para `/gerente/viagens/{id}/relatorio`

### 8. Testes manuais
- [ ] **T8.1** — Acessar relatório de viagem com reservas → cards preenchidos
- [ ] **T8.2** — Acessar relatório de viagem sem reservas → cards zerados + tabela vazia
- [ ] **T8.3** — Break-even atingido → badge verde "Viável"
- [ ] **T8.4** — Break-even não atingido → badge amarelo "Faltam X"
- [ ] **T8.5** — Copiar link → feedback visual
- [ ] **T8.6** — Telefones mascarados na lista de embarque
- [ ] **T8.7** — Viagem de outro gerente → erro 403
