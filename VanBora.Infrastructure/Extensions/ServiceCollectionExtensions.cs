using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VanBora.Application.Interfaces;
using VanBora.Application.Settings;
using VanBora.Domain.Interfaces;
using VanBora.Infrastructure.Data;
using VanBora.Infrastructure.Repositories;
using VanBora.Infrastructure.Services;

namespace VanBora.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // DbContext
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Repositories
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IReservaRepository, ReservaRepository>();
        services.AddScoped<IViagemRepository, ViagemRepository>();
        services.AddScoped<IViagemVanRepository, ViagemVanRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Services
        services.AddScoped<ITokenService, TokenService>();

        services.Configure<MercadoPagoSettings>(configuration.GetSection(MercadoPagoSettings.SectionName));
        services.AddHttpClient<IPagamentoGateway, MercadoPagoPagamentoGateway>();

        return services;
    }
}
