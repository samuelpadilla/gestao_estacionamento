using FluentValidation;
using GEST.Application.Dtos.Revenue;
using GEST.Application.Services.Revenue;
using GEST.Application.Notifications;
using Microsoft.AspNetCore.Mvc;

namespace GEST.Api.Endpoints.Garage;

public static class RevenueEndpoints
{
    public static void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/revenue").WithTags("Revenue");

        group.MapPost("", async (
            [FromBody] RevenueRequestDto request,
            IRevenueAppService revenue,
            IValidator<RevenueRequestDto> validator,
            INotificationContext notificationContext,
            CancellationToken ct) =>
        {
            var val = await validator.ValidateAsync(request, ct);

            if (!val.IsValid)
                return Results.ValidationProblem(val.ToDictionary());

            var response = await revenue.GetRevenueAsync(request, ct);

            if (notificationContext.HasNotifications())
            {
                var dict = notificationContext.GetAll()
                    .GroupBy(n => n.Key)
                    .ToDictionary(g => g.Key, g => g.Select(n => n.Message).ToArray());
                return Results.ValidationProblem(dict, statusCode: 422, title: "Business rule violation");
            }

            return Results.Ok(new
            {
                amount = response.Amount,
                currency = response.Currency,
                timestamp = response.Timestamp
            });
        })
        .WithSummary("Consulta faturamento por setor/data (body)");
    }
}
