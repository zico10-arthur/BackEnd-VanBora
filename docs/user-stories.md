# VanBora — User Stories, Critérios de Aceitação e Exemplos

> **Propósito:** Guiar a implementação com histórias de usuário claras, cenários Gherkin e exemplos de requisição/resposta para cada endpoint.

---

## US01 — Cadastro de Gerente

**Como** um gerente de van
**Quero** me cadastrar na plataforma VanBora como gerente (Usuario com Tipo = Gerente)
**Para** gerenciar minhas vans e criar viagens

> **Modelo:** O cadastro de gerente cria um **Usuario** com `Tipo = Gerente` via `Usuario.CriarGerente()`. Se o CPF já existir como Passageiro, o sistema faz `UpgradeParaGerente()` — o Tipo muda para Gerente de forma irreversível. O login é único (email + senha do Usuario), e o gerente também pode reservar assentos em viagens de outros gerentes.

### Cenários de Aceitação

```gherkin
Cenário: Cadastro bem-sucedido (novo Gerente)
  Dado que não existe um Usuario com o CPF informado
  Quando envio meus dados (nome, cpf, email, telefone, senha, slug)
  Então um novo Usuario é criado com Tipo = Gerente
  E recebo um token JWT com tipos = ["Gerente"]

Cenário: Cadastro com CPF existente (upgrade de Passageiro)
  Dado que já existe um Usuario do tipo Passageiro com CPF "12345678909"
  Quando envio dados de cadastro de gerente com o mesmo CPF
  Então o Usuario existente é atualizado via UpgradeParaGerente()
  E o Tipo muda para Gerente (irreversível)
  E recebo um token JWT com tipos = ["Gerente"]

Cenário: Email duplicado
  Dado que já existe um Usuario com email "contato@transpabc.com"
  Quando tento cadastrar outro Usuario com o mesmo email
  Então recebo um erro 409 Conflict
  E a mensagem "Email já cadastrado"

Cenário: Slug duplicado
  Dado que já existe um gerente com slug "transp-abc"
  Quando tento cadastrar com o mesmo slug
  Então recebo um erro 409 Conflict
  E a mensagem "Slug já cadastrado"

Cenário: Dados inválidos
  Dado que envio um email sem formato válido
  Quando tento cadastrar
  Então recebo um erro 400 Bad Request
  E a lista de validações falhas
```

### Exemplo

**Requisição:** `POST /api/auth/gerente/registrar`
```json
{
  "nome": "Transportadora ABC",
  "cpf": "12345678909",
  "slug": "transp-abc",
  "email": "contato@transpabc.com",
  "telefone": "11999999999",
  "senha": "MinhaSenha123",
  "chavePix": "contato@transpabc.com"
}
```

**Resposta (201):**
```json
{
  "usuarioId": "u1u2u3u4-...",
  "nome": "Transportadora ABC",
  "email": "contato@transpabc.com",
  "telefone": "11999999999",
  "cpf": "12345678909",
  "slug": "transp-abc",
  "taxaPlataforma": 5.0,
  "gratuito": false,
  "chavePix": "contato@transpabc.com",
  "tipo": "Gerente",
  "token": "eyJhbGciOi..."
}
```

---

## US02 — Login (único com TipoUsuario)

**Como** um usuário cadastrado
**Quero** fazer login na plataforma com meu email e senha únicos
**Para** acessar minhas funcionalidades (reservar, gerenciar, etc.)

> **Login único:** O login é feito com Email + Senha do **Usuario**. O JWT contém o TipoUsuario como claim "tipos". Cada usuário tem exatamente um tipo.

### Cenários de Aceitação

```gherkin
Cenário: Login bem-sucedido
  Dado que estou cadastrado com email "joao@email.com"
  Quando informo meu email e senha corretos
  Então recebo um token JWT válido
  E o JWT contém meu tipo = "Gerente"

Cenário: Senha incorreta
  Dado que estou cadastrado
  Quando informo a senha errada
  Então recebo um erro 401 Unauthorized

Cenário: Conta inativa
  Dado que minha conta foi desativada pelo admin
  Quando tento fazer login
  Então recebo um erro 403 Forbidden
  E a mensagem "Conta desativada"

Cenário: Login como Passageiro
  Dado que sou passageiro com email "joao@email.com"
  Quando faço login com meu email e senha
  Então recebo um token JWT com tipos = ["Passageiro"]
  E posso reservar assentos
```

### Exemplo

**Requisição:** `POST /api/auth/login`
```json
{
  "email": "joao@email.com",
  "senha": "MinhaSenha123"
}
```

**Resposta (200):**
```json
{
  "usuarioId": "u1u2u3u4-...",
  "nome": "João Silva",
  "email": "joao@email.com",
  "tipo": "Gerente",
  "token": "eyJhbGciOi..."
}
```

---

## US03 — Cadastro de Passageiro

**Como** um passageiro
**Quero** me cadastrar na plataforma VanBora
**Para** fazer reservas em viagens

> **Modelo:** O cadastro cria um **Usuario** com `Tipo = Passageiro` via `Usuario.CriarPassageiro()`. Login único com email + senha do Usuario. Se o CPF já existir no sistema com `SenhaHash = null` (ex: Motorista cadastrado pelo Gerente sem login), o sistema entende como **ativação de conta pendente** — mantém o Tipo = Motorista, apenas preenche email e senha para que o motorista possa fazer login e também reservar assentos.

### Cenários de Aceitação

```gherkin
Cenário: Cadastro bem-sucedido
  Dado que não existe Usuario com este email
  Quando envio meus dados (nome, email, cpf, telefone, senha)
  Então um novo Usuario é criado com Tipo = Passageiro
  E recebo um token JWT com tipo = "Passageiro"
  E já posso reservar assentos imediatamente

Cenário: CPF já existe como Motorista sem login (ativação de conta)
  Dado que existe um Usuario com CPF "12345678909" e Tipo = Motorista mas SenhaHash = null
  Quando me registro com o mesmo CPF informando email e senha
  Então o Usuario existente é reutilizado (Tipo permanece Motorista)
  E o email e senha são definidos (conta ativada)
  E recebo um token JWT com tipo = "Motorista"
  E agora posso fazer login e reservar assentos

Cenário: Email duplicado
  Dado que já existe um Usuario com email "joao@email.com"
  Quando tento cadastrar com o mesmo email
  Então recebo um erro 409 Conflict
  E a mensagem "Email já cadastrado"

Cenário: CPF inválido
  Dado que envio um CPF com dígitos inválidos
  Quando tento cadastrar
  Então recebo um erro 400
  E a mensagem "CPF inválido"
```

