---
name: Relatório Financeiro da Viagem
status: pendente
references:
  - visão.md: Item 10 — Dashboard
  - roadmap.md: Spec 60
---

# Spec 60 — Relatório Financeiro da Viagem

## O que é?

Tela de detalhamento que o **Vendedor (Gerente/Frotista)** acessa para ver os indicadores financeiros e a lista de embarque de uma viagem específica.

## Problema que resolve para o usuário final

O gerente precisa saber **se a viagem está dando lucro**, quantos assentos foram vendidos, quanto falta para o break-even, e **quem são os passageiros** que reservaram cada assento. Sem essa tela, ele opera no escuro — não sabe se a viagem se pagou nem quem estará na van no dia do evento.

## Critérios de Aceite

### AC1 — Cabeçalho da viagem
- Nome do evento, data, origem → destino
- Status da viagem (Agendada, Cancelada, Concluída)
- Badge "Viável" ou "Não atingiu break-even"

### AC2 — Cards de indicadores financeiros
- **Receita total** (soma das reservas confirmadas)
- **Assentos vendidos** / capacidade total
- **Quórum mínimo (break-even)**: quantos assentos faltam para cobrir custo
- **Progresso**: barra de progresso mostrando % até o break-even
- **Valor por assento**: preço unitário

### AC3 — Lista de embarque (passageiros)
- Tabela com todos os assentos da van (1 a N)
- Para cada assento:
  - Número do assento
  - Nome do passageiro (ou "Disponível" se vazio)
  - Status do pagamento (Confirmado / Pendente)
  - Contato mascarado (telefone)

### AC4 — Ações
- Botão "Compartilhar link" — copia URL da viagem pública para divulgação
- Botão "Voltar para viagens" — retorna à lista (Spec 20)
- Se viagem cancelada: exibir banner informativo

### AC5 — Estados e proteção
- Loading enquanto carrega o relatório
- Error state com botão "Tentar novamente"
- Autenticação JWT, perfil "Gerente"
- Se a viagem não pertencer ao gerente → 403 → redirecionar

## Endpoints consumidos

| Método | Rota | Uso |
|--------|------|-----|
| GET | `api/gerente/viagens/{id}/relatorio` | Obter indicadores financeiros e lista de embarque |

## Fora do escopo

- Exportar relatório em PDF/CSV (V2)
- Gráficos de barras/pizza (V2)
- Comparativo entre viagens (V2)
