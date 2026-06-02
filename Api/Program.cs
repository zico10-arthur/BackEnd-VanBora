using System.Text;
using Api.Middleware;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using VanBora.Application.DTOs.Auth;
using Api.Services;
using VanBora.Application.Interfaces;
using VanBora.Application.Services;
using VanBora.Application.Settings;
using VanBora.Application.Validators;
using VanBora.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Overrides locais (gitignored): appsettings.Development.local.json — webhook/ngrok, etc.
builder.Configuration.AddJsonFile(
    $"appsettings.{builder.Environment.EnvironmentName}.local.json",
    optional: true,
    reloadOnChange: true);

// ── JWT Settings ────────────────────────────────────────────────
var jwtSection = builder.Configuration.GetSection(JwtSettings.SectionName);
builder.Services.Configure<JwtSettings>(jwtSection);

var jwtSettings = jwtSection.Get<JwtSettings>()
    ?? throw new InvalidOperationException("JwtSettings não configurados no appsettings.json");

if (jwtSettings.SecretKey.Length < 32)
    throw new InvalidOperationException("A SecretKey do JWT deve ter pelo menos 32 caracteres.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
        };
    });

builder.Services.AddAuthorization();

var corsSection = builder.Configuration.GetSection(CorsSettings.SectionName);
builder.Services.Configure<CorsSettings>(corsSection);
var corsOrigins = corsSection.Get<CorsSettings>()?.AllowedOrigins
    ?? ["http://localhost:3000"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(corsOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ── Application Services ────────────────────────────────────────
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IReservaService, ReservaService>();
builder.Services.AddScoped<IViagemPublicService, ViagemPublicService>();

builder.Services.Configure<MercadoPagoSettings>(builder.Configuration.GetSection(MercadoPagoSettings.SectionName));

// ── Validators ──────────────────────────────────────────────────
builder.Services.AddScoped<IValidator<RegistrarGerenteRequest>, RegistrarGerenteValidator>();
builder.Services.AddScoped<IValidator<LoginRequest>, LoginValidator>();
builder.Services.AddScoped<IValidator<RegistrarPassageiroRequest>, RegistrarPassageiroRequestValidator>();
builder.Services.AddScoped<IValidator<VanBora.Application.DTOs.Reservas.CriarReservaRequest>, CriarReservaValidator>();

// ── Infrastructure ──────────────────────────────────────────────
builder.Services.AddInfrastructure(builder.Configuration);

// ── Controllers + Swagger (Swashbuckle) ─────────────────────────
// Evitar Microsoft.AspNetCore.OpenApi no mesmo projeto: conflito de Microsoft.OpenApi em runtime com Swashbuckle.
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ResultFilter>();
});
builder.Services.AddSwaggerGen();

builder.Services.AddHostedService<ExpirarReservasBackgroundService>();
builder.Services.AddHostedService<DevDataSeeder>();
builder.Services.AddScoped<Api.Services.MercadoPagoWebhookHandler>();

var app = builder.Build();

// ── Middleware Pipeline ─────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();

// Habilitar CORS antes da autenticação/autorização
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
