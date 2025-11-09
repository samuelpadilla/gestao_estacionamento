using GEST.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GEST.Infrastructure.Persistence;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<GestDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("SeedData");

        try
        {
            await db.Database.MigrateAsync();

            if (!await db.Sectors.AnyAsync())
            {
                var sectors = new List<Sector>
                {
                    new() { Code = "A", BasePrice = 10m, MaxCapacity = 10 },
                    new() { Code = "B", BasePrice = 8m,  MaxCapacity = 20 },
                    new() { Code = "C", BasePrice = 12m, MaxCapacity = 30 },
                };

                await db.Sectors.AddRangeAsync(sectors);
                await db.SaveChangesAsync();

                logger.LogInformation("SeedData: {count} setores inseridos com sucesso.", sectors.Count);
            }
            else
            {
                logger.LogInformation("SeedData: setores já existentes.");
            }

            if (!await db.Spots.AnyAsync())
            {
                var sectors = await db.Sectors.ToListAsync();
                var random = new Random();
                var spots = new List<Spot>();

                foreach (var sector in sectors)
                {
                    for (int i = 1; i <= sector.MaxCapacity; i++)
                    {
                        double baseLat = -23.5616;
                        double baseLng = -46.6559;
                        double lat = baseLat + random.NextDouble() * 0.001;
                        double lng = baseLng + random.NextDouble() * 0.001;

                        spots.Add(new Spot
                        {
                            SectorId = sector.Id,
                            Lat = Math.Round(lat, 6),
                            Lng = Math.Round(lng, 6),
                            IsOccupied = false
                        });
                    }
                }

                await db.Spots.AddRangeAsync(spots);
                await db.SaveChangesAsync();

                logger.LogInformation("SeedData: {count} vagas (Spots) inseridas com sucesso.", spots.Count);
            }
            else
            {
                logger.LogInformation("SeedData: vagas já existentes.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falha ao executar SeedData");
            throw;
        }
    }
}
