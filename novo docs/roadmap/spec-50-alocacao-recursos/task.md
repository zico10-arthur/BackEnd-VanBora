---
name: Alocação de Recursos à Viagem
status: auditada
---

# Tasks — Spec 50: Alocação de Recursos à Viagem

## Checklist

### 1. API (funções adicionais)
- [x] **T1.0** — **Verificar CORS**: testar chamadas POST/DELETE para `api/gerente/viagens/{id}/alocar-van` e `alocar-motorista` do frontend. Confirmar que o backend aceita os métodos HTTP e headers necessários
- [x] **T1.1** — Adicionar funções em `lib/api/viagens.ts`:
  - `alocarVan(viagemId: string, vanId: string): Promise<ViagemGerenteResponse>` → `POST api/gerente/viagens/{id}/alocar-van` body: `{ vanId }`
    - O `viagemVanId` da van recém-alocada deve ser extraído como: `response.vans[response.vans.length - 1].viagemVanId`
  - `removerVanAlocada(viagemId: string, viagemVanId: string): Promise<ViagemGerenteResponse>` → `DELETE api/gerente/viagens/{id}/alocar-van/{viagemVanId}`
  - `alocarMotorista(viagemId: string, viagemVanId: string, body: AlocarMotoristaRequest): Promise<ViagemGerenteResponse>` → `POST api/gerente/viagens/{id}/alocar-motorista/{viagemVanId}` body: `{ motoristaId, viagemVanId }`
  - `removerMotoristaAlocado(viagemId: string, viagemVanId: string): Promise<boolean>` → `DELETE api/gerente/viagens/{id}/remover-motorista/{viagemVanId}`
- [x] **T1.2** — Adicionar tipos em `lib/api/types.ts`:
  - `AlocarVanRequest = { vanId: string }`
  - `AlocarMotoristaRequest = { motoristaId: string; viagemVanId: string }` (contem ambos os GUIDs)
- [x] **T1.3** — Garantir que `listarVans()` (`lib/api/vans.ts`) e `listarMotoristas()` (`lib/api/motoristas.ts`) estão acessíveis (Spec 30 e 40)

### 2. Modificar ViagemForm — Seções de Van e Motorista
- [x] **T2.1** — Adicionar props `vans: VanResponse[]` e `motoristas: MotoristaResponse[]` ao `ViagemForm`
- [x] **T2.2** — Adicionar props `vanId` e `motoristaId` (controlados externamente) e callbacks `onVanChange` / `onMotoristaChange`
- [x] **T2.3** — Adicionar seção "Van":
  - Dropdown com todas as vans: `{van.nome} — {van.modelo} ({van.placa}) — {van.capacidade} lugares`
  - Opção default: "Selecione uma van…"
  - Se `vans.length === 0`: mensagem "Nenhuma van cadastrada" + link para `/gerente/vans/nova`
- [x] **T2.4** — Adicionar seção "Motorista":
  - Dropdown com todos os motoristas: `{m.nome} — CNH: {m.cnh}`
  - Opção default: "Selecione um motorista…"
  - Se `motoristas.length === 0`: mensagem "Nenhum motorista cadastrado" + link para `/gerente/motoristas/novo`
- [x] **T2.5** — Adicionar validação: van e motorista obrigatórios (campos `vanId`, `motoristaId` nos `FieldErrors`)
- [x] **T2.6** — Desabilitar botão submit se `vans.length === 0 || motoristas.length === 0`
- [x] **T2.7** — Adicionar estados de loading/erro nos dropdowns:
  - Receber props `loadingRecursos: boolean`, `erroRecursos: string | null`, `onRetryRecursos: () => void`
  - Se `loadingRecursos === true`: exibir skeleton/spinner nos dropdowns com "Carregando vans…" / "Carregando motoristas…"
  - Se `erroRecursos !== null`: exibir banner de erro com mensagem + botão "Tentar novamente" que chama `onRetryRecursos`

### 3. Modificar NovaViagemPage — Fluxo sequencial
- [x] **T3.1** — Buscar vans e motoristas no `useEffect` da página:
  - `GET api/gerente/vans` → `listarVans()`
  - `GET api/gerente/motoristas` → `listarMotoristas()`
