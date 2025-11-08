using GEST.Api.Endpoints.Garage;

namespace GEST.Api.Endpoints;

public static class ApiEndpoints
{
    public static IEndpointRouteBuilder MapGestEndpoints(this IEndpointRouteBuilder app)
    {
        GarageEndpoints.Map(app);
        RevenueEndpoints.Map(app);
        WebhookEndpoints.Map(app);
        return app;
    }
}
