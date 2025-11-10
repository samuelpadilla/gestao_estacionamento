using GEST.Application.Abstractions.Repositories;
using GEST.Domain.Entities;
using GEST.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GEST.Infrastructure.Persistence.Repositories;

public class ParkingSessionRepository(
    GestDbContext db
    ) : BaseRepository<ParkingSession>(db), IParkingSessionRepository
{
    public async Task<int> CountActiveAsync(CancellationToken ct)
        => await db.ParkingSessions.CountAsync(s => s.Status == ParkingStatus.Active, ct);

    public async Task<ParkingSession?> GetActiveByPlateAsync(string licensePlate, CancellationToken ct)
        => await db.ParkingSessions
            .FirstOrDefaultAsync(s => s.Vehicle.LicensePlate == licensePlate && s.Status == ParkingStatus.Active, ct);

    public async Task CloseAsync(Guid sessionId, DateTime exitUtc, decimal totalAmount, CancellationToken ct)
    {
        var session = await db.ParkingSessions.FirstOrDefaultAsync(s => s.Id == sessionId, ct)
                      ?? throw new InvalidOperationException("Sessão não encontrada.");
        session.ExitTimeUtc = exitUtc;
        session.TotalAmount = totalAmount;
        session.Status = ParkingStatus.Closed;
    }

    public async Task<decimal> SumRevenueAsync(int sectorId, DateOnly dateUtc, CancellationToken ct)
    {
        var start = dateUtc.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var end = dateUtc.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

        return await db.ParkingSessions
            .Where(s => s.SectorId == sectorId
                        && s.Status == ParkingStatus.Closed
                        && s.ExitTimeUtc != null
                        && s.ExitTimeUtc >= start
                        && s.ExitTimeUtc <= end)
            .SumAsync(s => s.TotalAmount ?? 0m, ct);
    }
}