- [x] **T3.2** — Adicionar states: `vanId: string`, `motoristaId: string`, `erroEtapa: "criar" | "alocar-van" | "alocar-motorista" | null`, `erroMensagem: string`
- [x] **T3.3** — Modificar `handleSubmit` para fluxo sequencial com erro por etapa:
  ```
  async handleSubmit(data: CriarViagemRequest) {
    setLoading(true)
    setErroEtapa(null)

    // Passo 1: criar viagem
    let viagem: ViagemGerenteResponse
    try {
      viagem = await criarViagemGerente(data)
    } catch (err: any) {
      setErroEtapa("criar")
      setErroMensagem(err.message)
      setLoading(false)
      return
    }

    // Passo 2: alocar van
    let viagemVanResponse: ViagemGerenteResponse
    try {
      viagemVanResponse = await alocarVan(viagem.viagemId, vanId)
    } catch (err: any) {
      setErroEtapa("alocar-van")
      setErroMensagem(err.message)
      setLoading(false)
      return
    }

    // Extrair viagemVanId da van recem-alocada
    const viagemVanId = viagemVanResponse.vans[viagemVanResponse.vans.length - 1].viagemVanId

    // Passo 3: alocar motorista
    try {
      await alocarMotorista(viagem.viagemId, viagemVanId, { motoristaId, viagemVanId })
    } catch (err: any) {
      setErroEtapa("alocar-motorista")
      setErroMensagem(err.message)
      setLoading(false)
      return
    }

    router.push("/gerente/viagens?sucesso=criada")
  }
  ```
- [x] **T3.4** — Passar vans, motoristas, vanId, motoristaId e callbacks para `ViagemForm`
- [x] **T3.5** — Loading state unificado: botão exibe "Criando viagem e alocando recursos…" e fica **desabilitado** (prevenindo double-submit)
- [x] **T3.6** — Tratar estados de erro por etapa com `erroEtapa` state (`"criar" | "alocar-van" | "alocar-motorista" | null`):
  - Etapa `"criar"`: banner "Erro ao criar viagem: {msg}" — formulário permanece preenchido, nada foi salvo
  - Etapa `"alocar-van"`: banner "Viagem criada, mas não foi possível alocar a van: {msg}" + link para `/gerente/viagens/{viagemId}/alocar`
  - Etapa `"alocar-motorista"`: banner "Van alocada, mas não foi possível alocar o motorista: {msg}" + link para `/gerente/viagens/{viagemId}/alocar`
- [x] **T3.7** — Adicionar state `loadingRecursos`, `erroRecursos` para o fetch inicial de vans e motoristas:
  - `loadingRecursos` = true enquanto `listarVans()` ou `listarMotoristas()` não resolvem
  - Se fetch rejeitar por erro de rede: `erroRecursos = "Não foi possível carregar os recursos. Tente novamente."`
  - Se fetch retornar 401: redirecionar para `/entrar`
  - Se fetch retornar 403: `erroRecursos = "Acesso negado"`
  - Função `retryFetchRecursos()` que re-executa ambos os fetches e limpa `erroRecursos`
- [x] **T3.8** — Passar `loadingRecursos`, `erroRecursos`, `onRetryRecursos={retryFetchRecursos}` para `ViagemForm`

### 4. Tela de gerenciamento de alocações (pós-criação)
- [x] **T4.1** — Criar `app/gerente/viagens/[viagemId]/alocar/page.tsx`
  - Server component com metadata: "Gerenciar alocações — VanBora"
  - Envolver em `<GerenteGuard>`
  - Header + breadcrumb: Viagens > {nomeEvento} > Gerenciar alocações
  - Renderiza `<AlocacaoClient viagemId={params.viagemId}>`
- [x] **T4.2** — Criar `app/gerente/viagens/[viagemId]/alocar/AlocacaoClient.tsx`
  - 3 fetches em paralelo no useEffect:
    - `GET api/gerente/viagens/{id}` (dados da viagem + alocações atuais)
    - `GET api/gerente/vans` (vans do frotista)
    - `GET api/gerente/motoristas` (motoristas do frotista)
  - state: viagem, vans, motoristas, loading, error
- [x] **T4.3** — Implementar estados de carregamento/erro no AlocacaoClient:
  - `loading === true`: exibir skeleton com 3 seções (cabeçalho com título, vans placeholder, motoristas placeholder)
  - `error === "rede"`: banner "Não foi possível carregar. Tentar novamente." + botão que re-executa os 3 fetches
  - `error === "404"`: mensagem "Viagem não encontrada" + link para `/gerente/viagens`
  - `error === "403"`: redirecionar para `/entrar`
