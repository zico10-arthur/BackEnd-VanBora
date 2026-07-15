---
name: Alocação de Recursos à Viagem
status: pendente
---

# Design — Spec 50: Alocação de Recursos à Viagem

## Arquitetura

Esta spec modifica duas áreas:

### 1. Formulário de criação de viagem (fluxo principal)

A alocação de van e motorista é integrada diretamente no `ViagemForm` (Spec 20), que passa a buscar e exibir os recursos do frotista.

```
app/gerente/viagens/
├── components/
│   └── ViagemForm.tsx          # ← MODIFICADO: ganha seções de van e motorista
└── nova/
    └── page.tsx                # ← MODIFICADO: fluxo sequencial criar → alocar
```

### 2. Tela de gerenciamento de alocações (fluxo secundário)

Para edição pós-criação, mantém-se a tela dedicada:

```
app/gerente/viagens/[viagemId]/
├── alocar/
│   ├── page.tsx                # Server wrapper
│   └── AlocacaoClient.tsx      # Client component de gerenciamento
```

## Stack

- Next.js 15 (App Router)
- Tailwind CSS
- Componentes: `Header`, `VbButton`
- API: `lib/api/http.ts`, `lib/api/viagens.ts`, `lib/api/vans.ts`, `lib/api/motoristas.ts`

## Fluxo de dados — Criação com alocação

```
NovaViagemPage
  └── ViagemForm (modificado)
        ├── Montagem (useEffect):
        │     GET api/gerente/vans       → lista de vans do frotista
        │     GET api/gerente/motoristas → lista de motoristas do frotista
        ├── state: vans[], motoristas[], vanSelecionada, motoristaSelecionado
        ├── Seção existente: campos da viagem (nome, data, local, etc.)
        ├── NOVA Seção "Van":
        │     ├── Label: "Selecione a van para esta viagem" (obrigatório)
        │     ├── Dropdown com todas as vans do frotista
        │     │     Formato: "{nome} — {modelo} ({placa}) — {capacidade} lugares"
        │     ├── Se vans.length === 0: mensagem + link para /gerente/vans/nova
        │     └── Validação: van é obrigatória
        └── NOVA Seção "Motorista":
              ├── Label: "Selecione o motorista para esta viagem" (obrigatório)
              ├── Dropdown com todos os motoristas do frotista
              │     Formato: "{nome} — CNH: {cnh}"
              ├── Se motoristas.length === 0: mensagem + link para /gerente/motoristas/novo
              └── Validação: motorista é obrigatório

  Fluxo de submit (handleSubmit no NovaViagemPage):
    1. ViagemForm.onSubmit(data) → dispatch para o page
    2. NovaViagemPage.handleSubmit:
       a. setLoading(true); setErroEtapa(null)
       b. criada = await criarViagemGerente(data)
          ── se rejeitar: setErroEtapa("criar", err.message); return
       c. alocada = await alocarVan(criada.viagemId, vanId)
          -- se rejeitar: setErroEtapa("alocar-van", err.message); return
          -- (viagem ja existe, aparece como "Pendente" na lista)
          -- Extrair viagemVanId: const viagemVanId = alocada.vans[alocada.vans.length - 1].viagemVanId
       d. await alocarMotorista(criada.viagemId, viagemVanId, { motoristaId, viagemVanId })
          -- se rejeitar: setErroEtapa("alocar-motorista", err.message); return
          -- (van alocada, mas sem motorista — gerenciar depois)
       e. router.push("/gerente/viagens?sucesso=criada")
    3. Loading: botão "Criando viagem e alocando recursos…" desabilitado
    4. Erro por etapa:
       ── "criar": banner "Erro ao criar viagem: {msg}" (nada foi salvo)
       ── "alocar-van": banner "Viagem criada, mas não foi possível alocar a van: {msg}" + link para /gerente/viagens/{id}/alocar
       ── "alocar-motorista": banner "Van alocada, mas não foi possível alocar o motorista: {msg}" + link para /gerente/viagens/{id}/alocar
```

## Fluxo de dados — Tela de gerenciamento (pós-criação)

