using ApiWatch.Api.Repositories;
using ApiWatch.Core.Interfaces;

namespace ApiWatch.Api.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IEndpointRepository, EndpointRepository>();
        services.AddScoped<ICheckResultRepository, CheckResultRepository>();
        return services;
    }
}
