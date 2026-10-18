using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ticketing.Api.Auth;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(AuthService authService) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login(
        LoginRequest request, CancellationToken ct)
    {
        return Ok(await authService.LoginAsync(request, ClientIp(), UserAgent(), ct));
    }

    private string? ClientIp()
    {
        var forwarded = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        var ip = string.IsNullOrWhiteSpace(forwarded)
            ? HttpContext.Connection.RemoteIpAddress?.ToString()
            : forwarded.Split(',')[0].Trim();

        return Truncate(ip, 64);
    }

    private string? UserAgent() =>
        Truncate(Request.Headers.UserAgent.FirstOrDefault(), 256);

    private static string? Truncate(string? value, int max) =>
        value is null || value.Length <= max ? value : value[..max];
}
