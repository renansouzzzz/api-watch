using ApiWatch.Api.Services;

namespace ApiWatch.Api.Endpoints;

public static class AuthRoutes
{
    public static void MapAuthRoutes(this WebApplication app)
    {
        var group = app.MapGroup("/auth").WithTags("Auth");

        group.MapPost("/register", async (RegisterRequest req, AuthService auth, CancellationToken ct) =>
        {
            var result = await auth.RegisterAsync(req, ct);
            return result is null
                ? Results.Conflict(new { error = "Email already in use." })
                : Results.Ok(result);
        }).RequireRateLimiting("auth");

        group.MapPost("/login", async (LoginRequest req, AuthService auth, CancellationToken ct) =>
        {
            var result = await auth.LoginAsync(req, ct);
            return result is null
                ? Results.Unauthorized()
                : Results.Ok(result);
        }).RequireRateLimiting("auth");
    }
}
