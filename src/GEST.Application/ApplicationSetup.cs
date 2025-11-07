using FluentValidation;
using GEST.Application.Abstractions;
using GEST.Application.Mapping;
using GEST.Application.Services.Garage;
using GEST.Application.Services.Parking;
using GEST.Application.Services.Revenue;
using GEST.Application.Services.Time;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GEST.Application;

public static class ApplicationSetup
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration config)
    {
        // AutoMapper
        services.AddAutoMapper(cfg => { }, typeof(ApplicationProfile).Assembly);

        // Validators
        services.AddValidatorsFromAssembly(typeof(ApplicationSetup).Assembly);

        // Services
        services.AddScoped<IGarageSyncAppService, GarageSyncAppService>();
        services.AddScoped<IParkingAppService, ParkingAppService>();
        services.AddScoped<IRevenueAppService, RevenueAppService>();
        services.AddSingleton<ITimeProvider, SystemTimeProvider>();

        return services;
    }
}