### Exemplo

**Requisição:** `POST /api/auth/registrar`
```json
{
  "nome": "João Silva",
  "email": "joao@email.com",
  "cpf": "12345678909",
  "telefone": "11988888888",
  "senha": "SenhaDoJoao123"
}
```

**Resposta (201):**
```json
{
  "usuarioId": "u1u2u3u4-...",
  "nome": "João Silva",
  "email": "joao@email.com",
  "cpf": "12345678909",
  "telefone": "11988888888",
  "tipo": "Passageiro",
  "token": "eyJhbGciOi..."
}
```

---

## US04 — Login de Usuário (mesmo endpoint do US02)

> **Nota:** O login é **único** para todos os tipos de usuário. Veja US02 para detalhes completos. O mesmo endpoint `POST /api/auth/login` atende Passageiros, Gerentes e Admins. Cada usuário tem exatamente **um Tipo**.

### Cenários de Aceitação

```gherkin
Cenário: Login como passageiro
  Dado que me cadastrei como usuario com email "joao@email.com"
  Quando faço login com meu email e senha
  Então recebo um token JWT com tipo = "Passageiro"
  E posso criar reservas

Cenário: Login como gerente
  Dado que me cadastrei como gerente com email "contato@transpabc.com"
  Quando faço login
  Então recebo um token JWT com tipo = "Gerente"
  E posso gerenciar viagens
```

### Exemplo

**Requisição:** `POST /api/auth/login` (mesmo do US02)
```json
{
  "email": "joao@email.com",
  "senha": "SenhaDoJoao123"
}
```

**Resposta (200):**
```json
{
  "usuarioId": "b2c3d4e5-...",
  "nome": "João Silva",
  "email": "joao@email.com",
  "tipo": "Passageiro",
  "token": "eyJhbGciOi..."
}
```

---

## US05 — Cadastrar Van

**Como** um gerente logado
**Quero** cadastrar uma van no meu perfil
**Para** poder alocá-la em viagens futuras

### Cenários de Aceitação

```gherkin
Cenário: Cadastro bem-sucedido
  Dado que estou logado como gerente
  Quando cadastro uma van com nome, placa, modelo e capacidade
  Então a van é criada e associada ao meu usuario (Gerente)

Cenário: Placa duplicada
  Dado que já cadastrei uma van com placa "ABC1D23"
  Quando tento cadastrar outra van com a mesma placa
  Então recebo um erro 409 Conflict

Cenário: Capacidade inválida
  Dado que informo capacidade menor que 2 (motorista + 1 assento)
  Quando tento cadastrar
  Então recebo um erro 400
```

### Exemplo

**Requisição:** `POST /api/gerente/vans`
```json
{
  "nome": "Van 1 - Mercedes",
  "placa": "ABC1D23",
  "modelo": "Mercedes-Benz Sprinter",
  "capacidade": 16
}
```

**Resposta (201):**
```json
{
  "id": "c3d4e5f6-...",
  "nome": "Van 1 - Mercedes",
  "placa": "ABC1D23",
  "modelo": "Mercedes-Benz Sprinter",
  "capacidade": 16,
  "ativo": true,
  "criadoEm": "2026-05-04T12:00:00Z"
}
```

---

## US06 — Criar Viagem

**Como** um gerente logado
**Quero** criar uma viagem para um evento
**Para** que passageiros possam reservar assentos

### Cenários de Aceitação

```gherkin
Cenário: Criar viagem
  Dado que estou logado como gerente
  Quando crio uma viagem com preço de assento e possuiIngresso = false
  Então a viagem é criada com status "Agendada"

Cenário: Criar viagem com ingresso
  Dado que estou logado como gerente
  Quando crio uma viagem com possuiIngresso = true
  Então a viagem é criada com status "Agendada"

Cenário: Data da partida após data do evento
  Dado que informo dataPartida maior que dataEvento
  Quando tento criar a viagem
  Então recebo um erro 400
  E a mensagem "Data de partida deve ser anterior à data do evento"
```

### Exemplo

**Requisição:** `POST /api/gerente/viagens`
```json
{
  "nomeEvento": "Flamengo x Palmeiras - Brasileirão",
  "dataEvento": "2026-06-15T21:30:00Z",
  "localEvento": "Maracanã - Rio de Janeiro",
  "dataPartida": "2026-06-15T17:00:00Z",
  "localPartida": "Praça da Sé - São Paulo",
  "precoAssento": 89.90,
  "possuiIngresso": true
}
```

**Resposta (201):**
```json
{
  "id": "d4e5f6a7-...",
  "nomeEvento": "Flamengo x Palmeiras - Brasileirão",
  "dataEvento": "2026-06-15T21:30:00Z",
  "localEvento": "Maracanã - Rio de Janeiro",
  "dataPartida": "2026-06-15T17:00:00Z",
  "localPartida": "Praça da Sé - São Paulo",
  "precoAssento": 89.90,
  "possuiIngresso": true,
  "status": "Agendada",
  "criadoEm": "2026-05-04T12:00:00Z"
}
```

---

## US07 — Alocar Van na Viagem

**Como** um gerente logado
**Quero** alocar uma ou mais vans em uma viagem
**Para** aumentar a capacidade de passageiros conforme a demanda

### Cenários de Aceitação

```gherkin
Cenário: Van já alocada na mesma viagem
  Dado que a van já está alocada nesta viagem
  Quando tento alocá-la novamente
  Então recebo um erro 409

Cenário: Viagem já iniciada
  Dado que a viagem já está em andamento
  Quando tento alocar uma van
  Então recebo um erro 400
```

### Exemplo

**Requisição:** `POST /api/gerente/viagens/{viagemId}/alocar-van`
```json
{
  "vanId": "c3d4e5f6-..."
}
```

