using GEST.Application.Abstractions.Repositories;
using GEST.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GEST.Infrastructure.Persistence.Repositories;

public class SectorRepository(
    GestDbContext db
    ) : BaseRepository<Sector>(db), ISectorRepository
{
    public async Task<Sector?> GetAsync(string code, CancellationToken ct)
        => await db.Sectors.AsNoTracking().FirstOrDefaultAsync(s => s.Code == code, ct);

    public async Task UpsertAsync(IEnumerable<Sector> sectors, CancellationToken ct)
    {
        var set = db.Sectors;
        foreach (var sector in sectors)
        {
            var existing = await set.FirstOrDefaultAsync(x => x.Code == sector.Code, ct);
            if (existing is null)
            {
                await set.AddAsync(sector, ct);
            }
            else
            {
                existing.BasePrice = sector.BasePrice;
                existing.MaxCapacity = sector.MaxCapacity;
                // Spots são tratados no SpotRepository
            }
        }
    }
}
