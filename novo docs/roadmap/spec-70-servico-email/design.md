---
name: Serviço de Email
status: auditada
---

# Design — Spec 70: Serviço de Email

## 1. Arquitetura de Arquivos

```
Arquivos a CRIAR:
├── VanBora.Infrastructure/
│   └── Configuration/
│       └── EmailSettings.cs                    ← Classe de configuração tipada
├── VanBora.Application/
│   └── DTOs/Auth/
│       ├── EsqueciSenhaRequest.cs              ← { email: string }
│       ├── EsqueciSenhaResponse.cs             ← { message: string }
│       ├── RedefinirSenhaRequest.cs            ← { email, codigo, novaSenha }
│       └── RedefinirSenhaResponse.cs           ← { message: string }
└── frontend/
    └── components/
        └── EsqueciSenhaCard.tsx                 ← Reutilizável (Gerente, Passageiro, Motorista)

Arquivos a MODIFICAR:
├── VanBora.Application/
│   ├── Interfaces/
│   │   └── IEmailService.cs                    ← + isHtml: bool = false
│   │   └── IAuthService.cs                     ← + EsqueciSenhaAsync, RedefinirSenhaAsync
│   └── Services/
│       ├── AuthService.cs                       ← + EsqueciSenhaAsync + RedefinirSenhaAsync + boas-vindas
│       ├── MotoristaService.cs                  ← + email boas-vindas após RegistrarMotorista
│       └── ReservaService.cs                    ← + email confirmação reserva + reembolso
├── VanBora.Infrastructure/
│   ├── Services/
│   │   └── EmailService.cs                      ← Mock → SMTP real com fallback
│   └── Extensions/
│       └── ServiceCollectionExtensions.cs       ← + services.Configure<EmailSettings>
├── VanBora.Domain/
│   └── Entities/
│       └── Usuario.cs                           ← + CodigoResetSenha, ExpiracaoCodigoResetSenha
├── Api/
│   ├── Controllers/
│   │   └── AuthController.cs                    ← + POST esqueci-senha, POST redefinir-senha
│   └── appsettings.Development.json            ← + seção EmailSettings
└── frontend/
    └── components/
        └── motorista/
            └── MotoristaRecuperarSenhaCard.tsx  ← Adaptar para novos endpoints
```

---

## 2. Stack

| Dependência | Versão | Uso |
|------------|--------|-----|
| `System.Net.Mail` (built-in .NET) | 9.0 | `SmtpClient` para envio SMTP |
| `Microsoft.Extensions.Options` | 9.0 | `IOptions<EmailSettings>` |
| `Microsoft.Extensions.Logging` | 9.0 | Logging |
| BCrypt (já usado no projeto) | - | Hash da nova senha na redefinição |

---

## 3. Configuração — `EmailSettings`

```csharp
namespace VanBora.Infrastructure.Configuration;

public class EmailSettings
{
    public const string SectionName = "EmailSettings";

    public string Host { get; init; } = string.Empty;
    public int Port { get; init; } = 587;
    public bool UseSsl { get; init; } = false;
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string FromAddress { get; init; } = string.Empty;
    public string FromName { get; init; } = "VanBora";

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Host) &&
        !string.IsNullOrWhiteSpace(Username) &&
        !string.IsNullOrWhiteSpace(Password) &&
        !string.IsNullOrWhiteSpace(FromAddress);
}
```

### appsettings.Development.json

```json
{
  "EmailSettings": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "UseSsl": false,
    "Username": "seu-email@gmail.com",
    "Password": "sua-senha-de-app",
    "FromAddress": "seu-email@gmail.com",
    "FromName": "VanBora"
  }
}
```

---

## 4. Interface `IEmailService`

```csharp
public interface IEmailService
{
    Task<Result<bool>> SendAsync(
        string to,
        string subject,
        string body,
        bool isHtml = false,
        CancellationToken cancellationToken = default);
}
```

**Retrocompatibilidade:** `isHtml` tem default `false`. Chamadas existentes compilam sem alteração.

---

## 5. `EmailService` — Implementação SMTP

### 5.1 Comportamento

