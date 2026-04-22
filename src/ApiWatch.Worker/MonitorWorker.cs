using ApiWatch.Core.Entities;
using ApiWatch.Core.Interfaces;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace ApiWatch.Worker;

public class MonitorWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<MonitorWorker> _logger;

    // Tracks when each endpoint was last checked
    private readonly ConcurrentDictionary<Guid, DateTime> _lastChecked = new();

    public MonitorWorker(
        IServiceScopeFactory scopeFactory,
        IHttpClientFactory httpClientFactory,
        ILogger<MonitorWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("MonitorWorker iniciado em {Time}", DateTimeOffset.UtcNow);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunCheckCycleAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no ciclo de monitoramento");
            }

            // Tick every 10 seconds — checks which endpoints are due for a ping
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }

    private async Task RunCheckCycleAsync(CancellationToken ct)
    {
        IEnumerable<MonitoredEndpoint> endpoints;
        using (var scope = _scopeFactory.CreateScope())
        {
            var endpointRepo = scope.ServiceProvider.GetRequiredService<IEndpointRepository>();
            endpoints = await endpointRepo.GetAllActiveAsync(ct);
        }

        var now = DateTime.UtcNow;
        var tasks = endpoints
            .Where(ep => ShouldCheck(ep, now))
            .Select(ep => CheckEndpointAsync(ep, ct));

        await Task.WhenAll(tasks);
    }

    private bool ShouldCheck(MonitoredEndpoint endpoint, DateTime now)
    {
        if (!_lastChecked.TryGetValue(endpoint.Id, out var lastCheck))
            return true;

        return (now - lastCheck).TotalSeconds >= endpoint.IntervalSeconds;
    }

    private async Task CheckEndpointAsync(
        MonitoredEndpoint endpoint,
        CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient("monitor");
        client.Timeout = TimeSpan.FromSeconds(endpoint.TimeoutSeconds);

        var stopwatch = Stopwatch.StartNew();
        CheckResult result;

        try
        {
            var response = await client.GetAsync(endpoint.Url, ct);
            stopwatch.Stop();

            result = new CheckResult
            {
                MonitoredEndpointId = endpoint.Id,
                IsUp = response.IsSuccessStatusCode,
                StatusCode = (int)response.StatusCode,
                LatencyMs = stopwatch.Elapsed.TotalMilliseconds,
                CheckedAt = DateTime.UtcNow
            };

            _logger.LogInformation(
                "[{Name}] {Url} → {StatusCode} em {Latency:F0}ms | IsUp: {IsUp}",
                endpoint.Name, endpoint.Url, (int)response.StatusCode,
                result.LatencyMs, result.IsUp);
        }
        catch (TaskCanceledException)
        {
            stopwatch.Stop();
            result = new CheckResult
            {
                MonitoredEndpointId = endpoint.Id,
                IsUp = false,
                LatencyMs = stopwatch.Elapsed.TotalMilliseconds,
                ErrorMessage = $"Timeout após {endpoint.TimeoutSeconds}s",
                CheckedAt = DateTime.UtcNow
            };

            _logger.LogWarning("[{Name}] TIMEOUT após {Timeout}s", endpoint.Name, endpoint.TimeoutSeconds);
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            result = new CheckResult
            {
                MonitoredEndpointId = endpoint.Id,
                IsUp = false,
                LatencyMs = stopwatch.Elapsed.TotalMilliseconds,
                ErrorMessage = ex.Message,
                CheckedAt = DateTime.UtcNow
            };

            _logger.LogWarning("[{Name}] FALHA: {Error}", endpoint.Name, ex.Message);
        }

        using (var scope = _scopeFactory.CreateScope())
        {
            var checkRepo = scope.ServiceProvider.GetRequiredService<ICheckResultRepository>();
            await checkRepo.SaveAsync(result, ct);
        }

        _lastChecked[endpoint.Id] = DateTime.UtcNow;
    }
}
