using FluentValidation;
using GEST.Application.Dtos.Webhook;
using GEST.Application.Services.Parking;
using GEST.Application.Notifications;
using System.Text.Json;

namespace GEST.Api.Endpoints.Garage;

public static class WebhookEndpoints
{
    public static void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/webhook").WithTags("Webhook");

        // Um único endpoint que roteia por event_type
        group.MapPost("", async (
            JsonElement raw,
            IParkingAppService parking,
            IValidator<EntryEventDto> entryValidator,
            IValidator<ParkedEventDto> parkedValidator,
            IValidator<ExitEventDto> exitValidator,
            INotificationContext notificationContext,
            CancellationToken ct) =>
        {
            if (!raw.TryGetProperty("event_type", out var eventTypeProp) || eventTypeProp.ValueKind != JsonValueKind.String)
                return Results.BadRequest(new { error = "Campo 'event_type' é obrigatório." });

            var eventType = eventTypeProp.GetString();

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // ajuda a casar event_type/Event_Type etc.
            };

            switch (eventType)
            {
                case "ENTRY":
                    {
                        var dto = raw.Deserialize<EntryEventDto>(jsonOptions)!;
                        var val = await entryValidator.ValidateAsync(dto, ct);
                        if (!val.IsValid)
                            return Results.ValidationProblem(val.ToDictionary());

                        await parking.HandleEntryAsync(dto, ct);
                        if (notificationContext.HasNotifications())
                            return BuildNotificationResult(notificationContext);

                        return Results.Ok();
                    }

                case "PARKED":
                    {
                        var dto = raw.Deserialize<ParkedEventDto>(jsonOptions)!;
                        var val = await parkedValidator.ValidateAsync(dto, ct);
                        if (!val.IsValid)
                            return Results.ValidationProblem(val.ToDictionary());

                        await parking.HandleParkedAsync(dto, ct);
                        if (notificationContext.HasNotifications())
                            return BuildNotificationResult(notificationContext);

                        return Results.Ok();
                    }

                case "EXIT":
                    {
                        var dto = raw.Deserialize<ExitEventDto>(jsonOptions)!;
                        var val = await exitValidator.ValidateAsync(dto, ct);
                        if (!val.IsValid)
                            return Results.ValidationProblem(val.ToDictionary());

                        await parking.HandleExitAsync(dto, ct);
                        if (notificationContext.HasNotifications())
                            return BuildNotificationResult(notificationContext);

                        return Results.Ok();
                    }

                default:
                    return Results.BadRequest(new { error = "event_type inválido. Use ENTRY, PARKED ou EXIT." });
            }
        })
        .WithSummary("Recebe eventos ENTRY, PARKED e EXIT")
        .WithDescription("Endpoint único que roteia por event_type conforme o simulador.");
    }

    private static IResult BuildNotificationResult(INotificationContext ctx)
    {
        var dict = ctx.GetAll()
            .GroupBy(n => n.Key)
            .ToDictionary(g => g.Key, g => g.Select(n => n.Message).ToArray());

        //422 Unprocessable Entity no formato ValidationProblem para padronizar com FluentValidation
        return Results.ValidationProblem(dict, statusCode: 422, title: "Business rule violation");
    }
}