```
AlocacaoClient  (envolto em <GerenteGuard> no page.tsx)
  ├── Montagem (useEffect, 3 fetches em paralelo):
  │     GET api/gerente/vans              → lista de vans do frotista
  │     GET api/gerente/motoristas        → lista de motoristas do frotista
  │     GET api/gerente/viagens/{id}      → viagem com vans e motoristas já alocados
  ├── state: vans[], motoristas[], viagem, loading, error
  ├── Estados de carregamento/erro:
  │     ├── loading=true: skeleton com 3 seções (cabeçalho, vans, motoristas)
  │     ├── error (rede): banner "Não foi possível carregar. Tentar novamente." + botão
  │     ├── error (404): "Viagem não encontrada" + link para /gerente/viagens
  │     └── error (403): redirecionar para /entrar
  ├── Seção "Vans alocadas":
  │     ├── Lista de vans alocadas:
  │     │     ├── Modelo + Placa + Capacidade
  │     │     ├── Motorista alocado (ou "Nenhum")
  │     │     └── Botão "Remover van" → DELETE .../{viagemVanId}
  │     └── Se ainda há vans disponíveis:
  │           ├── Dropdown com vans NÃO alocadas
  │           └── Botão "Alocar van" → POST .../{id}/alocar-van
  └── Seção "Motoristas":
        └── Para cada van alocada:
              ├── Se sem motorista: dropdown + botão "Alocar motorista"
              └── Se com motorista: nome + botão "Remover motorista"
```

## Tipos TypeScript

```ts
// lib/api/types.ts — adicionar:
type AlocarVanRequest = {
  vanId: string;  // GUID da van
};

type AlocarMotoristaRequest = {
  motoristaId: string;   // GUID do motorista (Usuario com Tipo=Motorista)
  viagemVanId: string;   // GUID do ViagemVan — obtido apos alocarVan()
};

// lib/api/viagens.ts — novas funcoes (assinaturas exatas):
// alocarVan(viagemId: string, vanId: string): Promise<ViagemGerenteResponse>
//   → POST api/gerente/viagens/{id}/alocar-van  body: { vanId }
//   → Retorna ViagemGerenteResponse. O viagemVanId da van recem-alocada:
//     response.vans[response.vans.length - 1].viagemVanId
//
// alocarMotorista(viagemId: string, viagemVanId: string, body: AlocarMotoristaRequest): Promise<ViagemGerenteResponse>
//   → POST api/gerente/viagens/{id}/alocar-motorista/{viagemVanId}  body: { motoristaId, viagemVanId }
//
// removerVanAlocada(viagemId: string, viagemVanId: string): Promise<ViagemGerenteResponse>
//   → DELETE api/gerente/viagens/{id}/alocar-van/{viagemVanId}
//
// removerMotoristaAlocado(viagemId: string, viagemVanId: string): Promise<boolean>
//   → DELETE api/gerente/viagens/{id}/remover-motorista/{viagemVanId}
```

### Interface do ViagemForm (modificada)

```ts
// app/gerente/viagens/components/ViagemForm.tsx — novas props:
type ViagemFormProps = {
  viagem?: ViagemGerenteResponse;           // existente (edição)
  onSubmit: (data: CriarViagemRequest) => Promise<void>;  // existente
  submitLabel: string;                      // existente
  // ── NOVAS props para Spec 50 ──
  vans: VanResponse[];                      // lista de vans do frotista
  motoristas: MotoristaResponse[];          // lista de motoristas do frotista
  vanId: string;                            // van selecionada (controlado externamente)
  motoristaId: string;                      // motorista selecionado (controlado externamente)
  onVanChange: (vanId: string) => void;     // callback
  onMotoristaChange: (motoristaId: string) => void;  // callback
  loadingRecursos: boolean;                 // true enquanto vans/motoristas carregam
  erroRecursos: string | null;              // mensagem de erro no fetch de recursos
  onRetryRecursos: () => void;              // callback para tentar novamente o fetch
};
```

## Decisões

1. **Alocação no formulário de criação**: A van e o motorista são selecionados no mesmo formulário de criação da viagem. O submit faz 3 chamadas em sequência: cria viagem → aloca van → aloca motorista. Isso garante atomicidade visual (loading único) e evita que o gerente esqueça de alocar.

