---
name: Serviço de Email
status: auditada
---

# Tasks — Spec 70: Serviço de Email

## Pré-requisitos

- [ ] **P0** — Verificar se `appsettings.Development.json` está no `.gitignore`. Se não estiver, ajustar antes de adicionar credenciais SMTP.

---

## Checklist

### T1 — Infraestrutura: `EmailSettings` + `IEmailService` + `EmailService`

- [ ] **T1.1** — Criar diretório `VanBora.Infrastructure/Configuration/`
- [ ] **T1.2** — Criar `EmailSettings.cs` com: `Host`, `Port`, `UseSsl`, `Username`, `Password`, `FromAddress`, `FromName`, `IsConfigured`
- [ ] **T1.3** — Atualizar `IEmailService.cs`: adicionar `bool isHtml = false`
- [ ] **T1.4** — Reescrever `EmailService.cs`: construtor recebe `IOptions<EmailSettings>`, implementa SMTP real com fallback mock, timeout 10s, tratamento de exceções
- [ ] **T1.5** — Atualizar `ServiceCollectionExtensions.cs`: `services.Configure<EmailSettings>(...)` antes de `AddScoped<IEmailService, EmailService>`
- [ ] **T1.6** — Adicionar seção `EmailSettings` ao `appsettings.Development.json` (valores placeholder)
- [ ] **Validação:** `dotnet build` na solution — zero erros. Sem `EmailSettings` configurado, API inicia e usa fallback mock.

### T2 — Caso de Uso 1: Email de boas-vindas

- [ ] **T2.1** — `AuthService.RegistrarGerente`: após criar usuário com sucesso → `_emailService.SendAsync` com mensagem de boas-vindas (Gerente)
- [ ] **T2.2** — `AuthService.RegistrarPassageiroAsync`: após criar usuário → `_emailService.SendAsync` com mensagem de boas-vindas (Passageiro)
- [ ] **T2.3** — `MotoristaService.RegistrarMotorista`: após criar motorista → `_emailService.SendAsync` com mensagem de boas-vindas (Motorista), **apenas se** `usuario.Email is not null`
- [ ] **T2.4** — Em todos os 3 casos: falha de email → `_logger.LogWarning`, **NÃO reverte** o cadastro
- [ ] **Validação:** Cadastrar um gerente, passageiro e motorista → verificar se emails de boas-vindas são logados (mock) ou enviados (SMTP real)

### T3 — Caso de Uso 2: Redefinição de senha (Backend)

- [ ] **T3.1** — Adicionar colunas `CodigoResetSenha` (VARCHAR 6 NULL) e `ExpiracaoCodigoResetSenha` (TIMESTAMPTZ NULL) em `Usuario.cs` + métodos `DefinirCodigoResetSenha` e `LimparCodigoResetSenha`
- [ ] **T3.2** — Criar migration: `dotnet ef migrations add AddCodigoResetSenha` → aplicar `dotnet ef database update`
- [ ] **T3.3** — Criar DTOs: `EsqueciSenhaRequest`, `EsqueciSenhaResponse`, `RedefinirSenhaRequest`, `RedefinirSenhaResponse`
- [ ] **T3.4** — `IAuthService`: adicionar `Task<Result<EsqueciSenhaResponse>> EsqueciSenhaAsync(...)` e `Task<Result<RedefinirSenhaResponse>> RedefinirSenhaAsync(...)`
- [ ] **T3.5** — `AuthService.EsqueciSenhaAsync`: buscar usuário por email → se não encontrado, retornar sucesso genérico → gerar código 6 dígitos + expiração 15 min → salvar → enviar email com código
- [ ] **T3.6** — `AuthService.RedefinirSenhaAsync`: buscar usuário → validar código (existe, não expirou, confere) → hash nova senha → limpar código → salvar
- [ ] **T3.7** — `AuthController`: adicionar `[AllowAnonymous] POST esqueci-senha` e `[AllowAnonymous] POST redefinir-senha`
- [ ] **T3.8** — Verificar se `IUsuarioRepository` tem `GetByEmailAsync` — se não tiver, adicionar
- [ ] **Validação:** `POST /api/auth/esqueci-senha` com email existente → código no log (mock) → `POST /api/auth/redefinir-senha` com código correto → senha alterada

### T4 — Caso de Uso 2: Redefinição de senha (Frontend)

