using GEST.Domain.Entities;

namespace GEST.Application.Abstractions.Repositories;

public interface ISpotRepository
{
    Task<IEnumerable<Spot>> GetAllAsync(CancellationToken ct);
    Task<int> CountOccupiedAsync(string sectorCode, CancellationToken ct);
    Task UpsertAsync(IEnumerable<Spot> spots, CancellationToken ct);
    Task<Spot?> FindByGeoAsync(string sectorCode, double lat, double lng, CancellationToken ct);
    Task SetOccupiedAsync(int spotId, string licensePlate, CancellationToken ct);
    Task FreeAsync(int spotId, CancellationToken ct);
}
