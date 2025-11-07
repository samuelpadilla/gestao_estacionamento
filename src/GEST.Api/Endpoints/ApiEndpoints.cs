using FluentValidation;
using GEST.Application.Abstractions.Repositories;
using GEST.Application.Dtos.Revenue;
using GEST.Application.Dtos.Webhook;
using GEST.Application.Services.Garage;
using GEST.Application.Services.Parking;
using GEST.Application.Services.Revenue;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace GEST.Api.Endpoints;

public static class ApiEndpoints
{
    public static IEndpointRouteBuilder MapGestEndpoints(this IEndpointRouteBuilder app)
    {
        MapGarage(app);
        MapRevenue(app);
        MapWebhook(app);
        return app;
    }

    private static void MapGarage(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/garage")
                       .WithTags("Garage");

        // POST /garage/sync -> força ressincronização com o simulador
        group.MapPost("/sync", async (IGarageSyncAppService service, CancellationToken ct) =>
        {
            await service.SyncAsync(ct);
            return Results.Ok(new { message = "Garage synchronized." });
        })
        .WithSummary("Sincroniza dados da garagem")
        .WithDescription("Busca /garage no simulador e aplica upsert em setores e vagas.");

        // GET /garage/state -> estado atual (capacidade x ocupados) por setor
        group.MapGet("/state", async ([FromServices] ISectorRepository sectors,
                                      [FromServices] ISpotRepository spots,
                                      CancellationToken ct) =>
        {
            // se precisar, carregue lista de setores do seu repositório/DbContext
            // aqui, usamos o ISectorRepository + SpotRepository para montar um snapshot simples
            // NOTE: ISectorRepository ainda não tem "GetAll" no stub. Se preferir, exponha no Infra
            // Para manter simples, mostraremos apenas um resumo por setor conhecido no banco
            // -> Alternativa: criar um repositório de consulta ad hoc. Aqui vai uma versão mínima:

            return Results.Ok(new { message = "Use uma QueryService/DbContext para projetar o estado detalhado." });
        })
        .WithSummary("Snapshot do estado da garagem")
        .WithDescription("Retorna um panorama da lotação/ocupação por setor (implemente sua Query).");
    }

    private static void MapRevenue(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/revenue")
                       .WithTags("Revenue");

        // GET /revenue?date=2025-01-01&sector=A  (mais RESTful)
        group.MapGet("", async ([FromQuery, Required] DateOnly date,
                                [FromQuery, Required] string sector,
                                [FromServices] IRevenueAppService revenue,
                                [FromServices] IValidator<RevenueRequestDto> validator,
                                CancellationToken ct) =>
        {
            var request = new RevenueRequestDto { Date = date, Sector = sector };
            var val = await validator.ValidateAsync(request, ct);
            if (!val.IsValid)
                return Results.ValidationProblem(val.ToDictionary());

            var response = await revenue.GetRevenueAsync(request, ct);
            // adapta para o contrato do teste {amount, currency, timestamp}
            return Results.Ok(new
            {
                amount = response.Amount,
                currency = response.Currency,
                timestamp = response.Timestamp
            });
        })
        .WithSummary("Consulta faturamento por setor/data (query string)");

        // POST /revenue  (compatível com enunciado que envia JSON no GET, mas aqui usamos POST)
        group.MapPost("", async ([FromBody] RevenueRequestDto request,
                                 [FromServices] IRevenueAppService revenue,
                                 [FromServices] IValidator<RevenueRequestDto> validator,
                                 CancellationToken ct) =>
        {
            var val = await validator.ValidateAsync(request, ct);
            if (!val.IsValid)
                return Results.ValidationProblem(val.ToDictionary());

            var response = await revenue.GetRevenueAsync(request, ct);
            return Results.Ok(new
            {
                amount = response.Amount,
                currency = response.Currency,
                timestamp = response.Timestamp
            });
        })
        .WithSummary("Consulta faturamento por setor/data (body)");
    }

    private static void MapWebhook(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/webhook")
                       .WithTags("Webhook");

        // Um único endpoint que roteia por event_type
        group.MapPost("", async ([FromBody] dynamic raw,
                                 [FromServices] IParkingAppService parking,
                                 [FromServices] IValidator<EntryEventDto> entryValidator,
                                 [FromServices] IValidator<ParkedEventDto> parkedValidator,
                                 [FromServices] IValidator<ExitEventDto> exitValidator,
                                 CancellationToken ct) =>
        {
            try
            {
                // event_type vem em snake_case no enunciado
                string eventType = (string)(raw?.event_type ?? string.Empty);

                switch (eventType)
                {
                    case "ENTRY":
                        {
                            var dto = System.Text.Json.JsonSerializer.Deserialize<EntryEventDto>(
                                System.Text.Json.JsonSerializer.Serialize(raw))!;
                            var val = await entryValidator.ValidateAsync(dto, ct);
                            if (!val.IsValid) return Results.ValidationProblem(val.ToDictionary());

                            await parking.HandleEntryAsync(dto, ct);
                            return Results.Ok();
                        }
                    case "PARKED":
                        {
                            var dto = System.Text.Json.JsonSerializer.Deserialize<ParkedEventDto>(
                                System.Text.Json.JsonSerializer.Serialize(raw))!;
                            var val = await parkedValidator.ValidateAsync(dto, ct);
                            if (!val.IsValid) return Results.ValidationProblem(val.ToDictionary());

                            await parking.HandleParkedAsync(dto, ct);
                            return Results.Ok();
                        }
                    case "EXIT":
                        {
                            var dto = System.Text.Json.JsonSerializer.Deserialize<ExitEventDto>(
                                System.Text.Json.JsonSerializer.Serialize(raw))!;
                            var val = await exitValidator.ValidateAsync(dto, ct);
                            if (!val.IsValid) return Results.ValidationProblem(val.ToDictionary());

                            await parking.HandleExitAsync(dto, ct);
                            return Results.Ok();
                        }
                    default:
                        return Results.BadRequest(new { error = "event_type inválido. Use ENTRY, PARKED ou EXIT." });
                }
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Erro ao processar webhook");
            }
        })
        .WithSummary("Recebe eventos ENTRY, PARKED e EXIT")
        .WithDescription("Endpoint único que roteia por event_type conforme o simulador.");
    }
}
