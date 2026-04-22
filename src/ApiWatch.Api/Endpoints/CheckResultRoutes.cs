using System.Security.Claims;
using ApiWatch.Api.DTOs;
using ApiWatch.Core.Interfaces;

namespace ApiWatch.Api.Endpoints;

public static class CheckResultRoutes
{
    public static void MapCheckResultRoutes(this WebApplication app)
    {
        var group = app.MapGroup("/api/endpoints/{endpointId:guid}/checks")
                       .WithTags("Checks")
                       .RequireAuthorization();

        group.MapGet("/", async (Guid endpointId, ClaimsPrincipal user, IEndpointRepository endpointRepo, ICheckResultRepository repo, int limit = 100, CancellationToken ct = default) =>
        {
            if (limit < 1 || limit > 1000)
                return Results.BadRequest(new { error = "limit must be between 1 and 1000." });

            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var endpoint = await endpointRepo.GetByIdAsync(endpointId, ct);
            if (endpoint is null || endpoint.UserId != userId) return Results.NotFound();

            var results = await repo.GetByEndpointAsync(endpointId, limit, ct);
            var response = results.Select(r => new CheckResultResponse(
                r.Id, r.IsUp, r.StatusCode, r.LatencyMs, r.ErrorMessage, r.CheckedAt
            ));
            return Results.Ok(response);
        }).RequireRateLimiting("api");

        group.MapGet("/uptime", async (Guid endpointId, ClaimsPrincipal user, IEndpointRepository endpointRepo, ICheckResultRepository repo, int days = 30, CancellationToken ct = default) =>
        {
            if (days < 1 || days > 365)
                return Results.BadRequest(new { error = "days must be between 1 and 365." });

            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var endpoint = await endpointRepo.GetByIdAsync(endpointId, ct);
            if (endpoint is null || endpoint.UserId != userId) return Results.NotFound();

            var period = TimeSpan.FromDays(days);
            var uptime = await repo.GetUptimePercentageAsync(endpointId, period, ct);
            var latency = await repo.GetAverageLatencyAsync(endpointId, period, ct);
            return Results.Ok(new { uptimePercentage = uptime, averageLatencyMs = latency, periodDays = days });
        }).RequireRateLimiting("api");
    }
}
