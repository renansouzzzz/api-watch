namespace ApiWatch.Core.Entities;

public class Subscription
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public int PlanId { get; set; }
    public string Status { get; set; } = "active"; // active, trialing, canceled, past_due
    public string? StripeSubscriptionId { get; set; }
    public string? StripeCustomerId { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CanceledAt { get; set; }
    public DateTime? CurrentPeriodEnd { get; set; }

    public User User { get; set; } = null!;
    public Plan Plan { get; set; } = null!;
}
