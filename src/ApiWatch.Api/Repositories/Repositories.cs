using ApiWatch.Core.Data;
using ApiWatch.Core.Entities;
using ApiWatch.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ApiWatch.Api.Repositories;

public class EndpointRepository : IEndpointRepository
{
    private readonly AppDbContext _db;
    public EndpointRepository(AppDbContext db) => _db = db;

    public async Task<IEnumerable<MonitoredEndpoint>> GetAllActiveAsync(CancellationToken ct = default)
        => await _db.MonitoredEndpoints.Where(e => e.IsActive).ToListAsync(ct);

    public async Task<MonitoredEndpoint?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.MonitoredEndpoints.FindAsync([id], ct);

    public async Task<MonitoredEndpoint> CreateAsync(MonitoredEndpoint endpoint, CancellationToken ct = default)
    {
        _db.MonitoredEndpoints.Add(endpoint);
        await _db.SaveChangesAsync(ct);
        return endpoint;
    }

    public async Task<MonitoredEndpoint> UpdateAsync(MonitoredEndpoint endpoint, CancellationToken ct = default)
    {
        _db.MonitoredEndpoints.Update(endpoint);
        await _db.SaveChangesAsync(ct);
        return endpoint;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var endpoint = await GetByIdAsync(id, ct);
        if (endpoint is not null)
        {
            _db.MonitoredEndpoints.Remove(endpoint);
            await _db.SaveChangesAsync(ct);
        }
    }
}

public class SubscriptionRepository : ISubscriptionRepository
{
    private readonly AppDbContext _db;
    public SubscriptionRepository(AppDbContext db) => _db = db;

    public async Task<Subscription?> GetActiveByUserIdAsync(Guid userId, CancellationToken ct = default)
        => await _db.Subscriptions
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Status == "active", ct);

    public async Task<Subscription> CreateAsync(Subscription subscription, CancellationToken ct = default)
    {
        _db.Subscriptions.Add(subscription);
        await _db.SaveChangesAsync(ct);
        return subscription;
    }

    public async Task<Subscription> UpdateAsync(Subscription subscription, CancellationToken ct = default)
    {
        _db.Subscriptions.Update(subscription);
        await _db.SaveChangesAsync(ct);
        return subscription;
    }
}

public class CheckResultRepository : ICheckResultRepository
{
    private readonly AppDbContext _db;
    public CheckResultRepository(AppDbContext db) => _db = db;

    public async Task<CheckResult> SaveAsync(CheckResult result, CancellationToken ct = default)
    {
        _db.CheckResults.Add(result);
        await _db.SaveChangesAsync(ct);
        return result;
    }

    public async Task<IEnumerable<CheckResult>> GetByEndpointAsync(Guid endpointId, int limit = 100, CancellationToken ct = default)
        => await _db.CheckResults
            .Where(r => r.MonitoredEndpointId == endpointId)
            .OrderByDescending(r => r.CheckedAt)
            .Take(limit)
            .ToListAsync(ct);

    public async Task<CheckResult?> GetLastByEndpointAsync(Guid endpointId, CancellationToken ct = default)
        => await _db.CheckResults
            .Where(r => r.MonitoredEndpointId == endpointId)
            .OrderByDescending(r => r.CheckedAt)
            .FirstOrDefaultAsync(ct);

    public async Task<double> GetUptimePercentageAsync(Guid endpointId, TimeSpan period, CancellationToken ct = default)
    {
        var since = DateTime.UtcNow - period;
        var results = await _db.CheckResults
            .Where(r => r.MonitoredEndpointId == endpointId && r.CheckedAt >= since)
            .ToListAsync(ct);

        if (!results.Any()) return 100.0;
        return results.Count(r => r.IsUp) / (double)results.Count * 100;
    }

    public async Task<double> GetAverageLatencyAsync(Guid endpointId, TimeSpan period, CancellationToken ct = default)
    {
        var since = DateTime.UtcNow - period;
        var results = await _db.CheckResults
            .Where(r => r.MonitoredEndpointId == endpointId && r.CheckedAt >= since && r.IsUp)
            .ToListAsync(ct);

        return results.Any() ? results.Average(r => r.LatencyMs) : 0;
    }
}
