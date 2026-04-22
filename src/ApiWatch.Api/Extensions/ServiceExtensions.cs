using ApiWatch.Api.Repositories;
using ApiWatch.Api.Services;
using ApiWatch.Core.Interfaces;

namespace ApiWatch.Api.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IEndpointRepository, EndpointRepository>();
        services.AddScoped<ICheckResultRepository, CheckResultRepository>();
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        return services;
    }

    public static IServiceCollection AddAppServices(this IServiceCollection services)
    {
        services.AddScoped<AuthService>();
        services.AddScoped<BillingService>();
        return services;
    }
}
