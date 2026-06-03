using Microsoft.EntityFrameworkCore;
using VanBora.Domain.Entities;
using VanBora.Domain.Enums;
using VanBora.Domain.ValueObjects;
using VanBora.Infrastructure.Data;

namespace Api.Services;

/// <summary>
/// Popula viagem/van de demonstração em Development quando o banco está vazio.
/// </summary>
public class DevDataSeeder : IHostedService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<DevDataSeeder> _logger;
    private readonly IWebHostEnvironment _env;

    public DevDataSeeder(IServiceProvider services, ILogger<DevDataSeeder> logger, IWebHostEnvironment env)
    {
        _services = services;
        _logger = logger;
        _env = env;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_env.IsDevelopment())
            return;

        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        try
        {
            await db.Database.MigrateAsync(cancellationToken);

            if (await db.Viagens.AnyAsync(cancellationToken))
                return;

            var gerente = await db.Usuarios
                .FirstOrDefaultAsync(u => u.Tipo == TipoUsuario.Gerente, cancellationToken);

            if (gerente is null)
            {
                _logger.LogWarning("DevDataSeeder: nenhum gerente no banco — crie um via POST /api/auth/gerente/registrar");
                return;
            }

            var placa = Placa.Criar("ABC1D34");
            if (placa.IsFailure) return;

            var van = new Van(gerente.Id, "Van Nilton", placa.Value, "Mercedes Sprinter", 17);
            await db.Vans.AddAsync(van, cancellationToken);

            var viagem = new Viagem(
                gerente.Id,
                "Botafogo x Flamengo",
                DateTime.UtcNow.AddDays(14),
                "Estádio Nilton Santos",
                DateTime.UtcNow.AddDays(14).AddHours(-3),
                "Shopping Nova América — saída 14h",
                89.90m,
                true);

            var viagemVan = new ViagemVan(viagem.Id, van);
            viagem.AdicionarViagemVan(viagemVan);

            await db.Viagens.AddAsync(viagem, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "DevDataSeeder: viagem demo criada. ViagemVanId={ViagemVanId}",
                viagemVan.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DevDataSeeder falhou");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
