using GEST.Application.Abstractions.Repositories;
using GEST.Application.Dtos.Revenue;

namespace GEST.Application.Services.Revenue;

public interface IRevenueAppService
{
    Task<RevenueResponseDto> GetRevenueAsync(RevenueRequestDto request, CancellationToken ct);
}

public sealed class RevenueAppService(
    ISectorRepository sectorRepo,
    IParkingSessionRepository sessionRepo
    ) : IRevenueAppService
{
    public async Task<RevenueResponseDto> GetRevenueAsync(
        RevenueRequestDto request, 
        CancellationToken ct
        )
    {
        var sector = await sectorRepo.GetAsync(request.Sector, ct)
                     ?? throw new InvalidOperationException("Setor não encontrado.");

        var amount = await sessionRepo.SumRevenueAsync(sector.Id, request.Date, ct);

        return new RevenueResponseDto
        {
            Amount = decimal.Round(amount, 2, MidpointRounding.AwayFromZero),
            // Currency e Timestamp já default
        };
    }
}
