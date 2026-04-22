using ApiWatch.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApiWatch.Core.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<MonitoredEndpoint> MonitoredEndpoints => Set<MonitoredEndpoint>();
    public DbSet<CheckResult> CheckResults => Set<CheckResult>();
    public DbSet<Incident> Incidents => Set<Incident>();
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // MonitoredEndpoint configuration
        modelBuilder.Entity<MonitoredEndpoint>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.Property(x => x.Url).HasMaxLength(2048).IsRequired();
            e.HasIndex(x => x.Url);
            e.HasOne(x => x.User)
             .WithMany(x => x.Endpoints)
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Cascade)
             .IsRequired(false);
        });

        // CheckResult configuration
        modelBuilder.Entity<CheckResult>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.MonitoredEndpoint)
             .WithMany(x => x.CheckResults)
             .HasForeignKey(x => x.MonitoredEndpointId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => x.CheckedAt);
            e.HasIndex(x => x.MonitoredEndpointId);
        });

        // Incident configuration
        modelBuilder.Entity<Incident>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.MonitoredEndpoint)
             .WithMany()
             .HasForeignKey(x => x.MonitoredEndpointId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // Plan configuration
        modelBuilder.Entity<Plan>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(50).IsRequired();
            e.Property(x => x.PriceMonthly).HasPrecision(10, 2);
        });

        // User configuration
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Email).HasMaxLength(256).IsRequired();
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.HasOne(x => x.Plan)
             .WithMany(x => x.Users)
             .HasForeignKey(x => x.PlanId);
        });

        // Development seed data
        var seedEndpointId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        modelBuilder.Entity<MonitoredEndpoint>().HasData(
            new MonitoredEndpoint
            {
                Id = seedEndpointId,
                Name = "Google",
                Url = "https://www.google.com",
                IntervalSeconds = 60,
                TimeoutSeconds = 10,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        );

        // Subscription configuration
        modelBuilder.Entity<Subscription>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Status).HasMaxLength(20).IsRequired();
            e.Property(x => x.StripeSubscriptionId).HasMaxLength(100);
            e.Property(x => x.StripeCustomerId).HasMaxLength(100);
            e.HasOne(x => x.User)
             .WithMany()
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Plan)
             .WithMany()
             .HasForeignKey(x => x.PlanId);
            e.HasIndex(x => new { x.UserId, x.Status });
        });

        // Seed plans — IDs are fixed integers so they never change across migrations
        modelBuilder.Entity<Plan>().HasData(
            new Plan { Id = 1, Name = "Free",     PriceMonthly = 0m,   MaxEndpoints = 3,  MinIntervalSeconds = 300, HistoryDays = 7,   HasEmailAlerts = false, HasWebhooks = false, HasStatusPage = false, MaxTeamMembers = 1 },
            new Plan { Id = 2, Name = "Starter",  PriceMonthly = 12m,  MaxEndpoints = 15, MinIntervalSeconds = 60,  HistoryDays = 30,  HasEmailAlerts = true,  HasWebhooks = false, HasStatusPage = false, MaxTeamMembers = 1 },
            new Plan { Id = 3, Name = "Pro",      PriceMonthly = 39m,  MaxEndpoints = 50, MinIntervalSeconds = 30,  HistoryDays = 90,  HasEmailAlerts = true,  HasWebhooks = true,  HasStatusPage = true,  MaxTeamMembers = 3 },
            new Plan { Id = 4, Name = "Business", PriceMonthly = 89m,  MaxEndpoints = -1, MinIntervalSeconds = 30,  HistoryDays = 365, HasEmailAlerts = true,  HasWebhooks = true,  HasStatusPage = true,  MaxTeamMembers = 10 }
        );
    }
}
