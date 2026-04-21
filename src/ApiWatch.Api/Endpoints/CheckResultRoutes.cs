using ApiWatch.Api.DTOs;
using ApiWatch.Core.Interfaces;

namespace ApiWatch.Api.Endpoints;

public static class CheckResultRoutes
{
    public static void MapCheckResultRoutes(this WebApplication app)
    {
        var group = app.MapGroup("/api/endpoints/{endpointId:guid}/checks").WithTags("Checks");

        group.MapGet("/", async (Guid endpointId, ICheckResultRepository repo, int limit = 100) =>
        {
            var results = await repo.GetByEndpointAsync(endpointId, limit);
            var response = results.Select(r => new CheckResultResponse(
                r.Id, r.IsUp, r.StatusCode, r.LatencyMs, r.ErrorMessage, r.CheckedAt
            ));
            return Results.Ok(response);
        });

        group.MapGet("/uptime", async (Guid endpointId, ICheckResultRepository repo, int days = 30) =>
        {
            var period = TimeSpan.FromDays(days);
            var uptime = await repo.GetUptimePercentageAsync(endpointId, period);
            var latency = await repo.GetAverageLatencyAsync(endpointId, period);
            return Results.Ok(new { uptimePercentage = uptime, averageLatencyMs = latency, periodDays = days });
        });
    }
}
