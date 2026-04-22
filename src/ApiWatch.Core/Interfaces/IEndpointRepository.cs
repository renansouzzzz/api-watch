using ApiWatch.Core.Entities;

namespace ApiWatch.Core.Interfaces;

public interface IEndpointRepository
{
    Task<IEnumerable<MonitoredEndpoint>> GetAllActiveAsync(CancellationToken ct = default);
    Task<IEnumerable<MonitoredEndpoint>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<MonitoredEndpoint?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<MonitoredEndpoint> CreateAsync(MonitoredEndpoint endpoint, CancellationToken ct = default);
    Task<MonitoredEndpoint> UpdateAsync(MonitoredEndpoint endpoint, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
