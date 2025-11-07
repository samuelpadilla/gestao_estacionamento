using GEST.Application.Abstractions.Gateways;
using GEST.Application.Dtos.Garage;
using System.Net.Http.Json;

namespace GEST.Infrastructure.Http;

public sealed class GarageGateway(HttpClient http) : IGarageGateway
{
    private readonly HttpClient _http = http;

    public async Task<GarageConfigDto> GetGarageConfigAsync(CancellationToken ct)
    {
        // BaseAddress configurada no DI (ex.: https://simulador.local/)
        var result = await _http.GetFromJsonAsync<GarageConfigDto>("/garage", cancellationToken: ct);
        return result ?? new GarageConfigDto();
    }
}
