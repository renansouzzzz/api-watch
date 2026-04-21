namespace ApiWatch.Core.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public int PlanId { get; set; }
    public string? StripeCustomerId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Plan Plan { get; set; } = null!;
    public ICollection<MonitoredEndpoint> Endpoints { get; set; } = new List<MonitoredEndpoint>();
}
