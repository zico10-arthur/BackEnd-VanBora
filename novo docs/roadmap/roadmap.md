# Roadmap — Portal do Gerente (Prioridade 1)

> **Documento pai:** [visão.md](../visão.md) — toda spec aqui referenciada entrega valor direto ao **Vendedor (Gerente/Frotista)**.

---

## Tabela de Specs

| Spec # | Nome | Status | O que faz | Referência no visão.md |
|--------|------|--------|-----------|------------------------|
| 10 | Dashboard do Gerente | 🔴 pendente | Portal inicial pós-login com visão geral das viagens ativas, reservas e ocupação | Item 10 — Dashboard |
| 20 | Gerenciamento de Viagens | 🔴 pendente | Criar, listar, editar, cancelar viagens (evento + van + motorista + rota) | Item 6 — Viagens |
| 30 | Gerenciamento de Vans | 🟢 auditada | Listar, cadastrar, editar, remover vans com validação de placa, ano, capacidade | Item 3 — Vans |
| 40 | Gerenciamento de Motoristas | 🔴 pendente | Listar, cadastrar, editar, remover motoristas com validação de CPF, CNH, idade | Item 2 — Motoristas |
| 50 | Alocação de Recursos à Viagem | 🔴 pendente | Alocar/remover van e motorista a uma viagem específica | Item 6 — Viagens |
| 60 | Relatório Financeiro da Viagem | 🔴 pendente | Visualizar receita, ocupação, break-even e lista de embarque por viagem | Item 10 — Dashboard |

---

## Status Legend

| Status | Significado |
|--------|-------------|
| 🔴 pendente | Spec criada, frontend não iniciado |
| 🟡 implementada | Código frontend concluído e integrado com backend |
| 🟢 auditada | Testada, revisada e aprovada em produção |

---

## Gráfico de Dependências

```
Spec 10 — Dashboard do Gerente
  ├── depende de: Spec 20 (precisa listar viagens)
  ├── depende de: Spec 30 (precisa listar vans)
  └── depende de: Spec 60 (exibe indicadores financeiros)

Spec 20 — Gerenciamento de Viagens
  ├── depende de: Spec 30 (precisa de vans cadastradas)
  ├── depende de: Spec 40 (precisa de motoristas cadastrados)
  └── depende de: Spec 50 (alocação de van/motorista)

Spec 30 — Gerenciamento de Vans
  └── sem dependências (CRUD isolado)

Spec 40 — Gerenciamento de Motoristas
  └── sem dependências (CRUD isolado)

Spec 50 — Alocação de Recursos à Viagem
  ├── depende de: Spec 20 (precisa de viagem criada)
  ├── depende de: Spec 30 (precisa de vans cadastradas)
  └── depende de: Spec 40 (precisa de motoristas cadastrados)

Spec 60 — Relatório Financeiro
  └── depende de: Spec 20 (precisa de viagem com reservas)
```

---

## Ordem Recomendada de Implementação

```
1. Spec 30 — Vans           ← sem dependências, é a base
2. Spec 40 — Motoristas     ← sem dependências, é a base
3. Spec 20 — Viagens        ← depende de Vans e Motoristas
4. Spec 50 — Alocação       ← depende de Viagens, Vans e Motoristas
5. Spec 60 — Relatório      ← depende de Viagens (com reservas)
6. Spec 10 — Dashboard      ← depende de tudo acima (visão consolidada)
```
