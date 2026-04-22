using ApiWatch.Core.Data;
using ApiWatch.Core.Entities;
using ApiWatch.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ApiWatch.Api.Services;

public class BillingService
{
    private readonly AppDbContext _db;
    private readonly ISubscriptionRepository _subscriptions;

    public BillingService(AppDbContext db, ISubscriptionRepository subscriptions)
    {
        _db = db;
        _subscriptions = subscriptions;
    }

    public async Task<IEnumerable<Plan>> GetPlansAsync(CancellationToken ct = default)
        => await _db.Plans.OrderBy(p => p.PriceMonthly).ToListAsync(ct);

    public async Task<Subscription?> GetCurrentSubscriptionAsync(Guid userId, CancellationToken ct = default)
        => await _subscriptions.GetActiveByUserIdAsync(userId, ct);

    public async Task<Subscription> SubscribeAsync(Guid userId, int planId, CancellationToken ct = default)
    {
        var existing = await _subscriptions.GetActiveByUserIdAsync(userId, ct);
        if (existing is not null)
        {
            existing.Status = "canceled";
            existing.CanceledAt = DateTime.UtcNow;
            await _subscriptions.UpdateAsync(existing, ct);
        }

        var user = await _db.Users.FindAsync([userId], ct);
        if (user is not null)
        {
            user.PlanId = planId;
            await _db.SaveChangesAsync(ct);
        }

        var subscription = new Subscription
        {
            UserId = userId,
            PlanId = planId,
            Status = "active",
            StartedAt = DateTime.UtcNow,
            CurrentPeriodEnd = DateTime.UtcNow.AddMonths(1)
        };

        return await _subscriptions.CreateAsync(subscription, ct);
    }
}
