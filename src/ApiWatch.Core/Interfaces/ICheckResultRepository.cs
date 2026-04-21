using ApiWatch.Core.Entities;

namespace ApiWatch.Core.Interfaces;

public interface ICheckResultRepository
{
    Task<CheckResult> SaveAsync(CheckResult result, CancellationToken ct = default);
    Task<IEnumerable<CheckResult>> GetByEndpointAsync(Guid endpointId, int limit = 100, CancellationToken ct = default);
    Task<CheckResult?> GetLastByEndpointAsync(Guid endpointId, CancellationToken ct = default);
    Task<double> GetUptimePercentageAsync(Guid endpointId, TimeSpan period, CancellationToken ct = default);
    Task<double> GetAverageLatencyAsync(Guid endpointId, TimeSpan period, CancellationToken ct = default);
}
