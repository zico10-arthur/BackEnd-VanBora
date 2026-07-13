---
name: Alocação de Recursos à Viagem
status: pendente
references:
  - visão.md: Item 6 — Viagens
  - roadmap.md: Spec 50
---

# Spec 50 — Alocação de Recursos à Viagem

## O que é?

Telas/modalidades que permitem ao **Vendedor (Gerente/Frotista)** alocar (vincular) uma **van** e um **motorista** a uma viagem específica, bem como remover essas alocações.

## Problema que resolve para o usuário final

Uma viagem só fica completa e visível para compradores quando tem **van + motorista alocados**. Sem essa tela, o gerente criaria uma viagem "vazia" — sem veículo e sem condutor. Esta spec fecha o ciclo de publicação da viagem, garantindo que o anúncio tenha todos os recursos necessários.

## Critérios de Aceite

### AC1 — Tela de alocação pós-criação
Após criar uma viagem (Spec 20), o gerente é redirecionado para esta tela, que mostra:
- Nome do evento e data (cabeçalho)
- Seção "Alocar Van" — dropdown/seletor com as vans do gerente que ainda **não estão alocadas** nesta viagem
- Seção "Alocar Motorista" — dropdown/seletor com os motoristas do gerente que ainda **não estão alocados** nesta viagem

### AC2 — Alocar van
- Selecionar uma van do dropdown
- Botão "Alocar van" → chama `POST api/gerente/viagens/{id}/alocar-van`
- Van aparece na lista de "Vans alocadas" com opção de remover

### AC3 — Remover alocação de van
- Botão "Remover" ao lado de cada van alocada
- Modal de confirmação: "Remover esta van da viagem?"
- Chama `DELETE api/gerente/viagens/{id}/alocar-van/{viagemVanId}`
- Se a van tiver reservas: backend retorna 400 → exibir "Esta van possui reservas ativas"

### AC4 — Alocar motorista a uma van específica
- Para cada van alocada, exibir dropdown "Motorista" com os motoristas do gerente
- Botão "Alocar motorista" → chama `POST api/gerente/viagens/{id}/alocar-motorista/{viagemVanId}`
- Motorista aparece vinculado à van na listagem

### AC5 — Remover alocação de motorista
- Botão "Remover motorista" ao lado do motorista alocado
- Chama `DELETE api/gerente/viagens/{id}/remover-motorista/{viagemVanId}`

### AC6 — Acesso pela edição da viagem
- Ao editar uma viagem existente (Spec 20), deve haver um botão/tab "Gerenciar alocações" que leva a esta tela

### AC7 — Indicador visual
- A lista de viagens (Spec 20) deve indicar visualmente se a viagem já tem van e motorista alocados (ex: badge verde "Completa" ou badge amarelo "Pendente alocação")

### AC8 — Estados e proteção
- Loading, feedback de sucesso/erro
- Autenticação JWT, perfil "Gerente"

## Endpoints consumidos

| Método | Rota | Uso |
|--------|------|-----|
| GET | `api/gerente/vans` | Listar vans disponíveis para alocar |
| GET | `api/auth/motoristas` | Listar motoristas disponíveis |
| POST | `api/gerente/viagens/{id}/alocar-van` | Alocar van à viagem |
| DELETE | `api/gerente/viagens/{id}/alocar-van/{viagemVanId}` | Remover van da viagem |
| POST | `api/gerente/viagens/{id}/alocar-motorista/{viagemVanId}` | Alocar motorista à van da viagem |
| DELETE | `api/gerente/viagens/{id}/remover-motorista/{viagemVanId}` | Remover motorista da van |

## Fora do escopo

- Alocação múltipla simultânea (V2)
- Drag and drop (V2)
