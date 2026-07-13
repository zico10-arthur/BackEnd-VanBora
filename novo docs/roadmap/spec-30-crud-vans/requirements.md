---
name: Gerenciamento de Vans
status: pendente
references:
  - visão.md: Item 3 — Vans
  - roadmap.md: Spec 30
---

# Spec 30 — Gerenciamento de Vans

## O que é?

Conjunto de telas que permitem ao **Vendedor (Gerente/Frotista)** cadastrar, listar, editar e remover as vans da sua frota.

## Problema que resolve para o usuário final

O gerente tem um ou mais veículos e precisa **cadastrá-los na plataforma** antes de poder alocá-los a viagens. Sem essa tela, ele não consegue gerenciar sua frota — cada van precisa estar registrada com placa, modelo, ano e capacidade para aparecer como opção na hora de criar uma viagem.

## Critérios de Aceite

### AC1 — Listar vans do gerente
- Tela com grid/cards das vans do gerente autenticado
- Cada card exibe: Modelo, Placa, Ano, Capacidade (X lugares), Status (Ativa/Inativa)
- Ações: Editar, Remover

### AC2 — Cadastrar van
- Formulário com campos:
  - **Modelo** (texto, ex: "Mercedes Sprinter")
  - **Placa** (texto, validação padrão Mercosul AAA0A00)
  - **Ano** (número, não pode ser futuro, mínimo 1990)
  - **Capacidade** (número, 8 a 25 lugares)
- Ao submeter, chamar `POST api/gerente/vans`
- Sucesso: redirecionar para a lista de vans com toast de confirmação

### AC3 — Editar van
- Mesmo formulário do AC2, pré-preenchido
- Chamar `PUT api/gerente/vans/{id}`

### AC4 — Remover van
- Botão "Remover" no card da van
- Modal de confirmação: "Tem certeza? Esta van não poderá mais ser usada em novas viagens."
- Chamar `DELETE api/gerente/vans/{id}`
- Van alocada em viagem futura: backend retorna erro → exibir mensagem explicando que a van está em uso

### AC5 — Validações de negócio
- Placa duplicada → backend retorna 409 Conflict → exibir "Esta placa já está cadastrada"
- Capacidade fora do intervalo 8-25 → backend retorna 400 → exibir mensagem de validação
- Ano futuro → backend retorna 400 → exibir "O ano não pode ser futuro"

### AC6 — Estados e feedback
- Loading state enquanto carrega a lista
- Empty state: "Nenhuma van cadastrada" + CTA "Cadastrar primeira van"
- Error state com botão "Tentar novamente"
- Toast de sucesso/erro nas operações

### AC7 — Proteção de rota
- Autenticação JWT obrigatória
- Apenas perfil "Gerente"

## Endpoints consumidos

| Método | Rota | Uso |
|--------|------|-----|
| GET | `api/gerente/vans` | Listar vans do gerente |
| POST | `api/gerente/vans` | Cadastrar nova van |
| PUT | `api/gerente/vans/{id}` | Editar van |
| DELETE | `api/gerente/vans/{id}` | Remover van (soft delete) |

## Fora do escopo

- Alocar van à viagem (Spec 50)
- Upload de foto da van (V2)
