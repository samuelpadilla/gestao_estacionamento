using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace GEST.Infrastructure.Persistence.DesignTime;

public sealed class GestDbContextFactory : IDesignTimeDbContextFactory<GestDbContext>
{
    public GestDbContext CreateDbContext(string[] args)
    {
        // descobre o diretório da API para ler o appsettings (ajuste se seu path for diferente)
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "GEST.Api");

        var config = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var cs = config.GetConnectionString("SqlServer")
                 ?? "Server=localhost;Database=GestDb;Trusted_Connection=True;TrustServerCertificate=True;";

        var options = new DbContextOptionsBuilder<GestDbContext>()
            .UseSqlServer(cs)
            .Options;

        return new GestDbContext(options);
    }
}