**Resposta (201):**
```json
{
  "id": "e5f6a7b8-...",
  "viagemId": "d4e5f6a7-...",
  "vanId": "c3d4e5f6-...",
  "vanNome": "Van 1 - Mercedes",
  "capacidade": 16,
  "assentosDisponiveis": 15
}
```

---

## US08 — Visualizar Viagens Disponíveis

**Como** um passageiro (logado ou não)
**Quero** ver as viagens disponíveis
**Para** escolher um evento e reservar assentos

### Cenários de Aceitação

```gherkin
Cenário: Listar viagens futuras
  Dado que existem viagens agendadas
  Quando consulto a lista de viagens
  Então vejo apenas viagens com status "Agendada"
  E ordenadas por data do evento (mais próximas primeiro)

Cenário: Filtrar por local
  Dado que existem viagens para diferentes locais
  Quando filtro por "Maracanã"
  Então vejo apenas viagens para aquele local

Cenário: Viagem sem vans alocadas
  Dado que uma viagem não tem nenhuma van alocada
  Quando consulto os detalhes
  Então a viagem aparece como "Sem vans disponíveis"
```

### Exemplo

**Requisição:** `GET /api/viagens`

**Resposta (200):**
```json
[
  {
    "id": "d4e5f6a7-...",
    "nomeEvento": "Flamengo x Palmeiras - Brasileirão",
    "dataEvento": "2026-06-15T21:30:00Z",
    "localEvento": "Maracanã - Rio de Janeiro",
    "dataPartida": "2026-06-15T17:00:00Z",
    "localPartida": "Praça da Sé - São Paulo",
    "precoAssento": 89.90,
    "possuiIngresso": true,
    "gerenteNome": "Transportadora ABC",
    "totalVans": 2,
    "totalAssentosDisponiveis": 30
  }
]
```

**Requisição:** `GET /api/viagens/{id}`

**Resposta (200):**
```json
{
  "id": "d4e5f6a7-...",
  "nomeEvento": "Flamengo x Palmeiras - Brasileirão",
  "dataEvento": "2026-06-15T21:30:00Z",
  "localEvento": "Maracanã - Rio de Janeiro",
  "dataPartida": "2026-06-15T17:00:00Z",
  "localPartida": "Praça da Sé - São Paulo",
  "precoAssento": 89.90,
  "possuiIngresso": true,
  "status": "Agendada",
  "gerente": {
    "id": "a1b2c3d4-...",
    "nome": "Transportadora ABC"
  },
  "vans": [
    {
      "viagemVanId": "e5f6a7b8-...",
      "vanId": "c3d4e5f6-...",
      "vanNome": "Van 1 - Mercedes",
      "capacidade": 16,
      "assentosDisponiveis": 15,
      "assentosLivres": [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15]
    }
  ]
}
```

---

## US09 — Criar Reserva

**Como** um usuário logado
**Quero** reservar um ou mais assentos em uma viagem
**Para** garantir meu lugar no evento

> **Nota sobre ingresso:** Após o pagamento da reserva ser confirmado, se a viagem tiver `PossuiIngresso = true`, o sistema exibe o contato do gerente para o passageiro tratar a compra do ingresso diretamente. **Qualquer usuário logado pode reservar** — independente do Tipo (Passageiro, Gerente, Motorista ou Admin).

### Cenários de Aceitação

```gherkin
Cenário: Reserva de 1 assento
  Dado que estou logado
  Quando reservo 1 assento (número 5)
  Então a reserva é criada com status "PendentePagamento"
  E o valorTotal = precoAssento × 1
  E a taxaPlataforma é calculada

Cenário: Reserva de 2 assentos
  Dado que estou logado
  Quando reservo 2 assentos (números 5 e 6)
  Então a reserva contém 2 itens
  E o valorTotal = precoAssento × 2

Cenário: Assento já ocupado
  Dado que o assento 5 já foi reservado por outro usuário
  Quando tento reservar o assento 5
  Então recebo um erro 409
  E a mensagem "Assento 5 já está ocupado"

Cenário: Primeiros 2 clientes são 0800
  Dado que sou um dos 2 primeiros clientes do gerente
  Quando crio uma reserva
  Então a taxaPlataforma = 0

Cenário: Reserva para terceiros
  Dado que estou logado como usuário
  Quando crio uma reserva com 3 passageiros (eu + 2 amigos)
  Então cada itemReserva tem seus próprios dados de passageiro
  E a reserva fica vinculada a mim como responsável

Cenário: Reserva expira em 10 minutos
  Dado que crio uma reserva com sucesso
  Quando não realizo o pagamento em até 10 minutos
  Então a reserva expira automaticamente
  E os assentos são liberados para outros usuários
```

### Exemplo

**Requisição:** `POST /api/reservas`
```json
{
  "viagemVanId": "e5f6a7b8-...",
  "itens": [
    {
      "numeroAssento": 5,
      "nomePassageiro": "João Silva",
      "emailPassageiro": "joao@email.com",
      "telefonePassageiro": "11988888888",
      "cpfPassageiro": "12345678909"
    },
    {
      "numeroAssento": 6,
      "nomePassageiro": "Maria Souza",
      "emailPassageiro": "maria@email.com",
      "telefonePassageiro": "11977777777",
      "cpfPassageiro": "98765432100"
    }
  ]
}
```

**Resposta (201):**
```json
{
  "id": "f6a7b8c9-...",
  "viagemVanId": "e5f6a7b8-...",
  "status": "PendentePagamento",
  "valorTotal": 179.80,
  "taxaPlataforma": 8.99,
  "codigoPix": "000201010212...",
  "expiraEm": "2026-05-04T12:15:00Z",
  "criadoEm": "2026-05-04T12:00:00Z",
  "itens": [
    {
      "id": "item1-...",
      "numeroAssento": 5,
      "precoAssento": 89.90,
      "nomePassageiro": "João Silva"
    },
    {
      "id": "item2-...",
      "numeroAssento": 6,
      "precoAssento": 89.90,
      "nomePassageiro": "Maria Souza"
    }
  ]
}
```

---

## US10 — Pagar Reserva (Assento)

