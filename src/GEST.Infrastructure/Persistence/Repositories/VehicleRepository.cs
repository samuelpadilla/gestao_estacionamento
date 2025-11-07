using GEST.Application.Abstractions.Repositories;
using GEST.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GEST.Infrastructure.Persistence.Repositories;

public sealed class VehicleRepository(GestDbContext db) : IVehicleRepository
{
    private readonly GestDbContext _db = db;

    public async Task<Vehicle?> GetAsync(string licensePlate, CancellationToken ct)
        => await _db.Vehicles.AsNoTracking().FirstOrDefaultAsync(v => v.LicensePlate == licensePlate, ct);

    public async Task EnsureAsync(string licensePlate, CancellationToken ct)
    {
        var exists = await _db.Vehicles.AnyAsync(v => v.LicensePlate == licensePlate, ct);

        if (!exists)
            await _db.Vehicles.AddAsync(new Vehicle { LicensePlate = licensePlate }, ct);
    }
}
