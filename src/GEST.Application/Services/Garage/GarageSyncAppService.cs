using AutoMapper;
using GEST.Application.Abstractions.Gateways;
using GEST.Application.Abstractions.Repositories;
using GEST.Domain.Abstractions;
using GEST.Domain.Entities;

namespace GEST.Application.Services.Garage;

public interface IGarageSyncAppService
{
    Task SyncAsync(CancellationToken ct);
}

public sealed class GarageSyncAppService(
    IGarageGateway gateway,
    ISectorRepository sectorRepo,
    ISpotRepository spotRepo,
    IMapper mapper,
    IUnitOfWork uow
    ) : IGarageSyncAppService
{
    public async Task SyncAsync(CancellationToken ct)
    {
        var dto = await gateway.GetGarageConfigAsync(ct);

        var sectors = mapper.Map<List<Sector>>(dto.Garage);
        var spots = mapper.Map<List<Spot>>(dto.Spots);

        await uow.ExecuteInTransactionAsync(async _ =>
        {
            await sectorRepo.UpsertAsync(sectors, ct);
            await spotRepo.UpsertAsync(spots, ct);
            await uow.SaveChangesAsync(ct);
        }, ct: ct);
    }
}
