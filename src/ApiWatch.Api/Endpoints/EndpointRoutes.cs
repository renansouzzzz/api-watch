using ApiWatch.Api.DTOs;
using ApiWatch.Core.Entities;
using ApiWatch.Core.Interfaces;

namespace ApiWatch.Api.Endpoints;

public static class EndpointRoutes
{
    public static void MapEndpointRoutes(this WebApplication app)
    {
        var group = app.MapGroup("/api/endpoints").WithTags("Endpoints");

        group.MapGet("/", async (IEndpointRepository repo) =>
        {
            var endpoints = await repo.GetAllActiveAsync();
            var response = endpoints.Select(e => new EndpointResponse(
                e.Id, e.Name, e.Url, e.IntervalSeconds, e.TimeoutSeconds, e.IsActive, e.CreatedAt
            ));
            return Results.Ok(response);
        });

        group.MapGet("/{id:guid}", async (Guid id, IEndpointRepository repo) =>
        {
            var endpoint = await repo.GetByIdAsync(id);
            if (endpoint is null) return Results.NotFound();

            return Results.Ok(new EndpointResponse(
                endpoint.Id, endpoint.Name, endpoint.Url,
                endpoint.IntervalSeconds, endpoint.TimeoutSeconds,
                endpoint.IsActive, endpoint.CreatedAt
            ));
        });

        group.MapPost("/", async (CreateEndpointRequest req, IEndpointRepository repo) =>
        {
            var endpoint = new MonitoredEndpoint
            {
                Name = req.Name,
                Url = req.Url,
                IntervalSeconds = req.IntervalSeconds,
                TimeoutSeconds = req.TimeoutSeconds
            };

            var created = await repo.CreateAsync(endpoint);
            var response = new EndpointResponse(
                created.Id, created.Name, created.Url,
                created.IntervalSeconds, created.TimeoutSeconds,
                created.IsActive, created.CreatedAt
            );

            return Results.Created($"/api/endpoints/{created.Id}", response);
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateEndpointRequest req, IEndpointRepository repo) =>
        {
            var endpoint = await repo.GetByIdAsync(id);
            if (endpoint is null) return Results.NotFound();

            endpoint.Name = req.Name;
            endpoint.Url = req.Url;
            endpoint.IntervalSeconds = req.IntervalSeconds;
            endpoint.TimeoutSeconds = req.TimeoutSeconds;
            endpoint.IsActive = req.IsActive;
            endpoint.UpdatedAt = DateTime.UtcNow;

            await repo.UpdateAsync(endpoint);
            return Results.NoContent();
        });

        group.MapDelete("/{id:guid}", async (Guid id, IEndpointRepository repo) =>
        {
            var endpoint = await repo.GetByIdAsync(id);
            if (endpoint is null) return Results.NotFound();

            await repo.DeleteAsync(id);
            return Results.NoContent();
        });
    }
}
