namespace ApiWatch.Core.Entities;

public class Plan
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal PriceMonthly { get; set; }
    public int MaxEndpoints { get; set; }       // -1 = unlimited
    public int MinIntervalSeconds { get; set; }
    public int HistoryDays { get; set; }
    public bool HasEmailAlerts { get; set; }
    public bool HasWebhooks { get; set; }
    public bool HasStatusPage { get; set; }
    public int MaxTeamMembers { get; set; }

    // Navigation
    public ICollection<User> Users { get; set; } = new List<User>();
}
