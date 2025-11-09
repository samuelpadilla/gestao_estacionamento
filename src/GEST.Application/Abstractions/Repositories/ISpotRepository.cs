using GEST.Domain.Entities;

namespace GEST.Application.Abstractions.Repositories;

public interface ISpotRepository : IBaseRepository<Spot>
{
    Task<int> CountAllAsync(CancellationToken ct);
    Task<int> CountAvailableAsync(CancellationToken ct);
    Task<int> CountOccupiedBySectorIdAsync(int sectorId, CancellationToken ct);
    Task UpsertAsync(IEnumerable<Spot> spots, CancellationToken ct);
    Task<Spot?> FindByGeoAsync(double lat, double lng, CancellationToken ct);
    Task SetOccupiedAsync(int spotId, string licensePlate, CancellationToken ct);
    Task FreeAsync(int spotId, CancellationToken ct);
}
