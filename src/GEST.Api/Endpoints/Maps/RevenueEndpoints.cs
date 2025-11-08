using FluentValidation;
using GEST.Application.Dtos.Revenue;
using GEST.Application.Services.Revenue;
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
}
