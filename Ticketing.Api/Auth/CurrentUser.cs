using System.Security.Claims;
using Ticketing.Api.Users;

namespace Ticketing.Api.Auth;

public static class CurrentUser
{
    public const string UserIdClaim = "uid";
    public const string RoleClaim = "role";
    public const string UsernameClaim = "sub";

    public static long Id(this ClaimsPrincipal principal) =>
        long.Parse(principal.FindFirstValue(UserIdClaim)
                   ?? throw new InvalidOperationException("Token carries no uid claim"));

    public static string Username(this ClaimsPrincipal principal) =>
        principal.FindFirstValue(UsernameClaim)
        ?? throw new InvalidOperationException("Token carries no sub claim");

    public static Role Role(this ClaimsPrincipal principal) =>
        Enum.Parse<Role>(principal.FindFirstValue(RoleClaim)
                         ?? throw new InvalidOperationException("Token carries no role claim"));
}