**Como** um usuário logado com reserva pendente
**Quero** gerar o QR Code Pix e efetuar o pagamento do assento
**Para** confirmar minha reserva

> **Nota:** Este pagamento cobre **apenas o assento**. Após a confirmação, se a viagem tiver `PossuiIngresso = true`, o sistema exibe o contato do gerente para o passageiro.

### Cenários de Aceitação

```gherkin
Cenário: Gerar QR Code Pix
  Dado que tenho uma reserva com status "PendentePagamento"
  Quando solicito o pagamento
  Então recebo o código Pix para pagamento

Cenário: Webhook de pagamento confirmado
  Dado que o gateway Pix confirma o pagamento
  Quando recebo o webhook
  Então a reserva muda para status "Confirmada"
  E um email de confirmação é enviado

Cenário: Tentar pagar reserva já confirmada
  Dado que a reserva já está paga
  Quando tento gerar novo QR Code
  Então recebo um erro 400
  E a mensagem "Reserva já confirmada"

Cenário: Tentar pagar reserva expirada
  Dado que a reserva expirou
  Quando tento gerar QR Code
  Então recebo um erro 400
```

### Exemplo

**Requisição:** `POST /api/reservas/{id}/pagar`

**Resposta (200):**
```json
{
  "id": "f6a7b8c9-...",
  "status": "PendentePagamento",
  "codigoPix": "000201010212...",
  "qrCodeBase64": "iVBORw0KGgo...",
  "expiraEm": "2026-05-04T12:15:00Z"
}
```

**Webhook (do Gateway → API):** `POST /api/webhooks/pix`
```json
{
  "transacaoId": "tx_123456",
  "status": "CONFIRMADO",
  "valor": 329.80
}
```

---

## US11 — Cancelar Reserva

**Como** um usuário logado
**Quero** cancelar minha reserva
**Para** liberar os assentos caso não possa ir

### Cenários de Aceitação

```gherkin
Cenário: Cancelar reserva pendente
  Dado que a reserva está "PendentePagamento"
  Quando cancelo a reserva
  Então os assentos são liberados
  E a reserva fica com status "Cancelada"

Cenário: Cancelar reserva confirmada
  Dado que a reserva está "Confirmada"
  Quando cancelo a reserva
  Então a reserva fica com status "Cancelada"
  E os assentos são liberados

Cenário: Cancelar reserva já finalizada
  Dado que a viagem já foi concluída
  Quando tento cancelar a reserva
  Então recebo um erro 400
```

### Exemplo

**Requisição:** `POST /api/reservas/{id}/cancelar`

**Resposta (200):**
```json
{
  "id": "f6a7b8c9-...",
  "status": "Cancelada",
  "canceladoEm": "2026-05-04T14:00:00Z"
}
```

---

## US12 — Relatório Financeiro da Viagem

**Como** um gerente logado
**Quero** ver o relatório financeiro de uma viagem
**Para** saber quanto faturarei e quanto será a taxa do VanBora

### Cenários de Aceitação

```gherkin
Cenário: Relatório com reservas confirmadas
  Dado que a viagem tem reservas confirmadas
  Quando solicito o relatório
  Então vejo total de assentos vendidos, faturamento bruto, taxa VanBora e faturamento líquido

Cenário: Relatório sem reservas
  Dado que a viagem não tem reservas
  Quando solicito o relatório
  Então vejo todos os valores zerados
```

### Exemplo

**Requisição:** `GET /api/gerente/viagens/{id}/relatorio`

**Resposta (200):**
```json
{
  "viagemId": "d4e5f6a7-...",
  "nomeEvento": "Flamengo x Palmeiras - Brasileirão",
  "vansAlocadas": 2,
  "totalAssentos": 30,
  "assentosVendidos": 12,
  "assentosDisponiveis": 18,
  "reservasConfirmadas": 5,
  "faturamentoBruto": 1078.80,
  "taxaPlataforma": 53.94,
  "faturamentoLiquido": 1024.86
}
```

---

## US13 — Admin: Gerenciar Gerentes

**Como** um administrador VanBora
**Quero** gerenciar os gerentes cadastrados
**Para** ativar/desativar contas e configurar taxas

### Cenários de Aceitação

```gherkin
Cenário: Listar gerentes
  Dado que existem gerentes cadastrados
  Quando consulto a lista
  Então vejo todos os gerentes com seus dados

Cenário: Definir taxa personalizada
  Dado que sou admin
  Quando altero a taxaPlataforma de um gerente para 3%
  Então as novas reservas usarão esta taxa
  E as reservas existentes mantêm a taxa anterior

Cenário: Ativar 0800 (gratuito)
  Dado que sou admin
  Quando ativo gratuito = true para um gerente
  Então as reservas desse gerente não terão taxa VanBora
```

### Exemplo

**Requisição:** `GET /api/admin/gerentes`

**Resposta (200):**
```json
[
  {
    "id": "a1b2c3d4-...",
    "nome": "Transportadora ABC",
    "slug": "transp-abc",
    "email": "contato@transpabc.com",
    "ativo": true,
    "taxaPlataforma": 5.0,
    "gratuito": false,
    "totalVans": 3,
    "totalViagens": 5,
    "criadoEm": "2026-05-01T10:00:00Z"
  }
]
```

**Requisição:** `PUT /api/admin/gerentes/{id}`
```json
{
  "taxaPlataforma": 3.0,
  "gratuito": false,
  "ativo": true
}
```

---

## US14 — Ver Minhas Reservas

**Como** um usuário logado
**Quero** ver minhas reservas
**Para** acompanhar o status e detalhes

### Cenários de Aceitação

```gherkin
Cenário: Listar minhas reservas
  Dado que estou logado
  Quando consulto minhas reservas
  Então vejo apenas as reservas que fiz
  E ordenadas da mais recente para a mais antiga

Cenário: Reserva com detalhes da viagem
  Dado que tenho uma reserva
  Quando vejo os detalhes
  Então vejo os dados da viagem, van, assentos e status
```

### Exemplo

**Requisição:** `GET /api/reservas/minhas`