```
EmailService.SendAsync(to, subject, body, isHtml, ct)
  │
  ├── !EmailSettings.IsConfigured?
  │     └── log warning "[EmailService Mock] ..." → return Success(true)
  │
  ├── !IsValidEmail(to)?
  │     └── return Failure("Endereço de email inválido.")
  │
  └── try
        ├── new MailMessage { From, To, Subject, Body, IsBodyHtml = isHtml }
        ├── new SmtpClient { Host, Port, EnableSsl=true, Credentials, Timeout=10s }
        ├── await client.SendMailAsync(message, ct)
        ├── log info "Email enviado para {To}"
        └── return Success(true)
      catch (SmtpException ex) when ex.StatusCode == ClientNotPermitted
        → return Failure("Credenciais de email inválidas.")
      catch (SmtpException)
        → return Failure("Falha ao conectar ao servidor de email.")
      catch (TaskCanceledException) when ct.IsCancellationRequested
        → return Failure("Operação cancelada.")
      catch (TaskCanceledException)
        → return Failure("Tempo limite excedido ao enviar email.")
      catch (Exception)
        → return Failure("Erro ao enviar email.")
```

### 5.2 Validação de email

```csharp
private static bool IsValidEmail(string email)
{
    try { var addr = new MailAddress(email); return addr.Address == email; }
    catch { return false; }
}
```

---

## 6. Fluxo dos 4 Casos de Uso

### 6.1 Caso 1 — Email de boas-vindas

**Ponto de inserção em cada serviço:**

```csharp
// AuthService.RegistrarGerente (após criar usuário com sucesso)
var usuarioCriado = ...;
await _emailService.SendAsync(
    usuarioCriado.Email.Valor,
    "Bem-vindo(a) ao VanBora!",
    $"Olá {usuarioCriado.Nome}, sua conta no VanBora foi criada com sucesso! " +
    "Agora você pode cadastrar suas vans, motoristas e publicar viagens. " +
    "Acesse: http://localhost:3000/entrar");
// Falha de email é logada, NÃO reverte o cadastro

// AuthService.RegistrarPassageiroAsync (após criar usuário com sucesso)
await _emailService.SendAsync(
    usuarioCriado.Email.Valor,
    "Bem-vindo(a) ao VanBora!",
    $"Olá {usuarioCriado.Nome}, sua conta no VanBora foi criada com sucesso! " +
    "Agora você pode buscar viagens e reservar assentos para eventos. " +
    "Acesse: http://localhost:3000/entrar");

// MotoristaService.RegistrarMotorista (após criar com sucesso, se tiver email)
if (usuarioExistente.Email is not null)
{
    await _emailService.SendAsync(
        usuarioExistente.Email.Valor,
        "Bem-vindo(a) ao VanBora!",
        $"Olá {usuarioExistente.Nome}, sua conta de motorista no VanBora foi criada com sucesso! " +
        "Agora você pode acessar seu painel e visualizar as viagens alocadas a você. " +
        "Acesse: http://localhost:3000/entrar");
}
```

### 6.2 Caso 2 — Redefinição de senha

**2 novos métodos no `AuthService`:**

