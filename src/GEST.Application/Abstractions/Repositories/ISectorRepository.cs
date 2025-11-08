using GEST.Domain.Entities;

namespace GEST.Application.Abstractions.Repositories;

public interface ISectorRepository
{
    Task<Sector?> GetAsync(string code, CancellationToken ct);

    Task<IEnumerable<Sector>> GetAllAsync(CancellationToken ct);

    Task UpsertAsync(IEnumerable<Sector> sectors, CancellationToken ct);
}
