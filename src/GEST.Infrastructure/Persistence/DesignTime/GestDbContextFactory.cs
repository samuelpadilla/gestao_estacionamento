using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace GEST.Infrastructure.Persistence.DesignTime;

public sealed class GestDbContextFactory : IDesignTimeDbContextFactory<GestDbContext>
{
    public GestDbContext CreateDbContext(string[] args)
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
                
        var apiPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "GEST.Api"));
        Console.WriteLine($"[GestDbContextFactory] Base path for configuration: {apiPath}");


        var cfgBuilder = new ConfigurationBuilder();

        if (Directory.Exists(apiPath))
        {
            cfgBuilder.SetBasePath(apiPath)
                      .AddJsonFile("appsettings.json", optional: true)
                      .AddJsonFile($"appsettings.{env}.json", optional: true);
        }

        cfgBuilder.AddEnvironmentVariables();
        if (env.Equals("Development", StringComparison.OrdinalIgnoreCase))
            cfgBuilder.AddUserSecrets<GestDbContextFactory>(optional: true);

        var config = cfgBuilder.Build();

        var connectionString = config.GetConnectionString("ConnGest")
                               ?? "Server=localhost;Database=GestDb;Trusted_Connection=True;TrustServerCertificate=True;";

        Console.WriteLine($"[GestDbContextFactory] Using connection string: {connectionString}");

        var options = new DbContextOptionsBuilder<GestDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new GestDbContext(options);
    }
}
