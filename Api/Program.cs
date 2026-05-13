using System.Text;
using Api.Middleware;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using VanBora.Application.DTOs.Auth;
using VanBora.Application.Interfaces;
using VanBora.Application.Services;
using VanBora.Application.Settings;
using VanBora.Application.Validators;
using VanBora.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

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

// ── Application Services ────────────────────────────────────────
builder.Services.AddScoped<IAuthService, AuthService>();

// ── Validators ──────────────────────────────────────────────────
builder.Services.AddScoped<IValidator<RegistrarGerenteRequest>, RegistrarGerenteValidator>();
builder.Services.AddScoped<IValidator<LoginRequest>, LoginValidator>();

// ── Infrastructure ──────────────────────────────────────────────
builder.Services.AddInfrastructure(builder.Configuration);

// ── Controllers + OpenAPI ───────────────────────────────────────
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ResultFilter>();
});
builder.Services.AddOpenApi();

var app = builder.Build();

// ── Middleware Pipeline ─────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
