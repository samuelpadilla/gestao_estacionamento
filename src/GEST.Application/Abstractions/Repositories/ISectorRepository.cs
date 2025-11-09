using GEST.Domain.Entities;

namespace GEST.Application.Abstractions.Repositories;

public interface ISectorRepository : IBaseRepository<Sector>
{
    Task<Sector?> GetAsync(string code, CancellationToken ct);

    Task UpsertAsync(IEnumerable<Sector> sectors, CancellationToken ct);
}
