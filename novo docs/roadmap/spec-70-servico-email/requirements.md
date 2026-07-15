---
name: Serviço de Email
status: auditada
references:
  - visão.md: Infraestrutura de comunicação com usuários
  - roadmap.md: Spec 70
---

# Spec 70 — Serviço de Email

## O que é?

Implementação real do serviço de envio de email via SMTP, substituindo o mock atual que apenas loga no console. Esta spec também implementa os **4 fluxos de envio de email** que o sistema precisa: boas-vindas, redefinição de senha, confirmação de reserva e confirmação de reembolso.

## Problema que resolve para o usuário final

O usuário (Admin, Gerente/Vendedor, Comprador, Motorista) **não recebe emails reais** do sistema. Hoje:
- Ao se cadastrar, não recebe nenhuma confirmação
- Ao esquecer a senha, não consegue redefini-la (o fluxo nem existe)
- Ao fazer uma reserva, não recebe comprovante
- Ao solicitar reembolso, não recebe confirmação

O mock atual só registra no log do servidor, tornando a comunicação por email inexistente.

---

## Casos de Uso (definidos pelo usuário)

| # | Caso de Uso | Quando dispara | Destinatário | Status atual |
|---|-------------|----------------|--------------|--------------|
| 1 | **Email de boas-vindas** | Após cadastro bem-sucedido | Novo usuário (Gerente, Passageiro ou Motorista) | ❌ Não existe |
| 2 | **Redefinição de senha** | Usuário clica "Esqueci minha senha" | Usuário que solicitou reset | ❌ Fluxo não existe |
| 3 | **Confirmação de reserva** | Após criar reserva com sucesso | Comprador (Passageiro) | ❌ Não existe |
| 4 | **Confirmação de reembolso** | Após cancelamento de reserva com reembolso | Comprador (Passageiro) | ❌ Não existe |

---

## Functional Requirements

### FR-001 — Infraestrutura: Envio de email via SMTP

**Descrição:** Substituir o `EmailService` mock por implementação real que conecta a um servidor SMTP.

**Critérios de Aceite:**
- [ ] O método `SendAsync` envia o email de fato via SMTP (`System.Net.Mail.SmtpClient`)
- [ ] Suporte a TLS/SSL com autenticação (usuário + senha)
- [ ] Remetente (`From`) configurável via `appsettings.json`
- [ ] Fallback para mock (log-only) se `EmailSettings` não estiver configurado → sistema não quebra em dev
- [ ] Mantém a mesma assinatura da interface `IEmailService` (com parâmetro novo `isHtml` opcional, default `false`)
- [ ] Timeout de 10 segundos; `CancellationToken` respeitado
- [ ] Falhas retornam `Result<bool>.Failure` (nunca exceção não tratada)

### FR-002 — Configuração SMTP externalizada

**Descrição:** As configurações do servidor SMTP devem ficar no `appsettings.json` (ou secrets do .NET), nunca hardcoded.

**Critérios de Aceite:**
- [ ] Seção `EmailSettings` no `appsettings.json` com: `Host`, `Port`, `UseSsl`, `Username`, `Password`, `FromAddress`, `FromName`
- [ ] Classe `EmailSettings` injetada via `IOptions<EmailSettings>`
- [ ] Propriedade computada `IsConfigured` — se `false`, ativa fallback mock automaticamente

### FR-003 — Caso de Uso 1: Email de boas-vindas

**Descrição:** Após cadastro bem-sucedido de qualquer perfil, enviar email de boas-vindas.

**Critérios de Aceite:**
- [ ] **Gerente:** `AuthService.RegistrarGerente` → após criar usuário com sucesso → envia email de boas-vindas
- [ ] **Passageiro:** `AuthService.RegistrarPassageiroAsync` → após criar usuário com sucesso → envia email de boas-vindas
- [ ] **Motorista:** `MotoristaService.RegistrarMotorista` → após criar motorista com sucesso → envia email de boas-vindas
- [ ] Conteúdo do email de boas-vindas:
  - Assunto: `"Bem-vindo(a) ao VanBora!"`
  - Corpo (texto puro): `"Olá {Nome}, sua conta no VanBora foi criada com sucesso! Agora você pode {ação principal do perfil}. Acesse: {link}"`
  - Onde `{ação principal do perfil}`:
    - Gerente: `"cadastrar suas vans, motoristas e publicar viagens"`
    - Passageiro: `"buscar viagens e reservar assentos para eventos"`
    - Motorista: `"acessar seu painel e visualizar as viagens alocadas a você"`
  - Onde `{link}`: `"http://localhost:3000/entrar"` (configurável)