**Resposta (200):**
```json
[
  {
    "id": "f6a7b8c9-...",
    "viagem": {
      "nomeEvento": "Flamengo x Palmeiras - Brasileirão",
      "dataEvento": "2026-06-15T21:30:00Z",
      "localEvento": "Maracanã - Rio de Janeiro"
    },
    "van": {
      "nome": "Van 1 - Mercedes"
    },
    "status": "Confirmada",
    "valorTotal": 329.80,
    "itens": [
      {
        "numeroAssento": 5,
        "nomePassageiro": "João Silva"
      }
    ],
    "criadoEm": "2026-05-04T12:00:00Z"
  }
]
```

---

## US15 — Remover Van da Viagem

**Como** um gerente
**Quero** remover uma van de uma viagem
**Para** ajustar a capacidade caso necessário

### Cenários de Aceitação

```gherkin
Cenário: Remover van sem reservas
  Dado que a van não tem reservas na viagem
  Quando removo a van
  Então a van é desalocada da viagem

Cenário: Remover van com reservas confirmadas
  Dado que a van tem 2 reservas confirmadas
  Quando removo a van
  Então as 2 reservas são reembolsadas integralmente
  E a van é desalocada da viagem

Cenário: Remover van com reservas pendentes
  Dado que a van tem 1 reserva pendente de pagamento
  Quando removo a van
  Então a reserva pendente é cancelada sem reembolso
  E a van é desalocada da viagem
```

### Exemplo

**Requisição:** `DELETE /api/gerente/viagens/{viagemId}/remover-van/{viagemVanId}`

**Resposta (200):**
```json
{
  "mensagem": "Van removida da viagem com sucesso",
  "reservasReembolsadas": 2,
  "reservasCanceladas": 1,
  "totalReembolsado": 179.80
}
```

---

## US16 — Fluxo 0800 (Primeiros Gerentes)

**Como** um novo gerente na plataforma
**Quero** saber se sou um dos primeiros gerentes a se cadastrar
**Para** não pagar taxa caso esteja entre os 2 primeiros

> **Regra:** Os **2 primeiros gerentes** a se cadastrarem na plataforma recebem `gratuito = true` automaticamente (taxa = 0 em todas as reservas). O 3º gerente em diante começa com `gratuito = false` e `taxaPlataforma` definida pelo Admin. O Admin pode ajustar `taxaPlataforma` e `gratuito` individualmente para qualquer gerente.

### Cenários de Aceitação

```gherkin
Cenário: Primeiro gerente do sistema é 0800
  Dado que não existem gerentes cadastrados
  Quando o primeiro gerente se cadastra
  Então ele recebe gratuito = true automaticamente
  E taxaPlataforma = 0
  E todas as suas reservas terão taxa = 0

Cenário: Segundo gerente do sistema é 0800
  Dado que existe 1 gerente cadastrado
  Quando o segundo gerente se cadastra
  Então ele recebe gratuito = true automaticamente
  E taxaPlataforma = 0

Cenário: Terceiro gerente paga taxa
  Dado que já existem 2 gerentes com gratuito = true
  Quando o terceiro gerente se cadastra
  Então ele recebe gratuito = false
  E taxaPlataforma = 5.0 (ou valor definido pelo Admin)

Cenário: Admin ajusta taxa de um gerente
  Dado que existe um gerente com taxaPlataforma = 5.0
  Quando o Admin altera a taxa para 3.0
  Então as novas reservas usarão taxa = 3.0
  E as reservas existentes mantêm a taxa anterior
```

---

## US17 — Atualizar Van

**Como** um gerente logado
**Quero** atualizar os dados de uma van
**Para** corrigir informações como nome, modelo ou placa

### Cenários de Aceitação

```gherkin
Cenário: Atualizar nome e modelo com sucesso
  Dado que tenho uma van cadastrada
  Quando atualizo o nome e o modelo
  Então os dados são alterados com sucesso

Cenário: Tentar alterar capacidade
  Dado que tenho uma van cadastrada com capacidade 16
  Quando tento alterar a capacidade para 20
  Então recebo um erro 400
  E a mensagem "Capacidade não pode ser alterada após a criação"

Cenário: Van não encontrada
  Dado que o id da van não existe
  Quando tento atualizar
  Então recebo um erro 404 Not Found
```

### Exemplo

**Requisição:** `PUT /api/gerente/vans/{id}`
```json
{
  "nome": "Van 1 - Mercedes Modificada",
  "placa": "ABC1D23",
  "modelo": "Mercedes-Benz Sprinter 2025"
}
```

**Resposta (200):**
```json
{
  "id": "c3d4e5f6-...",
  "nome": "Van 1 - Mercedes Modificada",
  "placa": "ABC1D23",
  "modelo": "Mercedes-Benz Sprinter 2025",
  "capacidade": 16,
  "ativo": true
}
```

---

## US18 — Atualizar Usuario

**Como** um usuário logado
**Quero** atualizar meus dados pessoais (nome, email, telefone)
**Para** manter meu cadastro sempre atualizado

> **Modelo:** Nome, email e telefone são dados do **Usuario** (entidade única). A senha é alterada em endpoint separado (`POST /api/auth/alterar-senha` — ver US21). CPF é imutável.

### Cenários de Aceitação

```gherkin
Cenário: Atualizar nome com sucesso
  Dado que estou logado
  Quando atualizo meu nome
  Então o nome é alterado no Usuario

Cenário: Atualizar email
  Dado que estou logado
  Quando atualizo meu email
  Então o email é alterado e passa a ser usado para login

Cenário: Email duplicado
  Dado que já existe outro Usuario com o email informado
  Quando tento alterar meu email para o mesmo valor
  Então recebo um erro 409 Conflict
  E a mensagem "Email já cadastrado"

Cenário: Tentar alterar CPF
  Dado que estou logado
  Quando tento alterar meu CPF
  Então recebo um erro 400
  E a mensagem "CPF não pode ser alterado"
```

### Exemplo

**Requisição:** `PUT /api/auth/usuario`
```json
{
  "nome": "João Silva Atualizado",
  "email": "joao.novo@email.com",
  "telefone": "11977777777"
}
```

**Resposta (200):**
```json
{
  "usuarioId": "u1u2u3u4-...",
  "nome": "João Silva Atualizado",
  "email": "joao.novo@email.com",
  "telefone": "11977777777",
  "cpf": "12345678909"
}
```

