using ApiWatch.Api.DTOs;
using ApiWatch.Core.Interfaces;

namespace ApiWatch.Api.Endpoints;

public static class DashboardRoutes
{
    public static void MapDashboardRoutes(this WebApplication app)
    {
        app.MapGet("/api/dashboard/summary", async (
            IEndpointRepository endpointRepo,
            ICheckResultRepository checkRepo) =>
        {
            var endpoints = await endpointRepo.GetAllActiveAsync();
            var endpointList = endpoints.ToList();

            // Sequential processing — EF Core DbContext does not support
            // concurrent operations on the same instance (no Task.WhenAll here)
            var statuses = new List<EndpointStatusResponse>();
            foreach (var ep in endpointList)
            {
                var last      = await checkRepo.GetLastByEndpointAsync(ep.Id);
                var uptime24h = await checkRepo.GetUptimePercentageAsync(ep.Id, TimeSpan.FromHours(24));

                statuses.Add(new EndpointStatusResponse(
                    ep.Id,
                    ep.Name,
                    ep.Url,
                    last?.IsUp,
                    last?.StatusCode,
                    last?.LatencyMs ?? 0,
                    uptime24h,
                    last?.CheckedAt
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
        }).WithTags("Dashboard");
    }
}
