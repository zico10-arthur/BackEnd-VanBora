# VanBora — User Stories, Critérios de Aceitação e Exemplos

> **Propósito:** Guiar a implementação com histórias de usuário claras, cenários Gherkin e exemplos de requisição/resposta para cada endpoint.

---

## US01 — Cadastro de Gerente

**Como** um gerente de van
**Quero** me cadastrar na plataforma VanBora
**Para** gerenciar minhas vans e criar viagens

### Cenários de Aceitação

```gherkin
Cenário: Cadastro bem-sucedido
  Dado que sou um gerente de van
  Quando envio meus dados (nome, email, telefone, senha)
  Então minha conta é criada com status ativo
  E recebo um token JWT para acesso

Cenário: Email duplicado
  Dado que já existe um gerente com email "gerente@email.com"
  Quando tento cadastrar com o mesmo email
  Então recebo um erro 409 Conflict
  E a mensagem "Email já cadastrado"

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
  "slug": "transp-abc",
  "email": "contato@transpabc.com",
  "telefone": "11999999999",
  "senha": "MinhaSenha123"
}
```

**Resposta (201):**
```json
{
  "id": "a1b2c3d4-...",
  "nome": "Transportadora ABC",
  "slug": "transp-abc",
  "email": "contato@transpabc.com",
  "telefone": "11999999999",
  "ativo": true,
  "taxaPlataforma": 5.0,
  "gratuito": false,
  "criadoEm": "2026-05-04T12:00:00Z",
  "token": "eyJhbGciOi..."
}
```

---

## US02 — Login de Gerente

**Como** um gerente de van cadastrado
**Quero** fazer login na plataforma
**Para** acessar minhas funcionalidades de gestão

### Cenários de Aceitação

```gherkin
Cenário: Login bem-sucedido
  Dado que estou cadastrado com email "contato@transpabc.com"
  Quando informo meu email e senha corretos
  Então recebo um token JWT válido

Cenário: Senha incorreta
  Dado que estou cadastrado
  Quando informo a senha errada
  Então recebo um erro 401 Unauthorized

Cenário: Conta inativa
  Dado que minha conta foi desativada pelo admin
  Quando tento fazer login
  Então recebo um erro 403 Forbidden
  E a mensagem "Conta desativada"
```

### Exemplo

**Requisição:** `POST /api/auth/gerente/login`
```json
{
  "email": "contato@transpabc.com",
  "senha": "MinhaSenha123"
}
```

**Resposta (200):**
```json
{
  "id": "a1b2c3d4-...",
  "nome": "Transportadora ABC",
  "email": "contato@transpabc.com",
  "token": "eyJhbGciOi..."
}
```

---

## US03 — Cadastro de Usuário (Passageiro)

**Como** um passageiro
**Quero** me cadastrar na plataforma VanBora
**Para** fazer reservas em viagens

### Cenários de Aceitação

```gherkin
Cenário: Cadastro bem-sucedido
  Dado que sou um passageiro
  Quando envio meus dados (nome, email, cpf, telefone, senha)
  Então minha conta é criada
  E recebo um token JWT

Cenário: CPF inválido
  Dado que envio um CPF com dígitos inválidos
  Quando tento cadastrar
  Então recebo um erro 400
  E a mensagem "CPF inválido"

Cenário: Email já cadastrado
  Dado que já existe um usuário com aquele email
  Quando tento cadastrar novamente
  Então recebo um erro 409 Conflict
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

---

## US04 — Login de Usuário

**Requisição:** `POST /api/auth/login`
```json
{
  "email": "joao@email.com",
  "senha": "SenhaDoJoao123"
}
```

**Resposta (200):**
```json
{
  "id": "b2c3d4e5-...",
  "nome": "João Silva",
  "email": "joao@email.com",
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
  Então a van é criada e associada ao meu perfil

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
Cenário: Criar viagem sem ingresso
  Dado que estou logado como gerente
  Quando crio uma viagem com preço de assento e possuiIngresso = false
  Então a viagem é criada com status "Agendada"

Cenário: Criar viagem com ingresso
  Dado que estou logado como gerente
  Quando crio uma viagem com preço de assento e preço de ingresso
  Então a viagem é criada com possuiIngresso = true

Cenário: Data da partida após data do evento
  Dado que informo dataPartida maior que dataEvento
  Quando tento criar a viagem
  Então recebo um erro 400
  E a mensagem "Data de partida deve ser anterior à data do evento"

Cenário: Preço de ingresso sem possuiIngresso
  Dado que informo precoIngresso mas possuiIngresso = false
  Quando tento criar a viagem
  Então recebo um erro 400
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
  "possuiIngresso": true,
  "precoIngresso": 150.00
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
  "precoIngresso": 150.00,
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
Cenário: Alocar van com ingressos
  Dado que a viagem possuiIngresso = true
  Quando aloco uma van com quantidadeIngressos = 10
  Então a van é alocada na viagem
  E ingressosDisponiveis = 10

Cenário: Alocar van sem ingressos
  Dado que a viagem possuiIngresso = false
  Quando aloco uma van sem informar ingressos
  Então a van é alocada na viagem
  E ingressosDisponiveis = null

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
  "vanId": "c3d4e5f6-...",
  "quantidadeIngressos": 10
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
  "assentosDisponiveis": 15,
  "quantidadeIngressos": 10,
  "ingressosDisponiveis": 10
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
    "precoIngresso": 150.00,
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
  "precoIngresso": 150.00,
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
      "ingressosDisponiveis": 10,
      "assentosLivres": [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15]
    }
  ]
}
```

---

## US09 — Criar Reserva

**Como** um passageiro logado
**Quero** reservar um ou mais assentos em uma viagem
**Para** garantir meu lugar no evento

### Cenários de Aceitação

```gherkin
Cenário: Reserva de 1 assento sem ingresso
  Dado que estou logado
  Quando reservo 1 assento (número 5) sem ingresso
  Então a reserva é criada com status "PendentePagamento"
  E o valorTotal é calculado (precoAssento × 1)
  E a taxaPlataforma é calculada

