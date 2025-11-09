using GEST.Application.Abstractions.Repositories;
using GEST.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GEST.Infrastructure.Persistence.Repositories;

public sealed class VehicleRepository(
    GestDbContext db
    ) : BaseRepository<Vehicle>(db), IVehicleRepository
{
    public async Task<Vehicle?> GetByLicensePlateAsync(string licensePlate, CancellationToken ct)
        => await db.Vehicles
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.LicensePlate == licensePlate, ct);
}
