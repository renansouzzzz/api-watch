using System.Security.Claims;
using ApiWatch.Api.DTOs;
using ApiWatch.Core.Data;
using ApiWatch.Core.Entities;
using ApiWatch.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

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

        group.MapPost("/", async (CreateEndpointRequest req, ClaimsPrincipal user, AppDbContext db, IEndpointRepository repo, CancellationToken ct) =>
        {
            Guid? userId = null;

            if (user.Identity?.IsAuthenticated == true)
            {
                userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

                var sub = await db.Subscriptions
                    .Include(s => s.Plan)
                    .FirstOrDefaultAsync(s => s.UserId == userId && s.Status == "active", ct);

                if (sub is not null)
                {
                    if (sub.Plan.MaxEndpoints != -1)
                    {
                        var count = await db.MonitoredEndpoints.CountAsync(e => e.UserId == userId, ct);
                        if (count >= sub.Plan.MaxEndpoints)
                            return Results.Problem(
                                title: "Plan limit reached",
                                detail: $"Your {sub.Plan.Name} plan allows up to {sub.Plan.MaxEndpoints} endpoints. Upgrade to add more.",
                                statusCode: 403
                            );
                    }

                    if (req.IntervalSeconds < sub.Plan.MinIntervalSeconds)
                        return Results.Problem(
                            title: "Interval too short",
                            detail: $"Your {sub.Plan.Name} plan requires a minimum check interval of {sub.Plan.MinIntervalSeconds}s.",
                            statusCode: 403
                        );
                }
            }

            var endpoint = new MonitoredEndpoint
            {
                UserId = userId,
                Name = req.Name,
                Url = req.Url,
                IntervalSeconds = req.IntervalSeconds,
                TimeoutSeconds = req.TimeoutSeconds
            };

            var created = await repo.CreateAsync(endpoint);
            return Results.Created($"/api/endpoints/{created.Id}", new EndpointResponse(
                created.Id, created.Name, created.Url,
                created.IntervalSeconds, created.TimeoutSeconds,
                created.IsActive, created.CreatedAt
            ));
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
