---
name: Alocação de Recursos à Viagem
status: pendente
references:
  - visão.md: Item 6 — Viagens
  - roadmap.md: Spec 50
---

# Spec 50 — Alocação de Recursos à Viagem

## O que é?

A alocação de **van** e **motorista** deve ocorrer **no momento da criação da viagem**, diretamente no formulário de "Nova viagem" (Spec 20). O gerente/frotista seleciona uma de suas vans cadastradas e um de seus motoristas enquanto preenche os dados da viagem, unificando criação + alocação em um único fluxo.

Esta spec também cobre a tela de gerenciamento de alocações para edição/ajuste pós-criação (acessível pela listagem de viagens).

## Problema que resolve para o usuário final

Antes, o gerente criava uma viagem "vazia" e depois precisava navegar para outra tela para alocar recursos. Isso gerava atrito: muitos gerentes esqueciam de alocar e a viagem ficava invisível para compradores.

Agora, **a alocação acontece no mesmo formulário de criação** — o gerente escolhe van e motorista enquanto preenche os dados básicos. A viagem já nasce completa e pronta para divulgação.

## Critérios de Aceite

### AC1 — Seleção de van no formulário de criação
No formulário de "Nova viagem" (`ViagemForm`), após os campos existentes, deve haver:
- Uma seção "Alocar Van" com um **dropdown/seletor** listando **todas as vans cadastradas pelo frotista logado**
- As vans devem mostrar: nome/modelo, placa e capacidade
- Seleção é obrigatória: o formulário não pode ser enviado sem uma van selecionada
- Dados obtidos via `GET api/gerente/vans` (já existente na Spec 30)

### AC2 — Seleção de motorista no formulário de criação
- Seção "Alocar Motorista" com **dropdown/seletor** listando **todos os motoristas cadastrados pelo frotista logado**
- Os motoristas devem mostrar: nome e CNH
- Seleção é obrigatória: o formulário não pode ser enviado sem um motorista selecionado
- Dados obtidos via `GET api/gerente/motoristas` (já existente na Spec 40)

### AC3 — Alocação automática ao criar a viagem
- Ao enviar o formulário de criação:
  1. Chama `POST api/gerente/viagens` (cria a viagem) → obtém `ViagemGerenteResponse` com `viagemId`
  2. Chama `POST api/gerente/viagens/{viagemId}/alocar-van` com `{ vanId: string }` → obtém `ViagemGerenteResponse`
     - O `viagemVanId` da van recém-alocada é extraído como: `response.vans[response.vans.length - 1].viagemVanId`
  3. Chama `POST api/gerente/viagens/{viagemId}/alocar-motorista/{viagemVanId}` com `{ motoristaId: string, viagemVanId: string }` no body
- Os 3 passos são executados em sequência no frontend após o submit
- Loading state cobre todo o processo: botão exibe "Criando viagem e alocando recursos…" e fica desabilitado
- O botão submit é **desabilitado** durante todo o fluxo para prevenir double-submit
- Se qualquer passo falhar, exibe erro específico com a etapa que falhou:
  - Falha no passo 1: "Erro ao criar viagem: {mensagem do backend}"
  - Falha no passo 2: "Viagem criada, mas não foi possível alocar a van: {mensagem}"
  - Falha no passo 3: "Van alocada, mas não foi possível alocar o motorista: {mensagem}"
- Se o passo 2 ou 3 falhar, a viagem **já foi criada** e aparece na lista como "Pendente" (sem alocação completa). O gerente pode completar a alocação pela tela de gerenciamento (`/gerente/viagens/{id}/alocar`)

### AC4 — Estados sem recursos cadastrados
- Se o frotista **não tem vans cadastradas**: dropdown exibe mensagem "Nenhuma van cadastrada" + link para cadastrar van (`/gerente/vans/nova`)
- Se o frotista **não tem motoristas cadastrados**: dropdown exibe mensagem "Nenhum motorista cadastrado" + link para cadastrar motorista (`/gerente/motoristas/novo`)
- O botão "Criar viagem" fica **desabilitado** se não houver van OU motorista disponível

### AC5 — Tela de gerenciamento de alocações (pós-criação)
Para viagens já criadas, deve existir uma tela em `/gerente/viagens/{viagemId}/alocar` que permite:
- Ver vans e motoristas já alocados
- **Remover** alocação de van (com modal de confirmação)
- **Remover** alocação de motorista
- **Adicionar** nova van (se ainda houver vans disponíveis)
- **Adicionar** motorista a uma van existente
- Esta tela é acessada via botão "Gerenciar alocações" na edição da viagem (Spec 20)