- [ ] **T4.1** — Criar `components/EsqueciSenhaCard.tsx` reutilizável (mesmo padrão visual do `MotoristaRecuperarSenhaCard`)
  - Props: nenhuma (usa `useSearchParams` para detectar origem se necessário)
  - Passo 1: input email + botão "Enviar código" → `POST /api/auth/esqueci-senha`
  - Passo 2: input código (6 dígitos) + input nova senha + input confirmar senha + botão "Redefinir senha" → `POST /api/auth/redefinir-senha`
  - Sucesso: mensagem verde + link "Ir para o login"
  - Estados: loading, erro, sucesso
- [ ] **T4.2** — Atualizar `MotoristaRecuperarSenhaCard.tsx` para usar os novos endpoints (ou substituir pelo `EsqueciSenhaCard` reutilizável)
- [ ] **T4.3** — Garantir que a tela de login (`LoginPageClient.tsx`) tem link "Esqueci minha senha" que leva ao `EsqueciSenhaCard`
- [ ] **Validação:** Fluxo completo no navegador: tela de login → "Esqueci minha senha" → digitar email → receber código (mock: aparece no log) → digitar código + nova senha → login com nova senha funciona

### T5 — Caso de Uso 3: Confirmação de reserva

- [ ] **T5.1** — `ReservaService` precisa receber `IEmailService` via DI (adicionar ao construtor)
- [ ] **T5.2** — `ReservaService.CriarReservaAsync`: após reserva criada com sucesso → `_emailService.SendAsync` com dados da reserva
- [ ] **T5.3** — Falha de email → `_logger.LogWarning`, **NÃO reverte** a reserva
- [ ] **Validação:** Criar reserva via frontend → verificar email de confirmação no log (mock)

### T6 — Caso de Uso 4: Confirmação de reembolso

- [ ] **T6.1** — `ReservaService.CancelarReservaAsync`: após cancelamento bem-sucedido → `_emailService.SendAsync` com dados do reembolso
- [ ] **T6.2** — Buscar `Usuario` pelo `reserva.UsuarioId` para obter email (se não disponível, log warning e skip)
- [ ] **T6.3** — Falha de email → `_logger.LogWarning`, **NÃO reverte** o cancelamento
- [ ] **Validação:** Cancelar reserva → verificar email de reembolso no log (mock)

### T7 — Build e verificação final

- [ ] **T7.1** — `dotnet build` na solution — zero erros
- [ ] **T7.2** — Verificar que `AuthService` existente (`ConfirmarExclusaoAsync`) continua compilando sem alterações
- [ ] **T7.3** — Verificar que sem `EmailSettings`, sistema inicia normalmente (fallback mock)
- [ ] **T7.4** — `npx tsc --noEmit` no frontend — zero erros (para os componentes modificados)

---

## Mapeamento Tasks → Requisitos

| Task | FR coberto |
|------|-----------|
| T1.1-T1.6 | FR-001 (SMTP infra), FR-002 (Config) |
| T2.1-T2.4 | FR-003 (Boas-vindas) |
| T3.1-T3.8 | FR-004 Partes A+B (Redefinição backend) |
| T4.1-T4.3 | FR-004 Parte C (Redefinição frontend) |
| T5.1-T5.3 | FR-005 (Confirmação reserva) |
| T6.1-T6.3 | FR-006 (Confirmação reembolso) |
| T7.1-T7.4 | NFR-001, NFR-002, NFR-003 |

---

## Testes Manuais de Verificação

- [ ] **TM-01** — Iniciar API sem `EmailSettings` → sistema inicia, usa fallback mock
- [ ] **TM-02** — Cadastrar novo Gerente → email de boas-vindas aparece no log (mock)
- [ ] **TM-03** — Cadastrar novo Passageiro → email de boas-vindas aparece no log (mock)
- [ ] **TM-04** — Cadastrar novo Motorista (com email) → email de boas-vindas aparece no log (mock)
- [ ] **TM-05** — Solicitar "esqueci senha" com email existente → código aparece no log → redefinir com código → login com nova senha funciona
- [ ] **TM-06** — Solicitar "esqueci senha" com email inexistente → mensagem genérica de sucesso (não revela inexistência)
- [ ] **TM-07** — Usar código expirado/incorreto na redefinição → erro "Código inválido ou expirado."
- [ ] **TM-08** — Criar reserva → email de confirmação aparece no log (mock)
- [ ] **TM-09** — Cancelar reserva → email de reembolso aparece no log (mock)
- [ ] **TM-10** — Configurar SMTP real (ex: Mailtrap) → emails chegam na caixa de entrada
- [ ] **TM-11** — Configurar SMTP com credenciais inválidas → `Result.Failure` logado, operação de negócio NÃO é revertida
- [ ] **TM-12** — Passar `isHtml = true` com corpo HTML → email chega renderizado como HTML