```csharp
public async Task<Result<EsqueciSenhaResponse>> EsqueciSenhaAsync(
    EsqueciSenhaRequest request, CancellationToken ct)
{
    // 1. Buscar usuário por email
    var usuario = await _usuarioRepo.GetByEmailAsync(request.Email, ct);
    
    // 2. Se não encontrado → retorna sucesso com mensagem genérica (não revela inexistência)
    if (usuario is null)
        return Result<EsqueciSenhaResponse>.Success(
            new EsqueciSenhaResponse("Se o email estiver cadastrado, um código de redefinição foi enviado."));
    
    // 3. Gerar código de 6 dígitos + expiração (15 min)
    var codigo = Random.Shared.Next(100000, 999999).ToString();
    usuario.DefinirCodigoResetSenha(codigo, DateTime.UtcNow.AddMinutes(15));
    
    // 4. Salvar no banco
    await _usuarioRepo.UpdateAsync(usuario, ct);
    await _unitOfWork.SaveChangesAsync(ct);
    
    // 5. Enviar email
    var emailResult = await _emailService.SendAsync(
        usuario.Email.Valor,
        "Código de redefinição de senha - VanBora",
        $"Seu código de redefinição de senha é: {codigo}. " +
        "Ele expira em 15 minutos. Se você não solicitou a redefinição, ignore este email.",
        cancellationToken: ct);
    
    if (emailResult.IsFailure)
        _logger.LogWarning("Falha ao enviar email de redefinição: {Error}", emailResult.Error.Message);
    
    return Result<EsqueciSenhaResponse>.Success(
        new EsqueciSenhaResponse("Se o email estiver cadastrado, um código de redefinição foi enviado."));
}

public async Task<Result<RedefinirSenhaResponse>> RedefinirSenhaAsync(
    RedefinirSenhaRequest request, CancellationToken ct)
{
    // 1. Buscar usuário por email
    var usuario = await _usuarioRepo.GetByEmailAsync(request.Email, ct);
    if (usuario is null)
        return Result<RedefinirSenhaResponse>.Failure("Código inválido ou expirado.");
    
    // 2. Validar código
    if (string.IsNullOrWhiteSpace(usuario.CodigoResetSenha) ||
        usuario.CodigoResetSenha != request.Codigo ||
        usuario.ExpiracaoCodigoResetSenha < DateTime.UtcNow)
        return Result<RedefinirSenhaResponse>.Failure("Código inválido ou expirado.");
    
    // 3. Hash e atualizar senha
    var senhaHash = BCrypt.Net.BCrypt.HashPassword(request.NovaSenha);
    usuario.AlterarSenha(senhaHash);
    
    // 4. Limpar código de reset
    usuario.LimparCodigoResetSenha();
    
    // 5. Salvar
    await _usuarioRepo.UpdateAsync(usuario, ct);
    await _unitOfWork.SaveChangesAsync(ct);
    
    return Result<RedefinirSenhaResponse>.Success(
        new RedefinirSenhaResponse("Senha redefinida com sucesso."));
}
```

**2 novos endpoints no `AuthController`:**

```csharp
[HttpPost("esqueci-senha")]
[AllowAnonymous]
public async Task<IActionResult> EsqueciSenha(
    [FromBody] EsqueciSenhaRequest request,
    CancellationToken ct)
{
    var result = await _authService.EsqueciSenhaAsync(request, ct);
    return Ok(result.Value);
}

[HttpPost("redefinir-senha")]
[AllowAnonymous]
public async Task<IActionResult> RedefinirSenha(
    [FromBody] RedefinirSenhaRequest request,
    CancellationToken ct)
{
    var result = await _authService.RedefinirSenhaAsync(request, ct);
    if (result.IsFailure)
        return BadRequest(new { error = new { code = "CODIGO_INVALIDO", message = result.Error.Message } });
    return Ok(result.Value);
}
```

**Novas colunas na entidade `Usuario`:**

```csharp
public string? CodigoResetSenha { get; private set; }
public DateTime? ExpiracaoCodigoResetSenha { get; private set; }

public void DefinirCodigoResetSenha(string codigo, DateTime expiracao)
{
    CodigoResetSenha = codigo;
    ExpiracaoCodigoResetSenha = expiracao;
}

public void LimparCodigoResetSenha()
{
    CodigoResetSenha = null;
    ExpiracaoCodigoResetSenha = null;
}
```

### 6.3 Caso 3 — Confirmação de reserva

**Ponto de inserção em `ReservaService.CriarReservaAsync`:**

```csharp
// No final do método, logo antes do return Ok(resultado)
var emailResult = await _emailService.SendAsync(
    usuario.Email.Valor,
    "Reserva confirmada - VanBora",
    $"Olá {usuario.Nome},\n\n" +
    $"Sua reserva foi confirmada!\n\n" +
    $"Evento: {viagem.NomeEvento}\n" +
    $"Data: {viagem.DataEvento:dd/MM/yyyy}\n" +
    $"Saída: {viagem.OrigemDescricao}, {viagem.OrigemCidade} - {viagem.OrigemEstado}\n" +
    $"Destino: {viagem.DestinoDescricao}, {viagem.DestinoCidade} - {viagem.DestinoEstado}\n" +
    $"Data/Hora de partida: {viagem.DataSaida:dd/MM/yyyy HH:mm}\n" +
    $"Assentos reservados: {reserva.Itens.Count}\n" +
    $"Valor total: R$ {reserva.ValorTotal:0.00}\n\n" +
    $"Código da reserva: {reserva.Id}\n\n" +
    $"Obrigado por usar o VanBora!",
    cancellationToken: ct);

if (emailResult.IsFailure)
    _logger.LogWarning("Falha ao enviar email de confirmação de reserva: {Error}", emailResult.Error.Message);
```

