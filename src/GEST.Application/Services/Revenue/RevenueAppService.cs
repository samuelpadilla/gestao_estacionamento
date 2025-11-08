using GEST.Application.Abstractions.Repositories;
using GEST.Application.Dtos.Revenue;

namespace GEST.Application.Services.Revenue;

public interface IRevenueAppService
{
    Task<RevenueResponseDto> GetRevenueAsync(RevenueRequestDto request, CancellationToken ct);
}

public sealed class RevenueAppService : IRevenueAppService
{
    private readonly IParkingSessionRepository _sessions;

    public RevenueAppService(IParkingSessionRepository sessions) => _sessions = sessions;

    public async Task<RevenueResponseDto> GetRevenueAsync(RevenueRequestDto request, CancellationToken ct)
    {
        var amount = await _sessions.SumRevenueAsync(request.Sector, request.Date, ct);

        return new RevenueResponseDto
        {
            Amount = decimal.Round(amount, 2, MidpointRounding.AwayFromZero),
            // Currency e Timestamp já default
        };
    }
}