---

## US19 — Atualizar Slug do Gerente

**Como** um gerente logado
**Quero** atualizar o nome da empresa (slug) do meu cadastro
**Para** manter meu perfil público sempre atualizado

> **Modelo:** O slug é um campo opcional diretamente no **Usuario** (válido apenas para Tipo = Gerente). Nome, email e telefone são dados do Usuario e devem ser alterados via `PUT /api/auth/usuario` (ver US18). Senha é alterada via `POST /api/auth/alterar-senha` (ver US21).

### Cenários de Aceitação

```gherkin
Cenário: Atualizar slug com sucesso
  Dado que estou logado como gerente
  Quando atualizo meu slug
  Então o slug do Usuario é alterado

Cenário: Slug duplicado
  Dado que já existe outro gerente com o mesmo slug
  Quando tento alterar para o mesmo slug
  Então recebo um erro 409 Conflict
  E a mensagem "Slug já cadastrado"

Cenário: Tentar alterar dados do Usuario pelo endpoint errado
  Dado que estou logado como gerente
  Quando envio email ou telefone neste endpoint
  Então recebo um erro 400
  E a mensagem "Use PUT /api/auth/usuario para alterar dados do usuário"
```

### Exemplo

**Requisição:** `PUT /api/auth/gerente/slug`
```json
{
  "slug": "transp-abc-novo"
}
```

**Resposta (200):**
```json
{
  "usuarioId": "u1u2u3u4-...",
  "slug": "transp-abc-novo",
  "tipo": "Gerente"
}
```

---

## US20 — Desativar Conta (Soft Delete)

**Como** um usuário logado
**Quero** desativar minha conta
**Para** remover meu acesso à plataforma, podendo reativar depois se quiser

> **Modelo:** Todas as exclusões são **soft delete** (lógicas). O registro permanece no banco com `Ativo = false`. Como o login é baseado no **Usuario** (entidade única), desativar o Usuario (`Usuario.Ativo = false`) impede qualquer login imediatamente. A reativação pode ser feita pelo Admin.

### Cenários de Aceitação

```gherkin
Cenário: Solicitar desativação com sucesso
  Dado que estou logado
  Quando solicito a desativação da minha conta
  Então recebo um código de confirmação no meu email

Cenário: Confirmar desativação com código válido
  Dado que solicitei a desativação e recebi o código
  Quando informo o código correto
  Então o Usuario é marcado como Ativo = false
  E recebo uma confirmação por email
  E não consigo mais fazer login

Cenário: Código inválido
  Dado que solicitei a desativação
  Quando informo um código incorreto
  Então recebo um erro 400
  E a mensagem "Código de confirmação inválido"

Cenário: Gerente com reservas ativas
  Dado que sou um gerente com viagens com reservas confirmadas
  Quando tento desativar minha conta
  Então recebo um erro 400
  E a mensagem "Não é possível desativar conta com reservas ativas"
```

### Exemplo

**Requisição:** `POST /api/auth/solicitar-exclusao`
```json
{
  "email": "joao@email.com"
}
```

**Resposta (200):**
```json
{
  "mensagem": "Código de confirmação enviado para joao@email.com",
  "expiraEm": "2026-05-05T19:15:00Z"
}
```

**Requisição:** `POST /api/auth/confirmar-exclusao`
```json
{
  "email": "joao@email.com",
  "codigo": "ABC123"
}
```

**Resposta (200):**
```json
{
  "mensagem": "Conta excluída com sucesso"
}
```

---

## US21 — Alterar Senha

**Como** qualquer usuário logado (passageiro ou gerente)
**Quero** alterar minha senha
**Para** manter minha conta segura

### Cenários de Aceitação

```gherkin
Cenário: Alterar senha com sucesso
  Dado que estou logado
  Quando informo minha senha atual e uma nova senha
  Então a senha é alterada com sucesso

Cenário: Senha atual incorreta
  Dado que estou logado
  Quando informo a senha atual errada
  Então recebo um erro 401
  E a mensagem "Senha atual incorreta"

Cenário: Nova senha fraca
  Dado que estou logado
  Quando informo uma nova senha muito curta
  Então recebo um erro 400
  E a mensagem "Senha deve ter no mínimo 6 caracteres"
```

### Exemplo

**Requisição:** `POST /api/auth/alterar-senha`
```json
{
  "senhaAtual": "MinhaSenhaAntiga123",
  "novaSenha": "MinhaNovaSenha456"
}
```

**Resposta (200):**
```json
{
  "mensagem": "Senha alterada com sucesso"
}
```

---

## US22 — Admin: Buscar Usuarios

**Como** um administrador VanBora
**Quero** pesquisar usuarios por nome, CPF ou tipo
**Para** encontrar rapidamente uma pessoa no sistema

> **Modelo:** A busca é feita na entidade **Usuario** (unificada). Cada Usuario tem exatamente um Tipo. A resposta mostra os dados do Usuario com o Tipo diretamente na raiz.

### Cenários de Aceitação

```gherkin
Cenário: Buscar usuario por nome
  Dado que existem usuarios cadastrados
  Quando pesquiso por "João"
  Então vejo todos os usuarios com "João" no nome
  E cada resultado mostra o tipo e dados do usuario

Cenário: Buscar usuario por CPF
  Dado que existe um usuario com CPF "12345678909"
  Quando pesquiso por "12345678909"
  Então vejo apenas aquele usuario
  E seus dados completos

Cenário: Buscar gerentes especificamente
  Dado que existem usuarios com Tipo = Gerente
  Quando pesquiso por "Transportadora" em /api/admin/gerentes
  Então vejo apenas usuarios com Tipo = Gerente
  E que correspondem ao nome "Transportadora"

Cenário: Nenhum resultado
  Dado que não existem usuarios com aquele nome
  Quando pesquiso
  Então recebo uma lista vazia
```

### Exemplo

**Requisição:** `GET /api/admin/usuarios?search=João`