### 6.4 Caso 4 — Confirmação de reembolso

**Ponto de inserção em `ReservaService.CancelarReservaAsync`:**

```csharp
// Após reserva.Cancelar() e SaveChangesAsync bem-sucedidos
var usuario = await _usuarioRepo.GetByIdAsync(reserva.UsuarioId, ct);
if (usuario?.Email is not null)
{
    var emailResult = await _emailService.SendAsync(
        usuario.Email.Valor,
        "Reembolso confirmado - VanBora",
        $"Olá {usuario.Nome},\n\n" +
        $"Sua reserva foi cancelada e o reembolso foi processado.\n\n" +
        $"Evento: {reserva.Viagem.NomeEvento}\n" +
        $"Valor reembolsado: R$ {reserva.ValorTotal:0.00}\n" +
        $"Código da reserva: {reserva.Id}\n\n" +
        $"O reembolso será creditado na mesma forma de pagamento utilizada na compra.\n\n" +
        $"Qualquer dúvida, entre em contato com o vendedor pelo WhatsApp informado na viagem.",
        cancellationToken: ct);

    if (emailResult.IsFailure)
        _logger.LogWarning("Falha ao enviar email de reembolso: {Error}", emailResult.Error.Message);
}
```

---

## 7. Registro no DI — `ServiceCollectionExtensions`

```csharp
// Antes da linha services.AddScoped<IEmailService, EmailService>();
services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));
services.AddScoped<IEmailService, EmailService>();
```

---

## 8. DTOs

### EsqueciSenhaRequest / EsqueciSenhaResponse
```csharp
public record EsqueciSenhaRequest(string Email);
public record EsqueciSenhaResponse(string Message);
```

### RedefinirSenhaRequest / RedefinirSenhaResponse
```csharp
public record RedefinirSenhaRequest(string Email, string Codigo, string NovaSenha);
public record RedefinirSenhaResponse(string Message);
```

---

## 9. Frontend — EsqueciSenhaCard (reutilizável)

```
EsqueciSenhaCard.tsx
  ├── Props: role: "gerente" | "passageiro" | "motorista"
  ├── Passo 1: "Digite seu email"
  │     ├── input email + botão "Enviar código"
  │     ├── POST /api/auth/esqueci-senha { email }
  │     └── Sucesso → avança para passo 2
  ├── Passo 2: "Digite o código enviado por email"
  │     ├── input código (6 dígitos) + input nova senha + input confirmar senha
  │     ├── POST /api/auth/redefinir-senha { email, codigo, novaSenha }
  │     └── Sucesso → mensagem "Senha redefinida!" + link para /entrar
  └── Estilo: mesmo padrão de MotoristaRecuperarSenhaCard (dark theme zinc)
```

---

## 10. Decisões de Design

1. **SmtpClient built-in vs MailKit:** SmtpClient é suficiente para SMTP simples. Sem dependência externa.

2. **Fallback mock automático:** Se `EmailSettings` não configurado → log warning + retorna sucesso. Desenvolvimento local não quebra.

3. **Email NÃO é transacional:** Se o envio falhar, o cadastro/reserva/reembolso **já foi concluído**. O sistema loga o erro e segue. Isso evita reverter operações de negócio por falha de infraestrutura.

4. **Código de reset 6 dígitos, 15 minutos:** Simples, sem JWT. One-time use, limpo após redefinição. Segurança adequada para MVP.

5. **Endpoint esqueci-senha é anônimo e não revela existência:** Retorna sempre a mesma mensagem, exista o email ou não. Timing attack mitigado pela operação de email (que é assíncrona e leva tempo similar).

6. **Texto puro (`isHtml = false`) para todos os 4 casos:** Simples, funciona em qualquer cliente de email, sem preocupação com renderização. HTML pode vir depois como melhoria.

7. **Motorista pode não ter email próprio:** Motoristas cadastrados pelo gerente podem ter apenas nome + CPF + CNH. O envio de boas-vindas só ocorre se `usuario.Email is not null`.

8. **Nova migration necessária:** `CodigoResetSenha` e `ExpiracaoCodigoResetSenha` são colunas novas — requer `dotnet ef migrations add` e `dotnet ef database update`.
