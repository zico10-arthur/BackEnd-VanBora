---
name: Dashboard do Gerente
status: auditada
references:
  - visão.md: Item 10 — Dashboard
  - roadmap.md: Spec 10
---

# Spec 10 — Dashboard do Gerente

## O que é?

Portal inicial que o **Vendedor (Gerente/Frotista)** vê imediatamente após fazer login. É a tela de boas-vindas que consolida as informações mais importantes do seu negócio no Vanbora.

## Problema que resolve para o usuário final

Hoje o gerente **não tem nenhuma tela após o login**. Ele se autentica e fica perdido — não sabe quantas viagens tem ativas, quantas reservas recebeu, qual a ocupação das suas vans, nem qual a receita gerada. O Dashboard resolve isso entregando uma **visão consolidada e acionável** do seu negócio em uma única tela.

## Critérios de Aceite

### AC1 — Cards de resumo
A tela deve exibir cards com indicadores principais:
- **Viagens ativas**: quantidade de viagens com status "Agendada"
- **Total de reservas**: soma de todas as reservas (confirmadas + pendentes)
- **Ocupação média**: percentual médio de assentos ocupados entre todas as vans alocadas
- **Receita total**: soma do valor de todas as reservas confirmadas

### AC2 — Lista de próximas viagens
Abaixo dos cards, exibir uma tabela/listagem com as **próximas 5 viagens** ordenadas por data de partida, mostrando:
- Nome do evento
- Data/hora da partida
- Van alocada (modelo + placa)
- Assentos vendidos / capacidade
- Status (Agendada, Cancelada, Concluída)

### AC3 — Acesso rápido
Cada card e cada item da lista deve ser clicável e levar à tela correspondente:
- Card "Viagens ativas" → lista de viagens (Spec 20)
- Card "Total de reservas" → lista de reservas por viagem
- Item da lista de viagens → relatório financeiro da viagem (Spec 60)

### AC4 — Estado vazio
Se o gerente não tiver nenhuma viagem cadastrada, exibir um estado vazio com CTA (Call to Action) para "Criar primeira viagem" que leva à Spec 20.

### AC5 — Proteção de rota
A rota deve exigir autenticação (JWT). Se o token estiver ausente/expirado, redirecionar para `/entrar`.

## Endpoints consumidos

| Método | Rota | Uso |
|--------|------|-----|
| GET | `api/gerente/viagens` | Listar viagens do gerente para montar cards e tabela |

## Fora do escopo

- Admin dashboard (será tratado em spec separada na Prioridade 4)
- Dashboard do comprador (já parcialmente coberto por `/minhas-reservas`)
- Gráficos complexos (V2)