- [ ] Se o envio do email falhar → loga o erro mas **NÃO reverte o cadastro** (o usuário já foi criado com sucesso)
- [ ] Usa `isHtml = false` (texto puro, simples e direto)

### FR-004 — Caso de Uso 2: Redefinição de senha

**Descrição:** Implementar o fluxo completo de "esqueci minha senha" — o usuário informa o email, recebe um código de redefinição por email, e define uma nova senha.

**Critérios de Aceite:**

**Parte A — Solicitar redefinição (novo endpoint):**
- [ ] `POST /api/auth/esqueci-senha` — body: `{ email: string }`
- [ ] Buscar usuário pelo email no banco
- [ ] Se email não encontrado → retornar 200 com mensagem genérica "Se o email estiver cadastrado, um código de redefinição foi enviado." (não revela se o email existe)
- [ ] Se email encontrado:
  - Gerar código de 6 dígitos aleatório
  - Salvar código + expiração (15 minutos) no banco (colunas `CodigoResetSenha` e `ExpiracaoCodigoResetSenha` na tabela `Usuarios`)
  - Enviar email com o código
- [ ] Conteúdo do email de redefinição:
  - Assunto: `"Código de redefinição de senha - VanBora"`
  - Corpo: `"Seu código de redefinição de senha é: {codigo}. Ele expira em 15 minutos. Se você não solicitou a redefinição, ignore este email."`
  - `isHtml = false`

**Parte B — Confirmar redefinição (novo endpoint):**
- [ ] `POST /api/auth/redefinir-senha` — body: `{ email: string, codigo: string, novaSenha: string }`
- [ ] Buscar usuário pelo email
- [ ] Validar código: existe, não expirou, confere com o salvo
- [ ] Se inválido/expirado → 400 "Código inválido ou expirado."
- [ ] Se válido → hash da nova senha, limpar `CodigoResetSenha` e `ExpiracaoCodigoResetSenha`, salvar
- [ ] Retornar 200 "Senha redefinida com sucesso."

**Parte C — Frontend (já existe parcialmente):**
- [ ] Tela `MotoristaRecuperarSenhaCard.tsx` já existe → garantir que consome os novos endpoints
- [ ] Criar tela equivalente para Gerente e Passageiro (componente reutilizável `EsqueciSenhaCard.tsx`)

### FR-005 — Caso de Uso 3: Confirmação de reserva

**Descrição:** Após criar uma reserva com sucesso, enviar email de confirmação ao comprador.

**Critérios de Aceite:**
- [ ] `ReservaService.CriarReservaAsync` → após criar reserva com sucesso (antes do `return`) → envia email de confirmação
- [ ] Conteúdo do email de confirmação:
  - Assunto: `"Reserva confirmada - VanBora"`
  - Corpo (texto puro):
    ```
    Olá {Nome},
    
    Sua reserva foi confirmada!
    
    Evento: {nomeEvento}
    Data: {dataEvento}
    Saída: {origem} → {destino}
    Data/Hora de partida: {dataPartida}
    Assentos reservados: {quantidade}
    Valor total: R$ {valorTotal}
    
    Código da reserva: {id}
    
    Obrigado por usar o VanBora!
    ```
  - `isHtml = false`
- [ ] O email é enviado após a reserva ser persistida no banco (reserva já existe)
- [ ] Se o envio falhar → loga o erro mas **NÃO reverte a reserva**

### FR-006 — Caso de Uso 4: Confirmação de reembolso

**Descrição:** Após cancelar uma reserva com reembolso, enviar email de confirmação ao comprador.

**Critérios de Aceite:**
- [ ] `ReservaService.CancelarReservaAsync` → após cancelar com sucesso → envia email de confirmação do reembolso
- [ ] Conteúdo do email de reembolso:
  - Assunto: `"Reembolso confirmado - VanBora"`
  - Corpo (texto puro):
    ```
    Olá {Nome},
    
    Sua reserva foi cancelada e o reembolso foi processado.
    
    Evento: {nomeEvento}
    Valor reembolsado: R$ {valorTotal}
    Código da reserva: {id}
    
    O reembolso será creditado na mesma forma de pagamento utilizada na compra.
    
    Qualquer dúvida, entre em contato com o vendedor pelo WhatsApp informado na viagem.
    ```
  - `isHtml = false`
- [ ] Se o envio falhar → loga o erro mas **NÃO reverte o cancelamento**

---

## Non-Functional Requirements

