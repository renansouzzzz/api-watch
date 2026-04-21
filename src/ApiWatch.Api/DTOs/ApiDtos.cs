namespace ApiWatch.Api.DTOs;

// ===== Endpoint DTOs =====

public record CreateEndpointRequest(
    string Name,
    string Url,
    int IntervalSeconds = 60,
    int TimeoutSeconds = 10
);

public record UpdateEndpointRequest(
    string Name,
    string Url,
    int IntervalSeconds,
    int TimeoutSeconds,
    bool IsActive
);

public record EndpointResponse(
    Guid Id,
    string Name,
    string Url,
    int IntervalSeconds,
    int TimeoutSeconds,
    bool IsActive,
    DateTime CreatedAt
);

// ===== CheckResult DTOs =====

public record CheckResultResponse(
    Guid Id,
    bool IsUp,
    int? StatusCode,
    double LatencyMs,
    string? ErrorMessage,
    DateTime CheckedAt
);

// ===== Dashboard DTOs =====

public record DashboardSummaryResponse(
    int TotalEndpoints,
    int UpCount,
    int DownCount,
    int SlowCount,
    double AverageUptimeLast30Days,
    double AverageLatencyMs,
    IEnumerable<EndpointStatusResponse> Endpoints
);

public record EndpointStatusResponse(
    Guid Id,
    string Name,
    string Url,
    bool? IsUp,
    int? LastStatusCode,
    double LastLatencyMs,
    double UptimeLast24h,
    DateTime? LastCheckedAt
);
