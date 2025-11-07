using GEST.Domain.Entities;

namespace GEST.Application.Abstractions.Repositories;

public interface IVehicleRepository
{
    Task<Vehicle?> GetAsync(string licensePlate, CancellationToken ct);
    Task EnsureAsync(string licensePlate, CancellationToken ct);
}