### AC6 — Remover alocação de van
- Botão "Remover" ao lado de cada van alocada
- Modal de confirmação: "Remover esta van da viagem?"
- Chama `DELETE api/gerente/viagens/{id}/alocar-van/{viagemVanId}`
- Se a van tiver reservas: backend retorna 400 → exibir "Esta van possui reservas ativas"

### AC7 — Remover alocação de motorista
- Botão "Remover motorista" ao lado do motorista alocado
- Chama `DELETE api/gerente/viagens/{id}/remover-motorista/{viagemVanId}`

### AC8 — Indicador visual na lista de viagens
- A lista de viagens (Spec 20) deve indicar visualmente:
  - Badge verde "Completa" se a viagem tem van + motorista alocados
  - Badge amarelo "Pendente" se falta van ou motorista

### AC9 — Estados e proteção
- **Loading inicial**: enquanto `GET api/gerente/vans` e `GET api/gerente/motoristas` estão carregando, os dropdowns exibem skeleton/spinner com texto "Carregando vans…" e "Carregando motoristas…"
- **Erro de rede nos fetches iniciais**: se `GET api/gerente/vans` ou `GET api/gerente/motoristas` falhar por erro de rede (fetch rejeitado, timeout), exibir banner de erro "Não foi possível carregar os recursos. Tente novamente." com botão "Tentar novamente" que re-executa o fetch
- **Erro HTTP nos fetches iniciais**: se backend retornar 401 (token expirado), redirecionar para `/entrar`. Se retornar 403, exibir "Acesso negado". Se retornar 5xx, exibir banner genérico com botão de retry
- **Validação de formulário**: van e motorista são campos obrigatórios. Se não selecionados, o formulário mostra erro de validação: "Selecione uma van" / "Selecione um motorista"
- **Sucesso**: após criação + alocação completa, redirecionar para `/gerente/viagens?sucesso=criada`
- **Autenticação**: JWT Bearer, perfil "Gerente". Ambos `NovaViagemPage` e `AlocacaoClient` usam `GerenteGuard`
- **Acesso à tela de gerenciamento**: se o usuário acessar `/gerente/viagens/{id}/alocar` para uma viagem inexistente (back-end retorna 404), exibir "Viagem não encontrada". Se a viagem pertence a outro gerente (403), exibir "Acesso negado"

### AC10 — Conflitos e estados inesperados
- **Van já alocada em outra viagem**: back-end retorna 400 → exibir "Esta van já está alocada em outra viagem"
- **Motorista já alocado nesta viagem**: back-end retorna 400 → exibir "Este motorista já está alocado nesta viagem"
- **Van ou motorista removidos entre fetch e submit**: back-end retorna 404 → exibir "Recurso não encontrado. Atualize a página e tente novamente."
- **Van sem motorista após remoção do motorista**: banner da tela de gerenciamento atualiza para amarelo "Aloque motoristas para completar"

## Endpoints consumidos

| Método | Rota | Request Body | Response | Uso |
|--------|------|-------------|----------|-----|
| GET | `api/gerente/vans` | — | `VanResponse[]` | Listar vans do frotista (dropdown) |
| GET | `api/gerente/motoristas` | — | `MotoristaResponse[]` | Listar motoristas do frotista (dropdown) |
| POST | `api/gerente/viagens` | `CriarViagemRequest` | `ViagemGerenteResponse` | Criar viagem (Spec 20) |
| POST | `api/gerente/viagens/{id}/alocar-van` | `{ vanId: string }` | `ViagemGerenteResponse` | Alocar van. `viagemVanId` via `response.vans[last].viagemVanId` |
| POST | `api/gerente/viagens/{id}/alocar-motorista/{viagemVanId}` | `{ motoristaId: string, viagemVanId: string }` | `ViagemGerenteResponse` | Alocar motorista. Body contém ambos os IDs |
| DELETE | `api/gerente/viagens/{id}/alocar-van/{viagemVanId}` | — | `ViagemGerenteResponse` | Remover van da viagem |
| DELETE | `api/gerente/viagens/{id}/remover-motorista/{viagemVanId}` | — | `boolean` | Remover motorista da van |

## Fora do escopo

- Alocação múltipla simultânea (múltiplas vans na criação) — V2
- Drag and drop — V2
- Alterar alocação no formulário de edição da viagem (só na tela dedicada)
