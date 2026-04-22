using System.Security.Claims;
using ApiWatch.Api.DTOs;
using ApiWatch.Core.Interfaces;

namespace ApiWatch.Api.Endpoints;

public static class DashboardRoutes
{
    public static void MapDashboardRoutes(this WebApplication app)
    {
        app.MapGet("/api/dashboard/summary", async (
            ClaimsPrincipal user,
            IEndpointRepository endpointRepo,
            ICheckResultRepository checkRepo,
            CancellationToken ct) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var endpoints = await endpointRepo.GetByUserIdAsync(userId, ct);
            var endpointList = endpoints.ToList();

            var statuses = new List<EndpointStatusResponse>();
            foreach (var ep in endpointList)
            {
                var last      = await checkRepo.GetLastByEndpointAsync(ep.Id, ct);
                var uptime24h = await checkRepo.GetUptimePercentageAsync(ep.Id, TimeSpan.FromHours(24), ct);

                statuses.Add(new EndpointStatusResponse(
                    ep.Id, ep.Name, ep.Url,
                    last?.IsUp, last?.StatusCode,
                    last?.LatencyMs ?? 0,
                    uptime24h, last?.CheckedAt
                ));
            }

            var summary = new DashboardSummaryResponse(
                TotalEndpoints: statuses.Count,
                UpCount: statuses.Count(s => s.IsUp == true),
                DownCount: statuses.Count(s => s.IsUp == false),
                SlowCount: statuses.Count(s => s.IsUp == true && s.LastLatencyMs > 500),
                AverageUptimeLast30Days: statuses.Any() ? statuses.Average(s => s.UptimeLast24h) : 0,
                AverageLatencyMs: statuses.Any(s => s.LastLatencyMs > 0)
                    ? statuses.Where(s => s.LastLatencyMs > 0).Average(s => s.LastLatencyMs)
                    : 0,
                Endpoints: statuses
            );

            return Results.Ok(summary);
        }).WithTags("Dashboard").RequireAuthorization().RequireRateLimiting("api");
    }
}
