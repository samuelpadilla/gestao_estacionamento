using GEST.Application.Abstractions.Repositories;
using GEST.Domain.Entities;
using GEST.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GEST.Infrastructure.Persistence.Repositories;

public sealed class ParkingSessionRepository(GestDbContext db) : IParkingSessionRepository
{
    private readonly GestDbContext _db = db;

    public async Task<ParkingSession?> GetActiveByPlateAsync(string licensePlate, CancellationToken ct)
        => await _db.ParkingSessions
            .FirstOrDefaultAsync(s => s.LicensePlate == licensePlate && s.Status == ParkingStatus.Active, ct);

    public async Task AddAsync(ParkingSession session, CancellationToken ct)
        => await _db.ParkingSessions.AddAsync(session, ct);

    public async Task CloseAsync(Guid sessionId, DateTime exitUtc, decimal totalAmount, CancellationToken ct)
    {
        var session = await _db.ParkingSessions.FirstOrDefaultAsync(s => s.Id == sessionId, ct)
                      ?? throw new InvalidOperationException("Sessão não encontrada.");
        session.ExitTimeUtc = exitUtc;
        session.TotalAmount = totalAmount;
        session.Status = ParkingStatus.Closed;
    }

    public async Task<decimal> SumRevenueAsync(string sectorCode, DateOnly dateUtc, CancellationToken ct)
    {
        var start = dateUtc.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var end = dateUtc.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

        return await _db.ParkingSessions
            .Where(s => s.SectorCode == sectorCode
                        && s.Status == ParkingStatus.Closed
                        && s.ExitTimeUtc != null
                        && s.ExitTimeUtc >= start
                        && s.ExitTimeUtc <= end)
            .SumAsync(s => s.TotalAmount ?? 0m, ct);
    }
}
