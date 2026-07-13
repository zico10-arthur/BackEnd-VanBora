---
name: Alocação de Recursos à Viagem
status: pendente
---

# Tasks — Spec 50: Alocação de Recursos à Viagem

## Checklist

### 1. API (funções adicionais)
- [ ] **T1.1** — Adicionar funções em arquivos existentes:
  - `alocarVan(viagemId, vanId)` → `POST api/gerente/viagens/{id}/alocar-van` em `lib/api/viagens.ts`
  - `removerVanAlocada(viagemId, viagemVanId)` → `DELETE api/gerente/viagens/{id}/alocar-van/{viagemVanId}`
  - `alocarMotorista(viagemId, viagemVanId, motoristaId)` → `POST api/gerente/viagens/{id}/alocar-motorista/{viagemVanId}`
  - `removerMotoristaAlocado(viagemId, viagemVanId)` → `DELETE api/gerente/viagens/{id}/remover-motorista/{viagemVanId}`
- [ ] **T1.2** — Adicionar tipos `AlocarVanRequest`, `AlocarMotoristaRequest` em `lib/api/types.ts`
- [ ] **T1.3** — Garantir que `listarVans()` e `listarMotoristas()` estão acessíveis (Spec 30 e 40)

### 2. Página de alocação
- [ ] **T2.1** — Criar `app/gerente/viagens/[viagemId]/alocar/page.tsx`
  - Server component com metadata: "Alocar recursos — VanBora"
  - Header + breadcrumb: Viagens > {nomeEvento} > Alocar recursos
  - Renderiza `<AlocacaoClient>`

### 3. Componente principal
- [ ] **T3.1** — Criar `app/gerente/viagens/[viagemId]/alocar/AlocacaoClient.tsx`
  - 3 fetches em paralelo no useEffect:
    - `GET api/gerente/viagens/{id}` (dados da viagem + alocações atuais)
    - `GET api/gerente/vans` (vans disponíveis)
    - `GET api/auth/motoristas` (motoristas disponíveis)
  - state: viagem, vans, motoristas, loading, error

### 4. Seção "Vans"
- [ ] **T4.1** — Filtrar vans não alocadas: `vans.filter(v => !viagem.vans.find(vv => vv.vanId === v.vanId))`
- [ ] **T4.2** — Dropdown com vans disponíveis + botão "Alocar van"
- [ ] **T4.3** — Lista de vans alocadas (da resposta da viagem):
  - Exibe: modelo, placa, capacidade
  - Mostra motorista alocado (ou "Nenhum motorista")
  - Botão "Remover van" → modal de confirmação → `removerVanAlocada()`
- [ ] **T4.4** — Se van tem reservas ativas: erro → exibir "Esta van possui reservas ativas"

### 5. Seção "Motoristas"
- [ ] **T5.1** — Para cada van alocada, mostrar dropdown de motoristas disponíveis
  - Filtrar motoristas já alocados em outras vans desta viagem
- [ ] **T5.2** — Botão "Alocar motorista" → `alocarMotorista(viagemId, viagemVanId, motoristaId)`
- [ ] **T5.3** — Motorista alocado aparece abaixo da van com botão "Remover motorista"
- [ ] **T5.4** — Remover → `removerMotoristaAlocado()` → atualiza UI

### 6. Indicadores visuais
- [ ] **T6.1** — Banner no topo:
  - Se todas as vans têm motorista → verde "Viagem pronta para divulgação"
  - Se há vans sem motorista → amarelo "Aloque motoristas para completar"
  - Se não há vans → azul "Aloque pelo menos uma van"

### 7. Integração com Spec 20
- [ ] **T7.1** — Na `ViagensListClient` (Spec 20), adicionar badge "Completa"/"Pendente" baseado em `vans.length > 0 && todas com motorista`
- [ ] **T7.2** — Na `ViagemForm` ao criar, redirecionar para `/gerente/viagens/{id}/alocar` após sucesso

### 8. Testes manuais
- [ ] **T8.1** — Criar viagem → redireciona para alocação
- [ ] **T8.2** — Alocar van → van aparece na lista
- [ ] **T8.3** — Alocar motorista → motorista aparece vinculado à van
- [ ] **T8.4** — Remover van → confirmação → van some da lista
- [ ] **T8.5** — Banner muda conforme estado das alocações
- [ ] **T8.6** — Badge na lista de viagens reflete estado correto
