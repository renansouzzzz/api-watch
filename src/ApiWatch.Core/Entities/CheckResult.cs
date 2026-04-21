namespace ApiWatch.Core.Entities;

public class CheckResult
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MonitoredEndpointId { get; set; }
    public bool IsUp { get; set; }
    public int? StatusCode { get; set; }
    public double LatencyMs { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public MonitoredEndpoint MonitoredEndpoint { get; set; } = null!;
}
