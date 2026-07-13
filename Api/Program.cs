using System.Text;
using Api.Middleware;
using Api.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using VanBora.Application.DTOs.Admin;
using VanBora.Application.DTOs.Auth;
using VanBora.Application.DTOs.Reservas;
using VanBora.Application.DTOs.Vans;
using VanBora.Application.DTOs.Viagens;
using VanBora.Application.Interfaces;
using VanBora.Application.Mappings;
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

// ── Mercado Pago Settings ──────────────────────────────────────
builder.Services.Configure<MercadoPagoSettings>(
    builder.Configuration.GetSection(MercadoPagoSettings.SectionName));

// ── AutoMapper ──────────────────────────────────────────────────
builder.Services.AddAutoMapper(typeof(VanProfile).Assembly);
builder.Services.AddAutoMapper(typeof(MotoristaProfile).Assembly);
builder.Services.AddAutoMapper(typeof(ViagemProfile).Assembly);
builder.Services.AddAutoMapper(typeof(ReservaProfile).Assembly);




// ── Application Services ────────────────────────────────────────
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IVanService, VanService>();
builder.Services.AddScoped<IViagemService, ViagemService>();
builder.Services.AddScoped<IReservaService, ReservaService>();
builder.Services.AddScoped<IRelatorioService, RelatorioService>();
builder.Services.AddScoped<IMotoristaService, MotoristaService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IViagemPublicService, ViagemPublicService>();
builder.Services.AddScoped<MercadoPagoWebhookHandler>();


// ── Validators ──────────────────────────────────────────────────
builder.Services.AddScoped<IValidator<RegistrarGerenteRequest>, RegistrarGerenteValidator>();
builder.Services.AddScoped<IValidator<LoginRequest>, LoginValidator>();
builder.Services.AddScoped<IValidator<RegistrarPassageiroRequest>, RegistrarPassageiroRequestValidator>();
builder.Services.AddScoped<IValidator<CriarVanRequest>, CriarVanValidator>();
builder.Services.AddScoped<IValidator<AtualizarVanRequest>, AtualizarVanValidator>();
builder.Services.AddScoped<IValidator<CriarViagemRequest>, CriarViagemValidator>();
builder.Services.AddScoped<IValidator<AtualizarViagemRequest>, AtualizarViagemValidator>();
builder.Services.AddScoped<IValidator<AlocarVanRequest>, AlocarVanValidator>();
builder.Services.AddScoped<IValidator<RegistrarMotoristaRequest>, RegistrarMotoristaValidator>();
builder.Services.AddScoped<IValidator<CriarReservaRequest>, CriarReservaValidator>();
builder.Services.AddScoped<IValidator<AlocarMotoristaRequest>, AlocarMotoristaValidator>();
builder.Services.AddScoped<IValidator<AtualizarGerenteAdminRequest>, AtualizarGerenteAdminValidator>();
builder.Services.AddScoped<IValidator<CriarGerenteAdminRequest>, CriarGerenteAdminValidator>();




// ── CORS ────────────────────────────────────────────────────────
var corsSection = builder.Configuration.GetSection(CorsSettings.SectionName);
builder.Services.Configure<CorsSettings>(corsSection);

var corsSettings = corsSection.Get<CorsSettings>() ?? new CorsSettings();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(corsSettings.AllowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ── Infrastructure ──────────────────────────────────────────────
builder.Services.AddInfrastructure(builder.Configuration);

// ── Hosted Services ──────────────────────────────────────────────
builder.Services.AddHostedService<ExpirarReservasBackgroundService>();
builder.Services.AddHostedService<DevDataSeeder>();

// ── Controllers + Swagger (Swashbuckle) ─────────────────────────
// Evitar Microsoft.AspNetCore.OpenApi no mesmo projeto: conflito de Microsoft.OpenApi em runtime com Swashbuckle.
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ResultFilter>();
});
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ── Middleware Pipeline ─────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
