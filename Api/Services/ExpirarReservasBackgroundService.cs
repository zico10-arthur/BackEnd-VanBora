using VanBora.Application.Interfaces;

namespace Api.Services;

public class ExpirarReservasBackgroundService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<ExpirarReservasBackgroundService> _logger;

    public ExpirarReservasBackgroundService(
        IServiceProvider services,
        ILogger<ExpirarReservasBackgroundService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _services.CreateScope();
                var reservaService = scope.ServiceProvider.GetRequiredService<IReservaService>();
                await reservaService.ExpirarReservasPendentesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao expirar reservas pendentes");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
