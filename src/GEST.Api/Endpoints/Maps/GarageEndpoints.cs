using GEST.Application.Services.Garage;

namespace GEST.Api.Endpoints.Garage;

public static class GarageEndpoints
{
    public static void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/garage").WithTags("Garage");

        group.MapGet("", async (IGarageAppService service, CancellationToken ct) =>
        {
            var garage = await service.GetAsync(ct);
            return Results.Ok(garage);
        })
        .WithSummary("Configuração da garagem")
        .WithDescription("Retorna setores e vagas da garagem.");

        group.MapGet("/state", async (IGarageAppService service, CancellationToken ct) =>
        {
            var state = await service.GetStateAsync(ct);
            return Results.Ok(state);
        })
        .WithSummary("Estado da garagem")
        .WithDescription("Retorna um panorama da lotação/ocupação por setor.");
    }
}