**Resposta (200):**
```json
[
  {
    "usuarioId": "u1u2u3u4-...",
    "nome": "João Silva",
    "cpf": "12345678909",
    "email": "joao@email.com",
    "tipo": "Passageiro",
    "ativo": true,
    "totalReservas": 3,
    "criadoEm": "2026-05-01T10:00:00Z"
  }
]
```

**Requisição:** `GET /api/admin/gerentes?search=Transportadora`

**Resposta (200):**
```json
[
  {
    "usuarioId": "u1u2u3u4-...",
    "nome": "Transportadora ABC",
    "cpf": "12345678909",
    "email": "contato@transpabc.com",
    "tipo": "Gerente",
    "slug": "transp-abc",
    "ativo": true,
    "taxaPlataforma": 5.0,
    "totalViagens": 5,
    "criadoEm": "2026-04-01T10:00:00Z"
  }
]
```

---

## US23 — Admin: Ver Histórico de Reservas de Passageiro ou Gerente

**Como** um administrador VanBora
**Quero** visualizar o histórico completo de reservas de qualquer passageiro ou gerente
**Para** auditar o uso da plataforma e oferecer suporte

### Cenários de Aceitação

```gherkin
Cenário: Ver histórico de reservas de um passageiro
  Dado que existe um passageiro com reservas passadas e futuras
  Quando o admin acessa o histórico desse passageiro
  Então visualiza todas as reservas ordenadas da mais recente para a mais antiga
  E cada reserva mostra o status, data, valor e detalhes da viagem

Cenário: Ver histórico de viagens de um gerente
  Dado que um gerente possui viagens realizadas
  Quando o admin acessa o histórico desse gerente
  Então visualiza todas as viagens com data, vans envolvidas e total arrecadado

Cenário: Admin busca passageiro e vê o histórico
  Dado que o admin pesquisou por um passageiro
  Quando clica no perfil do passageiro
  Então o histórico de reservas aparece junto com os dados do usuário

Cenário: Histórico vazio
  Dado que um usuário nunca fez nenhuma reserva
  Quando o admin acessa o histórico
  Então recebe uma lista vazia
```

### Exemplo

**Requisição:** `GET /api/admin/usuarios/{id}/reservas`

**Resposta (200):**
```json
[
  {
    "id": "r1s2t3u4-...",
    "status": "Confirmada",
    "valorTotal": 120.00,
    "taxaPlataforma": 6.00,
    "criadaEm": "2026-05-10T14:30:00Z",
    "viagem": {
      "id": "v1w2x3y4-...",
      "origem": "São Paulo, SP",
      "destino": "Rio de Janeiro, RJ",
      "dataPartida": "2026-05-20T08:00:00Z"
    },
    "itens": [
      {
        "assento": 1,
        "passageiroNome": "João Silva",
        "passageiroDocumento": "12345678909",
        "valor": 60.00
      },
      {
        "assento": 2,
        "passageiroNome": "Maria Souza",
        "passageiroDocumento": "98765432100",
        "valor": 60.00
      }
    ]
  }
]
```

**Requisição:** `GET /api/admin/gerentes/{id}/reservas`

**Resposta (200):**
```json
[
  {
    "viagemId": "v1w2x3y4-...",
    "origem": "São Paulo, SP",
    "destino": "Rio de Janeiro, RJ",
    "dataPartida": "2026-05-20T08:00:00Z",
    "totalReservas": 15,
    "totalArrecadado": 900.00,
    "taxaPlataforma": 45.00,
    "statusViagem": "Realizada"
  }
]
```

---

## US24 — Gerente: Cadastrar Motorista

**Como** um gerente de van
**Quero** cadastrar motoristas no sistema
**Para** alocá-los nas viagens e controlar minha frota

> **Modelo:** O Motorista é um **Usuario** com `Tipo = Motorista` criado via `Usuario.CriarMotorista()`. O Gerente informa nome, CPF, telefone e CNH. O sistema cria um novo Usuario com `Tipo = Motorista`, `Email = null`, `SenhaHash = null` e `CriadoPorUsuarioId = id do Gerente`. O Motorista **não tem login inicialmente**, mas pode depois **ativar a conta** registrando-se pelo endpoint de passageiro (US03) com o mesmo CPF — neste caso o Tipo permanece Motorista, apenas email e senha são preenchidos.

### Cenários de Aceitação

```gherkin
Cenário: Cadastrar motorista com CPF novo
  Dado que estou logado como gerente
  E que NÃO existe um Usuario com o CPF informado
  Quando informo nome, CPF, telefone e CNH do motorista
  Então um novo Usuario é criado com Tipo = Motorista via CriarMotorista()
  E o CriadoPorUsuarioId é vinculado ao gerente logado
  E recebo os dados do motorista criado

Cenário: CPF já existe como Usuario (com email)
  Dado que estou logado como gerente
  E que existe um Usuario com CPF "98765432100" e Email != null
  Quando informo nome, CPF, telefone e CNH
  Então recebo um erro 409
  E a mensagem "CPF já cadastrado no sistema"

Cenário: Motorista já cadastrado pelo gerente
  Dado que já existe um Usuario com Tipo = Motorista e CPF "98765432100" vinculado ao meu gerente
  Quando tento cadastrar outro motorista com o mesmo CPF
  Então recebo um erro 409
  E a mensagem "Motorista já cadastrado"

Cenário: CPF inválido
  Dado que estou logado como gerente
  Quando informo um CPF com dígitos inválidos
  Então recebo um erro 400
  E a mensagem "CPF inválido"

Cenário: Campos obrigatórios ausentes
  Dado que estou logado como gerente
  Quando não informo o nome do motorista
  Então recebo um erro 400
  E a mensagem "Nome é obrigatório"

Cenário: Listar motoristas cadastrados
  Dado que estou logado como gerente
  E que cadastrei 3 motoristas
  Quando solicito a lista de motoristas
  Então vejo os 3 motoristas cadastrados (Tipo = Motorista, CriadoPorUsuarioId = meu id)
  E cada motorista mostra nome, CPF, telefone, CNH e status

Cenário: Remover motorista sem viagens futuras
  Dado que estou logado como gerente
  E que o motorista não está alocado em nenhuma viagem futura
  Quando removo o motorista
  Então o Usuario com Tipo = Motorista é removido (exclusão lógica ou física)

Cenário: Remover motorista alocado em viagem futura
  Dado que estou logado como gerente
  E que o motorista está alocado em uma viagem futura
  Quando tento remover o motorista
  Então recebo um erro 422
  E a mensagem "Motorista possui viagens futuras. Remova a alocação primeiro"
```