2. **Dropdowns obrigatórios**: Ambos os campos (van e motorista) são obrigatórios no formulário de criação. Não faz sentido criar viagem sem recursos alocados.

3. **Dropdown com todas as vans/motoristas do frotista**: Diferente da versão anterior (que filtrava os já alocados), no momento da criação a viagem ainda não existe, então todas as vans e motoristas estão disponíveis. O backend é responsável por validar se o recurso já está em uso em outra viagem.

4. **Tela de gerenciamento mantida**: Embora a alocação inicial ocorra na criação, a tela `/gerente/viagens/{id}/alocar` continua existindo para ajustes pós-criação (trocar motorista, adicionar/remover vans).

5. **Badge de completude na Spec 20**: A lista de viagens mostra badge verde "Completa" se `vans.length > 0 && todas têm motorista`, senão badge amarelo "Pendente".

6. **Sem modal para alocar**: A ação de alocar na tela de gerenciamento é instantânea (dropdown + botão). Só a remoção usa modal de confirmação.

## Rotas

| Rota | Page | Auth | Descrição |
|------|------|------|-----------|
| `/gerente/viagens/nova` | Criação + alocação | JWT + Gerente | Formulário com dropdowns |
| `/gerente/viagens/{id}/alocar` | Gerenciamento | JWT + Gerente | Tela de ajuste de alocações |

## Estados visuais

| Estado | UI |
|--------|-----|
| Loading da criação | Botão "Criando viagem e alocando recursos…" desabilitado + spinner |
| Loading inicial (fetch vans/motoristas) | Dropdowns com skeleton/spinner + "Carregando vans…" / "Carregando motoristas…" |
| Erro de rede no fetch inicial | Banner "Nao foi possivel carregar os recursos. Tente novamente." + botao "Tentar novamente" |
| Erro 401/403 no fetch inicial | Redirecionar para /entrar (token expirado) ou exibir "Acesso negado" |
| Sem vans cadastradas | Dropdown desabilitado + "Nenhuma van cadastrada. Cadastre uma van primeiro." + link |
| Sem motoristas cadastrados | Dropdown desabilitado + "Nenhum motorista cadastrado. Cadastre um motorista primeiro." + link |
| Validacao: van nao selecionada | Erro no campo: "Selecione uma van" |
| Validacao: motorista nao selecionado | Erro no campo: "Selecione um motorista" |
| Erro ao criar viagem (passo 1) | Banner "Erro ao criar viagem: {msg}" (nada foi salvo, formulario permanece preenchido) |
| Erro ao alocar van (passo 2) | Banner "Viagem criada, mas nao foi possivel alocar a van: {msg}" + link para tela de gerenciamento |
| Erro ao alocar motorista (passo 3) | Banner "Van alocada, mas nao foi possivel alocar o motorista: {msg}" + link para tela de gerenciamento |
| Sucesso total | Redireciona para /gerente/viagens?sucesso=criada |
| Van alocada com sucesso (gerenciamento) | Van aparece na lista + toast verde "Van alocada com sucesso" |
| Motorista alocado com sucesso (gerenciamento) | Motorista aparece vinculado a van + toast verde "Motorista alocado com sucesso" |
| Erro ao remover van com reservas | Toast vermelho: "Esta van possui reservas ativas" |
| Erro 400 (van em outra viagem) | Toast vermelho: "Esta van ja esta alocada em outra viagem" |
| Erro 404 (recurso removido) | Toast vermelho: "Recurso nao encontrado. Atualize a pagina." |
| Loading do gerenciamento | Skeleton com 3 secoes (cabecalho, vans, motoristas) |
| Erro ao carregar gerenciamento | Banner "Nao foi possivel carregar. Tentar novamente." + botao |
| Viagem nao encontrada (404 no GET) | "Viagem nao encontrada" + link para /gerente/viagens |
| Tudo completo (gerenciamento) | Banner verde "Viagem pronta para divulgacao" |
| Pendente (falta van ou motorista) | Banner amarelo "Aloque recursos para completar a viagem" |
| Badge na lista (Spec 20) | Verde "Completa" se vans.length > 0 && todas tem motorista, amarelo "Pendente" caso contrario |
