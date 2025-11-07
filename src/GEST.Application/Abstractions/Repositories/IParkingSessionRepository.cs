using GEST.Domain.Entities;

namespace GEST.Application.Abstractions.Repositories;

public interface IParkingSessionRepository
{
    Task<ParkingSession?> GetActiveByPlateAsync(string licensePlate, CancellationToken ct);
    Task AddAsync(ParkingSession session, CancellationToken ct);
    Task CloseAsync(Guid sessionId, DateTime exitUtc, decimal totalAmount, CancellationToken ct);
    Task<decimal> SumRevenueAsync(string sectorCode, DateOnly dateUtc, CancellationToken ct);
}
