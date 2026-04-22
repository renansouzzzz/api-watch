using System.Security.Claims;
using ApiWatch.Api.DTOs;
using ApiWatch.Api.Services;
using ApiWatch.Core.Data;
using ApiWatch.Core.Interfaces;

namespace ApiWatch.Api.Endpoints;

public static class BillingRoutes
{
    public static void MapBillingRoutes(this WebApplication app)
    {
        var group = app.MapGroup("/api/billing").WithTags("Billing");

        group.MapGet("/plans", async (BillingService billing, CancellationToken ct) =>
        {
            var plans = await billing.GetPlansAsync(ct);
            var response = plans.Select(p => new PlanResponse(
                p.Id, p.Name, p.PriceMonthly, p.MaxEndpoints,
                p.MinIntervalSeconds, p.HistoryDays, p.HasEmailAlerts,
                p.HasWebhooks, p.HasStatusPage, p.MaxTeamMembers
            ));
            return Results.Ok(response);
        });

        group.MapGet("/subscription", async (ClaimsPrincipal user, BillingService billing, CancellationToken ct) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var sub = await billing.GetCurrentSubscriptionAsync(userId, ct);
            if (sub is null) return Results.NotFound(new { error = "No active subscription found." });

            return Results.Ok(ToResponse(sub));
        }).RequireAuthorization();

        group.MapPost("/subscribe", async (SubscribeRequest req, ClaimsPrincipal user, BillingService billing, AppDbContext db, CancellationToken ct) =>
        {
            var plan = await db.Plans.FindAsync([req.PlanId], ct);
            if (plan is null) return Results.BadRequest(new { error = "Invalid plan." });

            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var sub = await billing.SubscribeAsync(userId, req.PlanId, ct);

            await db.Entry(sub).Reference(s => s.Plan).LoadAsync(ct);

            return Results.Ok(ToResponse(sub));
        }).RequireAuthorization();

        group.MapPost("/cancel", async (ClaimsPrincipal user, BillingService billing, ISubscriptionRepository subscriptions, CancellationToken ct) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var sub = await subscriptions.GetActiveByUserIdAsync(userId, ct);
            if (sub is null) return Results.NotFound(new { error = "No active subscription found." });

            if (sub.PlanId == 1)
                return Results.BadRequest(new { error = "Already on the Free plan." });

            await billing.SubscribeAsync(userId, 1, ct);
            return Results.Ok(new { message = "Subscription canceled. Downgraded to Free plan." });
        }).RequireAuthorization();
    }

    private static SubscriptionResponse ToResponse(ApiWatch.Core.Entities.Subscription sub) =>
        new(sub.Id, sub.Status, sub.Plan.Name, sub.PlanId,
            sub.Plan.PriceMonthly, sub.Plan.MaxEndpoints,
            sub.Plan.MinIntervalSeconds, sub.StartedAt, sub.CurrentPeriodEnd);
}
