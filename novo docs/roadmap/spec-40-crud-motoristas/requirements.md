---
name: Gerenciamento de Motoristas
status: pendente
references:
  - visão.md: Item 2 — Motoristas
  - roadmap.md: Spec 40
---

# Spec 40 — Gerenciamento de Motoristas

## O que é?

Conjunto de telas que permitem ao **Vendedor (Gerente/Frotista)** cadastrar, listar, editar e remover os motoristas que dirigem suas vans.

## Problema que resolve para o usuário final

O gerente precisa **cadastrar quem vai dirigir** cada van. O motorista é uma pessoa física com CPF, CNH e dados de contato — e o sistema valida que a CNH está válida e o motorista tem 18+ anos. Sem essa tela, o gerente não consegue registrar quem conduz sua frota, e a viagem não pode ser publicada.

## Critérios de Aceite

### AC1 — Listar motoristas
- Tela com tabela/listagem dos motoristas do gerente autenticado
- Colunas: Nome, CPF (mascarado), Telefone, CNH (categoria + vencimento), Status
- Ações: Editar, Remover

### AC2 — Cadastrar motorista
- Formulário com campos:
  - **Nome** (texto, mínimo 3 caracteres)
  - **CPF** (texto, 11 dígitos com validação)
  - **Telefone** (texto, DDD + número)
  - **CNH** (texto, 11 dígitos)
  - **Categoria CNH** (select: A, B, AB, C, D, E)
  - **Data de nascimento** (date picker, mínimo 18 anos)
  - **Validade da CNH** (date picker, deve ser data futura)
- Ao submeter, chamar `POST api/auth/motorista/registrar`
- Sucesso: voltar para a lista com toast

### AC3 — Editar motorista
- Mesmo formulário do AC2, pré-preenchido
- Chamar `PUT api/auth/motorista/{id}`

### AC4 — Remover motorista
- Modal de confirmação
- Chamar `DELETE api/auth/motorista/{id}`
- Se alocado em viagem futura: backend retorna 400 → exibir "Motorista está alocado em viagens futuras"

### AC5 — Validações de negócio
- CPF duplicado → 409 Conflict → "Este CPF já está cadastrado"
- CNH vencida → 400 → "A CNH não pode estar vencida"
- Menor de 18 anos → 400 → "O motorista deve ter pelo menos 18 anos"
- Nome < 3 caracteres → 400 → "O nome deve ter no mínimo 3 caracteres"

### AC6 — Dados sensíveis
- CPF exibido com máscara na listagem (ex: `***.456.789-**`)
- Telefone exibido com máscara parcial

### AC7 — Estados e proteção
- Loading, empty state, error state
- Autenticação JWT, perfil "Gerente"

## Endpoints consumidos

| Método | Rota | Uso |
|--------|------|-----|
| GET | `api/auth/motoristas` | Listar motoristas do gerente |
| POST | `api/auth/motorista/registrar` | Cadastrar motorista |
| PUT | `api/auth/motorista/{id}` | Editar motorista |
| DELETE | `api/auth/motorista/{id}` | Remover motorista |

## Fora do escopo

- Login do motorista (motorista não tem acesso direto ao sistema — é gerenciado pelo gerente)
- Alocar motorista à viagem (Spec 50)
