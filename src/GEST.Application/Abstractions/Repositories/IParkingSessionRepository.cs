using GEST.Domain.Entities;

namespace GEST.Application.Abstractions.Repositories;

public interface IParkingSessionRepository : IBaseRepository<ParkingSession>
{
    Task<int> CountActiveAsync(CancellationToken ct);
    Task<ParkingSession?> GetActiveByPlateAsync(string licensePlate, CancellationToken ct);
    Task CloseAsync(Guid sessionId, DateTime exitUtc, decimal totalAmount, CancellationToken ct);
    Task<decimal> SumRevenueAsync(int sectorId, DateOnly dateUtc, CancellationToken ct);
}