### Exemplo

**Requisição:** `POST /api/gerente/motoristas`
```json
{
  "nome": "Carlos Santos",
  "cpf": "98765432100",
  "telefone": "11977777777",
  "cnh": "12345678901"
}
```

**Resposta (201):**
```json
{
  "usuarioId": "u1u2u3u4-...",
  "nome": "Carlos Santos",
  "cpf": "98765432100",
  "telefone": "11977777777",
  "cnh": "12345678901",
  "tipo": "Motorista",
  "ativo": true,
  "criadoPorUsuarioId": "g1g2g3g4-...",
  "criadoEm": "2026-05-05T10:00:00Z"
}
```

**Requisição:** `GET /api/gerente/motoristas`

**Resposta (200):**
```json
[
  {
    "usuarioId": "u1u2u3u4-...",
    "nome": "Carlos Santos",
    "cpf": "98765432100",
    "telefone": "11977777777",
    "cnh": "12345678901",
    "tipo": "Motorista",
    "ativo": true,
    "criadoPorUsuarioId": "g1g2g3g4-...",
    "criadoEm": "2026-05-05T10:00:00Z"
  }
]
```

---

## US25 — Gerente: Alocar Motorista na Viagem

**Como** um gerente de van
**Quero** alocar um motorista a uma van específica em uma viagem
**Para** definir quem vai dirigir cada van

### Cenários de Aceitação

```gherkin
Cenário: Alocar motorista na van da viagem
  Dado que existe uma viagem com uma van alocada
  E que existe um motorista ativo cadastrado
  Quando aloco o motorista naquela van da viagem
  Então o motorista é vinculado com sucesso
  E a van da viagem agora possui um motorista

Cenário: Re-alocar motorista
  Dado que a van da viagem já possui um motorista alocado
  Quando aloco um motorista diferente
  Então o motorista é substituído com sucesso

Cenário: Motorista inativo
  Dado que o motorista está marcado como inativo
  Quando tento alocá-lo em uma van
  Então recebo um erro 400
  E a mensagem "Motorista inativo"

Cenário: Motorista de outro gerente
  Dado que o motorista pertence a outro gerente
  Quando tento alocá-lo em minha van
  Então recebo um erro 404
```

### Exemplo

**Requisição:** `POST /api/gerente/viagens/{viagemId}/alocar-motorista/{viagemVanId}`
```json
{
  "motoristaId": "m1n2o3p4-..."
}
```

**Resposta (200):**
```json
{
  "mensagem": "Motorista alocado com sucesso",
  "viagemVanId": "v1w2x3y4-...",
  "motorista": {
    "id": "m1n2o3p4-...",
    "nome": "Carlos Santos"
  }
}
```

---

## US26 — Gerente: Listar e Cancelar Viagens

**Como** um gerente logado
**Quero** listar minhas viagens e cancelar viagens quando necessário
**Para** gerenciar minha oferta e reembolsar passageiros em caso de cancelamento

### Cenários de Aceitação

```gherkin
Cenário: Listar viagens do gerente
  Dado que estou logado como gerente
  E que criei 3 viagens (2 futuras e 1 concluída)
  Quando solicito a lista de viagens
  Então vejo todas as minhas viagens
  E cada viagem mostra nome do evento, data, status e total de reservas

Cenário: Cancelar viagem sem reservas
  Dado que estou logado como gerente
  E que a viagem não possui nenhuma reserva
  Quando cancelo a viagem
  Então a viagem é cancelada com status "Cancelada"
  E não há reembolso a processar

Cenário: Cancelar viagem com reservas confirmadas
  Dado que estou logado como gerente
  E que a viagem possui 5 reservas confirmadas
  Quando cancelo a viagem
  Então todas as 5 reservas são reembolsadas integralmente
  E o status de cada reserva muda para "Cancelada"
  E a viagem é marcada como "Cancelada"

Cenário: Cancelar viagem com reservas pendentes
  Dado que estou logado como gerente
  E que a viagem possui 2 reservas pendentes de pagamento
  Quando cancelo a viagem
  Então as reservas pendentes são canceladas sem reembolso
  E a viagem é marcada como "Cancelada"

Cenário: Cancelar viagem já concluída
  Dado que a viagem já foi realizada
  Quando tento cancelá-la
  Então recebo um erro 400
  E a mensagem "Viagem já foi concluída"
```

### Exemplo

**Requisição:** `GET /api/gerente/viagens`

**Resposta (200):**
```json
[
  {
    "id": "d4e5f6a7-...",
    "nomeEvento": "Flamengo x Palmeiras - Brasileirão",
    "dataEvento": "2026-06-15T21:30:00Z",
    "localEvento": "Maracanã - Rio de Janeiro",
    "status": "Agendada",
    "totalReservas": 5,
    "totalReservasConfirmadas": 3,
    "criadoEm": "2026-05-04T12:00:00Z"
  }
]
```

**Requisição:** `DELETE /api/gerente/viagens/{id}`

**Resposta (200):**
```json
{
  "mensagem": "Viagem cancelada com sucesso",
  "reservasReembolsadas": 3,
  "reservasCanceladas": 2,
  "totalReembolsado": 269.70
}
```

---



## Glossário de Respostas HTTP

| Código | Significado | Uso |
|--------|-------------|-----|
| 200 | OK | Sucesso em GET, PUT, POST de pagar/cancelar |
| 201 | Created | Sucesso em POST de criação (reserva, van, viagem, cadastro) |
| 400 | Bad Request | Dados inválidos, validação falhou |
| 401 | Unauthorized | Token ausente ou inválido |
| 403 | Forbidden | Conta inativa, sem permissão |
| 404 | Not Found | Recurso não encontrado |
| 409 | Conflict | Assento já ocupado, email duplicado, van já alocada |
| 422 | Unprocessable Entity | Regra de negócio violada (ex: remover van com reservas) |
