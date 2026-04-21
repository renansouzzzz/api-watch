using ApiWatch.Core.Data;
using ApiWatch.Core.Interfaces;
using ApiWatch.Worker;
using ApiWatch.Worker.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var host = Host.CreateDefaultBuilder(args)
    .UseSerilog((ctx, lc) => lc
        .ReadFrom.Configuration(ctx.Configuration)
        .WriteTo.Console())
    .ConfigureServices((ctx, services) =>
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(ctx.Configuration.GetConnectionString("DefaultConnection")),
            ServiceLifetime.Scoped);

        services.AddHttpClient("monitor", client =>
        {
            client.DefaultRequestHeaders.Add("User-Agent", "ApiWatch-Monitor/1.0");
        });

        services.AddScoped<IEndpointRepository, WorkerEndpointRepository>();
        services.AddScoped<ICheckResultRepository, WorkerCheckResultRepository>();

        services.AddHostedService<MonitorWorker>();
    })
    .Build();

using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    Log.Information("Database migrations applied.");
}

await host.RunAsync();