- [x] **T4.4** — Implementar banner de status geral:
  - Se `viagem.vans.length === 0`: banner azul "Aloque pelo menos uma van"
  - Se `viagem.vans.length > 0 && alguma van sem motorista`: banner amarelo "Aloque motoristas para completar"
  - Se `viagem.vans.length > 0 && todas as vans têm motorista`: banner verde "Viagem pronta para divulgação"

### 5. Seção "Vans" no gerenciamento
- [x] **T5.1** — Listar vans alocadas (da resposta da viagem):
  - Exibe: modelo, placa, capacidade
  - Mostra motorista alocado (ou "Nenhum motorista")
  - Botão "Remover van" → modal de confirmação → `removerVanAlocada()`
- [x] **T5.2** — Se van tem reservas ativas: erro 400 → exibir "Esta van possui reservas ativas"
- [x] **T5.3** — Se ainda há vans disponíveis: dropdown + botão "Alocar van"

### 6. Seção "Motoristas" no gerenciamento
- [x] **T6.1** — Para cada van alocada sem motorista: dropdown + botão "Alocar motorista"
- [x] **T6.2** — Para cada van alocada com motorista: nome do motorista + botão "Remover motorista"
- [x] **T6.3** — Filtrar motoristas já alocados em outras vans desta viagem

### 7. Indicadores visuais
- [x] **T7.1** — Banner no topo da tela de gerenciamento:
  - Se todas as vans têm motorista → verde "Viagem pronta para divulgação"
  - Se há vans sem motorista → amarelo "Aloque motoristas para completar"
  - Se não há vans → azul "Aloque pelo menos uma van"
- [x] **T7.2** — Badge na lista de viagens (`ViagensListClient` da Spec 20):
  - `vans.length > 0 && todas com motorista` → badge verde "Completa"
  - Caso contrário → badge amarelo "Pendente"

### 8. Integração com Spec 20
- [x] **T8.1** — Na `ViagensListClient`, adicionar badge "Completa"/"Pendente"
- [x] **T8.2** — Na `ViagemForm` ao criar, após sucesso, redirecionar para `/gerente/viagens?sucesso=criada` (já existente — apenas garantir que o fluxo sequencial está correto)
- [x] **T8.3** — Na edição de viagem existente, adicionar botão/link "Gerenciar alocações" → `/gerente/viagens/{id}/alocar`

### 9. Testes manuais
- [x] **T9.1** — Criar viagem com van + motorista → redireciona para lista com badge "Completa"
- [x] **T9.2** — Tentar criar viagem sem vans cadastradas → dropdown mostra mensagem + link
- [x] **T9.3** — Tentar criar viagem sem motoristas cadastrados → dropdown mostra mensagem + link
- [x] **T9.4** — Submeter sem selecionar van: erro "Selecione uma van"
- [x] **T9.5** — Submeter sem selecionar motorista: erro "Selecione um motorista"
- [x] **T9.6** — Erro na alocacao da van (400): banner com link para gerenciamento
- [x] **T9.7** — Erro na alocacao do motorista (400): banner com link para gerenciamento
- [x] **T9.8** — Erro de rede ao carregar vans/motoristas: banner com botao "Tentar novamente"
- [x] **T9.9** — Loading inicial: skeleton/spinner nos dropdowns
- [x] **T9.10** — Double-submit prevenido: botao desabilitado durante "Criando viagem e alocando recursos..."
- [x] **T9.11** — Gerenciamento: alocar van adicional: van aparece + toast verde
- [x] **T9.12** — Gerenciamento: alocar motorista: motorista vinculado + toast verde
- [x] **T9.13** — Gerenciamento: remover van: modal de confirmacao, van some
- [x] **T9.14** — Gerenciamento: remover van com reservas: toast "Esta van possui reservas ativas"
- [x] **T9.15** — Banner do gerenciamento muda: azul, amarelo, verde conforme estado
- [x] **T9.16** — Badge na lista: verde "Completa" ou amarelo "Pendente"
- [x] **T9.17** — Acessar gerenciamento para viagem inexistente: "Viagem nao encontrada"
- [x] **T9.18** — Token expirado durante fetches: redireciona para /entrar
