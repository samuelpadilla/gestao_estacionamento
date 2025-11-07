using GEST.Application.Dtos.Garage;

namespace GEST.Application.Abstractions.Gateways;

public interface IGarageGateway
{
    Task<GarageConfigDto> GetGarageConfigAsync(CancellationToken ct);
}