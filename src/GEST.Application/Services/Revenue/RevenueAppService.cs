using GEST.Application.Abstractions;
using GEST.Application.Abstractions.Repositories;
using GEST.Application.Dtos.Revenue;
using GEST.Application.Notifications;

namespace GEST.Application.Services.Revenue;

public interface IRevenueAppService
{
    Task<RevenueResponseDto> GetRevenueAsync(RevenueRequestDto request, CancellationToken ct);
}

public sealed class RevenueAppService(
    ISectorRepository sectorRepo,
    IParkingSessionRepository sessionRepo,
    IPublishEvent publisher
    ) : IRevenueAppService
{
    public async Task<RevenueResponseDto> GetRevenueAsync(
        RevenueRequestDto request,
        CancellationToken ct
        )
    {
        var sector = await sectorRepo.GetAsync(request.Sector, ct);
        if (sector is null)
        {
            await publisher.PublishAsync(new DomainNotification("Revenue.SectorNotFound", "Setor não encontrado."), ct);
            return new RevenueResponseDto { Amount = 0m };
        }

        var amount = await sessionRepo.SumRevenueAsync(sector.Id, request.Date, ct);

        return new RevenueResponseDto
        {
            Amount = decimal.Round(amount, 2, MidpointRounding.AwayFromZero),
            // Currency e Timestamp já default
        };
    }
}
