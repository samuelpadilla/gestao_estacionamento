using GEST.Application.Abstractions.Gateways;
using GEST.Application.Abstractions.Repositories;
using GEST.Domain.Abstractions;
using GEST.Infrastructure.Http;
using GEST.Infrastructure.Persistence;
using GEST.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net.Http.Headers;

namespace GEST.Infrastructure.Setup;

public static class InfrastructureSetup
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config, IHostEnvironment env)
    {
        var connSigaBoletos = config.GetConnectionString("ConnGest") ?? "";
        services.AddDbContext<GestDbContext>(db =>
        {
            var opts = db.UseSqlServer(connSigaBoletos);

            if (env.IsDevelopment())
            {
                opts.EnableSensitiveDataLogging();
                opts.EnableDetailedErrors();
            }
        });

        services.AddHttpClient<IGarageGateway, GarageGateway>(client =>
        {
            // Ajuste a base conforme o simulador
            // Ex.: "GarageApi:BaseUrl": "https://localhost:5005"
            var baseUrl = config["GarageApi:BaseUrl"] ?? "http://localhost:5000";
            client.BaseAddress = new Uri(baseUrl);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        });

        services.AddScoped<ISectorRepository, SectorRepository>();
        services.AddScoped<ISpotRepository, SpotRepository>();
        services.AddScoped<IVehicleRepository, VehicleRepository>();
        services.AddScoped<IParkingSessionRepository, ParkingSessionRepository>();
        services.AddScoped<IWebhookEventLogRepository, WebhookEventLogRepository>();

        services.AddScoped<GestDbContext>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
