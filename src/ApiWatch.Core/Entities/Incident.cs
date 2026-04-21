namespace ApiWatch.Core.Entities;

public class Incident
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MonitoredEndpointId { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
    public string Cause { get; set; } = string.Empty;
    public bool IsResolved => ResolvedAt.HasValue;

    // Navigation
    public MonitoredEndpoint MonitoredEndpoint { get; set; } = null!;
}
