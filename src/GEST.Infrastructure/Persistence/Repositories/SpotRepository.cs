using GEST.Application.Abstractions.Repositories;
using GEST.Application.Abstractions; // para IPublishEvent
using GEST.Application.Notifications; // DomainNotification
using GEST.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GEST.Infrastructure.Persistence.Repositories;

public sealed class SpotRepository(
    GestDbContext db,
    IPublishEvent publisher
    ) : BaseRepository<Spot>(db), ISpotRepository
{
    public async Task<int> CountAllAsync(CancellationToken ct)
        => await db.Spots.CountAsync(ct);

    public async Task<int> CountAvailableAsync(CancellationToken ct)
        => await db.Spots.CountAsync(s => !s.IsOccupied, ct);

    public async Task<int> CountOccupiedBySectorIdAsync(int sectorId, CancellationToken ct)
        => await db.Spots.CountAsync(s => s.SectorId == sectorId && s.IsOccupied, ct);

    public async Task UpsertAsync(IEnumerable<Spot> spots, CancellationToken ct)
    {
        var set = db.Spots;
        foreach (var spot in spots)
        {
            var existing = await set.FirstOrDefaultAsync(x => x.Id == spot.Id, ct);
            if (existing is null)
            {
                await set.AddAsync(spot, ct);
            }
            else
            {
                existing.SectorId = spot.SectorId;
                existing.Lat = spot.Lat;
                existing.Lng = spot.Lng;
                // IsOccupied/CurrentLicensePlate são atualizados por eventos
            }
        }
    }

    public async Task<Spot?> FindByGeoAsync(double lat, double lng, CancellationToken ct)
        => await db.Spots
            .AsNoTracking()
            .Where(s => s.Lat == lat && s.Lng == lng)
            .FirstOrDefaultAsync(ct);

    public async Task SetOccupiedAsync(int spotId, string licensePlate, CancellationToken ct)
    {
        var spot = await db.Spots.FirstOrDefaultAsync(s => s.Id == spotId, ct);
        if (spot is null)
        {
            await publisher.PublishAsync(new DomainNotification("Spot.SetOccupied.NotFound", "Vaga não encontrada."), ct);
            return;
        }
        spot.IsOccupied = true;
        spot.CurrentLicensePlate = licensePlate;
    }

    public async Task FreeAsync(int spotId, CancellationToken ct)
    {
        var spot = await db.Spots.FirstOrDefaultAsync(s => s.Id == spotId, ct);
        if (spot is null)
        {
            await publisher.PublishAsync(new DomainNotification("Spot.Free.NotFound", "Vaga não encontrada."), ct);
            return;
        }
        spot.IsOccupied = false;
        spot.CurrentLicensePlate = null;
    }
}