using GEST.Application.Abstractions.Repositories;
using GEST.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GEST.Infrastructure.Persistence.Repositories;

public sealed class SpotRepository(GestDbContext db) : ISpotRepository
{
    private readonly GestDbContext _db = db;

    public async Task<IEnumerable<Spot>> GetAllAsync(CancellationToken ct)
        => await _db.Spots.AsNoTracking().ToListAsync(ct);

    public async Task<int> CountOccupiedAsync(string sectorCode, CancellationToken ct)
        => await _db.Spots.CountAsync(s => s.SectorCode == sectorCode && s.IsOccupied, ct);

    public async Task UpsertAsync(IEnumerable<Spot> spots, CancellationToken ct)
    {
        var set = _db.Spots;
        foreach (var spot in spots)
        {
            var existing = await set.FirstOrDefaultAsync(x => x.Id == spot.Id, ct);
            if (existing is null)
            {
                await set.AddAsync(spot, ct);
            }
            else
            {
                existing.SectorCode = spot.SectorCode;
                existing.Lat = spot.Lat;
                existing.Lng = spot.Lng;
                // IsOccupied/CurrentLicensePlate são atualizados por eventos
            }
        }
    }

    public async Task<Spot?> FindByGeoAsync(string sectorCode, double lat, double lng, CancellationToken ct)
        => await _db.Spots
            .AsNoTracking()
            .Where(s => s.SectorCode == sectorCode)
            .OrderBy(s => Math.Abs(s.Lat - lat) + Math.Abs(s.Lng - lng)) // heurística simples
            .FirstOrDefaultAsync(ct);

    public async Task SetOccupiedAsync(int spotId, string licensePlate, CancellationToken ct)
    {
        var spot = await _db.Spots.FirstOrDefaultAsync(s => s.Id == spotId, ct)
                   ?? throw new InvalidOperationException("Vaga não encontrada.");
        spot.IsOccupied = true;
        spot.CurrentLicensePlate = licensePlate;
        // RowVersion resolverá concorrência caso configurado nas entidades
    }

    public async Task FreeAsync(int spotId, CancellationToken ct)
    {
        var spot = await _db.Spots.FirstOrDefaultAsync(s => s.Id == spotId, ct)
                   ?? throw new InvalidOperationException("Vaga não encontrada.");
        spot.IsOccupied = false;
        spot.CurrentLicensePlate = null;
    }
}