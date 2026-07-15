using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VanBora.Application.Interfaces;
using VanBora.Domain.Interfaces;
using VanBora.Infrastructure.Configuration;
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
        services.AddScoped<IVanRepository, VanRepository>();
        services.AddScoped<IViagemRepository, ViagemRepository>();
        services.AddScoped<IViagemVanRepository, ViagemVanRepository>();
        services.AddScoped<IReservaRepository, ReservaRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Services
        services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IEmailService, EmailService>();

        // Gateways
        services.AddHttpClient<IPagamentoGateway, MercadoPagoPagamentoGateway>();

        return services;
    }
}
