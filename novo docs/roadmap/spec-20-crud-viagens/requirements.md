---
name: Gerenciamento de Viagens
status: pendente
references:
  - visão.md: Item 6 — Viagens
  - roadmap.md: Spec 20
---

# Spec 20 — Gerenciamento de Viagens

## O que é?

Conjunto de telas que permitem ao **Vendedor (Gerente/Frotista)** criar, visualizar, editar e cancelar viagens — que são os "anúncios" que os compradores veem na plataforma.

## Problema que resolve para o usuário final

O gerente precisa **publicar viagens** para que compradores encontrem e reservem assentos. Sem essa tela, o produto inteiro não funciona — é o core do marketplace. A tela mock atual (`/motorista/nova-viagem`) usa dados falsos de jogos do Botafogo e não se comunica com o backend. Esta spec substitui o mock por uma tela real integrada.

## Critérios de Aceite

### AC1 — Listar viagens do gerente
- Tela com tabela/listagem de **todas as viagens** do gerente autenticado
- Colunas: Evento, Data/Hora Partida, Van alocada, Assentos (vendidos/capacidade), Status, Ações
- Ordenação padrão: data de partida (mais próxima primeiro)
- Cada linha tem ações: Ver detalhes, Editar, Cancelar

### AC2 — Criar viagem
- Formulário com campos:
  - **Evento** (nome, data, local) — campos de texto livre
  - **Origem** (descrição, cidade, estado)
  - **Destino** (descrição, cidade, estado)
  - **Data/Hora de saída** e **Data/Hora de chegada** (datetime pickers)
  - **Preço do assento** (R$, decimal)
  - **Possui ingresso** (checkbox — se o evento cobra ingresso separado)
- Validação: origem ≠ destino, data de saída < data de chegada, data de saída no futuro
- Ao submeter, chamar `POST api/gerente/viagens`
- Sucesso: redirecionar para a tela de alocação (Spec 50)

### AC3 — Editar viagem
- Mesmo formulário do AC2, pré-preenchido com dados atuais
- Só permitido se a viagem estiver com status "Agendada"
- Chamar `PUT api/gerente/viagens/{id}`

### AC4 — Cancelar viagem
- Botão "Cancelar viagem" na lista de viagens (apenas para viagens "Agendada")
- Modal de confirmação: "Tem certeza? Passageiros com reserva serão notificados."
- Chamar `POST api/gerente/viagens/{id}/cancelar`

### AC5 — Estados e feedback
- Loading state (skeleton) enquanto carrega a lista
- Empty state: "Nenhuma viagem cadastrada" + CTA "Criar primeira viagem"
- Error state: mensagem de erro + botão "Tentar novamente"
- Toast/snackbar de sucesso ao criar/editar/cancelar

### AC6 — Proteção de rota
- Todas as rotas exigem autenticação (JWT)
- Apenas usuários com perfil "Gerente" podem acessar

## Endpoints consumidos

| Método | Rota | Uso |
|--------|------|-----|
| GET | `api/gerente/viagens` | Listar viagens do gerente |
| POST | `api/gerente/viagens` | Criar nova viagem |
| PUT | `api/gerente/viagens/{id}` | Editar viagem existente |
| POST | `api/gerente/viagens/{id}/cancelar` | Cancelar viagem |

## Fora do escopo

- Alocar van/motorista (Spec 50)
- Relatório financeiro (Spec 60)
- Lista de embarque (Spec 60)