### NFR-001 — Segurança
- [ ] Senha do SMTP **nunca** aparece em logs
- [ ] `EmailSettings` carregado de fonte segura (appsettings.Development.json não commitado → secrets → env vars)
- [ ] Código de redefinição de senha: 6 dígitos, expira em 15 min, one-time use, limpo após uso
- [ ] Endpoint `esqueci-senha` não revela se o email existe ou não (timing attack mitigation: resposta sempre ~igual)

### NFR-002 — Disponibilidade
- [ ] Se SMTP indisponível, sistema continua funcionando — apenas o email falha (log + `Result.Failure`)
- [ ] Nenhum fluxo de negócio (cadastro, reserva, reembolso) depende do sucesso do email

### NFR-003 — Manutenibilidade
- [ ] Troca de provedor SMTP = alterar `appsettings.json` (zero código)
- [ ] Fallback mock ativado automaticamente se config ausente

---

## Edge Cases

| ID | Cenário | Comportamento esperado |
|----|---------|------------------------|
| EC-01 | `EmailSettings` não configurado | Fallback mock com warning no log |
| EC-02 | SMTP indisponível durante cadastro | Usuário é criado normalmente; email de boas-vindas falha silenciosamente (log) |
| EC-03 | SMTP indisponível durante reserva | Reserva é criada normalmente; email de confirmação falha (log) |
| EC-04 | SMTP indisponível durante reembolso | Reembolso processado; email de confirmação falha (log) |
| EC-05 | Email inexistente no "esqueci senha" | Retorna 200 com mensagem genérica (não revela inexistência) |
| EC-06 | Código de reset expirado (> 15 min) | 400 "Código inválido ou expirado." |
| EC-07 | Código de reset já usado | 400 "Código inválido ou expirado." (código limpo após uso) |
| EC-08 | Múltiplas solicitações de reset para mesmo email | Cada nova solicitação sobrescreve o código anterior (só o último código é válido) |
| EC-09 | Usuário tenta redefinir senha sem ter solicitado código | 400 "Código inválido ou expirado." |
| EC-10 | Email com `To` inválido | `Result.Failure`; log de erro |
| EC-11 | Motorista recém-registrado não tem email | Não envia email de boas-vindas (motorista pode ser cadastrado sem email próprio — usa o email do gerente?) |

---

## Consumidores do `IEmailService`

| Consumidor | Método | Caso de Uso |
|-----------|--------|-------------|
| `AuthService.RegistrarGerente` | `SendAsync` | FR-003 — Boas-vindas Gerente |
| `AuthService.RegistrarPassageiroAsync` | `SendAsync` | FR-003 — Boas-vindas Passageiro |
| `MotoristaService.RegistrarMotorista` | `SendAsync` | FR-003 — Boas-vindas Motorista |
| `AuthService.EsqueciSenhaAsync` (NOVO) | `SendAsync` | FR-004 — Redefinição de senha |
| `ReservaService.CriarReservaAsync` | `SendAsync` | FR-005 — Confirmação de reserva |
| `ReservaService.CancelarReservaAsync` | `SendAsync` | FR-006 — Confirmação de reembolso |
| `AuthService.ConfirmarExclusaoAsync` (existente) | `SendAsync` | Código de exclusão de conta |

---

## Endpoints novos (FR-004)

| Método | Rota | Body | Response | Autenticação |
|--------|------|------|----------|--------------|
| POST | `/api/auth/esqueci-senha` | `{ email }` | 200 `{ message }` | Não (pública) |
| POST | `/api/auth/redefinir-senha` | `{ email, codigo, novaSenha }` | 200 `{ message }` | Não (pública) |

---

## Mudanças no banco de dados (FR-004)

Adicionar colunas na tabela `Usuarios`:

```sql
ALTER TABLE "Usuarios" ADD COLUMN "CodigoResetSenha" VARCHAR(6) NULL;
ALTER TABLE "Usuarios" ADD COLUMN "ExpiracaoCodigoResetSenha" TIMESTAMPTZ NULL;
```

---

## Dependências entre Specs

| Spec | Relação | Impacto |
|------|---------|---------|
| Spec 20 — Viagens | **auditada** | `ReservaService` já existe e consome viagens para criar reservas |
| Todas as demais | Nenhuma bloqueia nem é bloqueada | Spec 70 é infraestrutura isolada |

---

## Fora do escopo

- Templates de email HTML bonitos (ex: Razor, Liquid) — usa texto puro por enquanto
- Fila de emails (outbox pattern, retry com backoff)
- Tracking de abertura/clique
- Envio de emails em lote (bulk)
- Webhooks de bounce/complaint
- Serviços terceiros (SendGrid, Mailgun, Amazon SES) — SMTP genérico cobre todos
- Login social / OAuth
- 2FA por email
