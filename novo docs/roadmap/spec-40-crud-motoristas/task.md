---
name: Gerenciamento de Motoristas
status: pendente
---

# Tasks — Spec 40: Gerenciamento de Motoristas

## Checklist

### 1. Tipos e API
- [ ] **T1.1** — Adicionar tipos `MotoristaResponse`, `CriarMotoristaRequest` em `lib/api/types.ts`
- [ ] **T1.2** — Criar funções em `lib/api/motoristas.ts` (novo arquivo):
  - `listarMotoristas()` → `GET api/auth/motoristas`
  - `criarMotorista(body)` → `POST api/auth/motorista/registrar`
  - `atualizarMotorista(id, body)` → `PUT api/auth/motorista/{id}`
  - `removerMotorista(id)` → `DELETE api/auth/motorista/{id}`
- [ ] **T1.3** — Criar função utilitária `mascararCPF(cpf: string): string` em `lib/format.ts`

### 2. Componente MotoristaForm (compartilhado)
- [ ] **T2.1** — Criar `app/gerente/motoristas/components/MotoristaForm.tsx`
  - Props: `motorista?: MotoristaResponse`, `onSubmit: (data) => Promise<void>`
  - Campos: nome, cpf, telefone, cnh, categoriaCnh (select), dataNascimento, validadeCnh
  - Validação client-side:
    - nome: min 3 caracteres, regex letras/espaços
    - cpf: 11 dígitos numéricos
    - telefone: 10-11 dígitos
    - cnh: 11 dígitos
    - dataNascimento: idade >= 18 anos (calcular diferença)
    - validadeCnh: data futura
  - Mensagens de erro inline por campo
  - Erro da API: mensagem contextual (CPF duplicado, CNH vencida, etc.)

### 3. Lista de motoristas
- [ ] **T3.1** — Criar `app/gerente/motoristas/MotoristasListClient.tsx`
  - Fetch `GET api/auth/motoristas`
  - Tabela: Nome, CPF (mascarado), Telefone, CNH, Status, Ações
  - Botões: Editar, Remover
  - Estados: loading (skeleton rows), empty, error
- [ ] **T3.2** — Criar `MotoristaRow.tsx` (linha individual com máscara de CPF)

### 4. Página de cadastro
- [ ] **T4.1** — Criar `app/gerente/motoristas/novo/page.tsx`
  - Header + título "Novo motorista"
  - Usa `<MotoristaForm>` com `motorista={undefined}`
  - onSubmit → `criarMotorista()` → toast + redirect

### 5. Página de edição
- [ ] **T5.1** — Criar `app/gerente/motoristas/[motoristaId]/editar/page.tsx`
  - Fetch motorista via `obterMotorista(id)`
  - Usa `<MotoristaForm>` com `motorista={data}`
  - onSubmit → `atualizarMotorista()` → toast + redirect

### 6. Modal de remoção
- [ ] **T6.1** — Criar `RemoverMotoristaModal.tsx`
  - Exibe nome do motorista
  - Confirmação → `removerMotorista(id)` → fecha + recarrega
  - Erro 400 (alocado) → mensagem "Motorista alocado em viagens futuras"

### 7. Lista principal
- [ ] **T7.1** — Criar `app/gerente/motoristas/page.tsx`

### 8. Testes manuais
- [ ] **T8.1** — Cadastrar motorista válido → aparece na lista com CPF mascarado
- [ ] **T8.2** — Cadastrar com CPF inválido → erro de validação
- [ ] **T8.3** — Cadastrar menor de 18 → erro "mínimo 18 anos"
- [ ] **T8.4** — Cadastrar CNH vencida → erro "CNH não pode estar vencida"
- [ ] **T8.5** — CPF duplicado → erro 409
- [ ] **T8.6** — Remover motorista alocado → erro 400
