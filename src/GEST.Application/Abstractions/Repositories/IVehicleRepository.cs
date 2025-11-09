using GEST.Domain.Entities;

namespace GEST.Application.Abstractions.Repositories;

public interface IVehicleRepository : IBaseRepository<Vehicle>
{
    Task<Vehicle?> GetByLicensePlateAsync(string licensePlate, CancellationToken ct);
}