Cenário: Reserva de 2 assentos, 1 com ingresso
  Dado que estou logado
  Quando reservo 2 assentos (números 5 e 6), sendo o assento 5 com ingresso
  Então a reserva contém 2 itens
  E o valorTotal = precoAssento×2 + precoIngresso×1

Cenário: Assento já ocupado
  Dado que o assento 5 já foi reservado por outro usuário
  Quando tento reservar o assento 5
  Então recebo um erro 409
  E a mensagem "Assento 5 já está ocupado"

Cenário: Ingresso indisponível
  Dado que a van tem 0 ingressosDisponiveis
  Quando tento reservar com possuiIngresso = true
  Então recebo um erro 400
  E a mensagem "Ingressos esgotados para esta van"

Cenário: Primeiros 2 clientes são 0800
  Dado que sou um dos 2 primeiros clientes do gerente
  Quando crio uma reserva
  Então a taxaPlataforma = 0

Cenário: Reserva para terceiros
  Dado que estou logado como usuário
  Quando crio uma reserva com 3 passageiros (eu + 2 amigos)
  Então cada itemReserva tem seus próprios dados de passageiro
  E a reserva fica vinculada a mim como responsável
```

### Exemplo

**Requisição:** `POST /api/reservas`
```json
{
  "viagemVanId": "e5f6a7b8-...",
  "itens": [
    {
      "numeroAssento": 5,
      "possuiIngresso": true,
      "nomePassageiro": "João Silva",
      "emailPassageiro": "joao@email.com",
      "telefonePassageiro": "11988888888",
      "cpfPassageiro": "12345678909"
    },
    {
      "numeroAssento": 6,
      "possuiIngresso": false,
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
  "valorTotal": 329.80,
  "taxaPlataforma": 16.49,
  "codigoPix": "000201010212...",
  "expiraEm": "2026-05-04T12:15:00Z",
  "criadoEm": "2026-05-04T12:00:00Z",
  "itens": [
    {
      "numeroAssento": 5,
      "possuiIngresso": true,
      "precoAssento": 89.90,
      "precoIngresso": 150.00,
      "nomePassageiro": "João Silva"
    },
    {
      "numeroAssento": 6,
      "possuiIngresso": false,
      "precoAssento": 89.90,
      "precoIngresso": null,
      "nomePassageiro": "Maria Souza"
    }
  ]
}
```

---

## US10 — Pagar Reserva

**Como** um passageiro com reserva pendente
**Quero** gerar o QR Code Pix e efetuar o pagamento
**Para** confirmar minha reserva

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
  E os ingressosDisponiveis são reduzidos na ViagemVan
  E um email é enviado com o link Face ID (se houver ingressos)

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

**Como** um passageiro
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

**Como** um passageiro logado
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
        "possuiIngresso": true,
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

Cenário: Remover van com reservas
  Dado que a van tem reservas confirmadas
  Quando tento remover a van
  Então recebo um erro 400
  E a mensagem "Não é possível remover van com reservas ativas"
```

### Exemplo

**Requisição:** `DELETE /api/gerente/viagens/{viagemId}/remover-van/{viagemVanId}`

**Resposta (200):**
```json
{
  "mensagem": "Van removida da viagem com sucesso"
}
```

---

## US16 — Fluxo 0800 (Primeiros Clientes)

**Como** um novo gerente na plataforma
**Quero** que os 2 primeiros clientes que fizerem reserva não paguem taxa
**Para** incentivar os primeiros passageiros a experimentar o serviço

### Cenários de Aceitação

```ghermin
Cenário: Primeira reserva do gerente é 0800
  Dado que o gerente tem gratuito = true
  Quando o primeiro cliente faz uma reserva
  Então a taxaPlataforma na reserva é calculada como 0
  E o gerente ainda permanece com gratuito = true

Cenário: Segunda reserva do gerente é 0800
  Dado que o gerente tem gratuito = true e já tem 1 reserva
  Quando o segundo cliente faz uma reserva
  Então a taxaPlataforma na reserva é calculada como 0
  E após a confirmação, gratuito muda para false

Cenário: Terceira reserva paga taxa normal
  Dado que o gerente tem gratuito = false
  Quando um cliente faz uma reserva
  Então a taxaPlataforma é calculada normalmente (ex: 5% do valorTotal)
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
