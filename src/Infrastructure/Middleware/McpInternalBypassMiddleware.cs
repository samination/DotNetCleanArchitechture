using System.Security.Claims;
using System.Threading.Tasks;
using Infrastructure.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Infrastructure.Middleware;

public sealed class McpInternalBypassMiddleware
{
    private const string HeaderName = "X-Mcp-Internal-Key";

    private readonly RequestDelegate _next;
    private readonly McpInternalOptions _options;

    public McpInternalBypassMiddleware(RequestDelegate next, IOptions<McpInternalOptions> options)
    {
        _next = next;
        _options = options.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            if (!string.IsNullOrWhiteSpace(_options.SharedKey) &&
                context.Request.Headers.TryGetValue(HeaderName, out var providedKey) &&
                string.Equals(providedKey.ToString(), _options.SharedKey, StringComparison.Ordinal))
            {
                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, "mcp-server"),
                    new Claim(ClaimTypes.Name, "McpServer"),
                    new Claim("scope", "mcp.internal")
                };

                context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "McpInternalKey"));
            }
        }

        await _next(context);
    }
}

