using ApiWatch.Core.Entities;

namespace ApiWatch.Core.Interfaces;

public interface ISubscriptionRepository
{
    Task<Subscription?> GetActiveByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<Subscription> CreateAsync(Subscription subscription, CancellationToken ct = default);
    Task<Subscription> UpdateAsync(Subscription subscription, CancellationToken ct = default);
}
