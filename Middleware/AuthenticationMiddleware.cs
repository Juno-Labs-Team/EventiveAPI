using System.Security.Claims;
using EventiveAPI.CSharp.Models;
using EventiveAPI.CSharp.Services;

namespace EventiveAPI.CSharp.Middleware;

public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthenticationMiddleware> _logger;

    public AuthenticationMiddleware(RequestDelegate next, ILogger<AuthenticationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, SupabaseService supabaseService)
    {
        // Skip authentication for health check and docs
        var path = context.Request.Path.Value ?? "";
        if (path == "/health" || path.StartsWith("/swagger") || path.StartsWith("/docs"))
        {
            await _next(context);
            return;
        }

        // Check for Authorization header
        if (!context.Request.Headers.TryGetValue("Authorization", out var authHeader) ||
            string.IsNullOrWhiteSpace(authHeader))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                error = new { message = "No authorization token provided" }
            });
            return;
        }

        var token = authHeader.ToString();
        if (!token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                error = new { message = "Invalid authorization header format" }
            });
            return;
        }

        token = token.Substring(7); // Remove 'Bearer ' prefix

        try
        {
            // Verify token with Supabase
            var user = await supabaseService.GetUserFromToken(token);
            
            if (user == null)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new
                {
                    success = false,
                    error = new { message = "Invalid or expired token" }
                });
                return;
            }

            // Fetch user profile to get role
            var client = supabaseService.GetClient();
            var profileResponse = await client
                .From<UserProfile>()
                .Where(p => p.Id == user.Id)
                .Single();

            var role = profileResponse?.Role ?? "user";

            // Add user claims to context
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id ?? ""),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Role, role)
            };

            var identity = new ClaimsIdentity(claims, "SupabaseAuth");
            context.User = new ClaimsPrincipal(identity);

            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Authentication error");
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                error = new { message = "Authentication failed" }
            });
        }
    }
}

public static class AuthenticationMiddlewareExtensions
{
    public static IApplicationBuilder UseSupabaseAuthentication(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<AuthenticationMiddleware>();
    }
}
