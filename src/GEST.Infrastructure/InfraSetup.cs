using GEST.Domain.Abstractions;
using GEST.Infrastructure.Data;
using GEST.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GEST.Infrastructure;

public static class InfraSetup
{
    public static IServiceCollection AdicionarInfra(this IServiceCollection services, IConfiguration cfg, IHostEnvironment env)
    {
        var connSigaBoletos = cfg.GetConnectionString("ConnGest") ?? "";
        services.AddDbContext<GestContext>(db =>
        {
            var opts = db.UseSqlServer(connSigaBoletos);

            if (env.IsDevelopment())
            {
                opts.EnableSensitiveDataLogging();
                opts.EnableDetailedErrors();
            }
        });

        services.AddScoped<GestContext>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
