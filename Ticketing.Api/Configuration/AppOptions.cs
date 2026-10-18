using System.ComponentModel.DataAnnotations;

namespace Ticketing.Api.Configuration;

public sealed class JwtOptions
{
    public const string Section = "Jwt";

    [Required] public string Issuer { get; init; } = "";
    [Required] public string Audience { get; init; } = "";

    [Required(AllowEmptyStrings = false)] public string Secret { get; init; } = "";

    public TimeSpan Ttl { get; init; } = TimeSpan.FromMinutes(15);
}

public sealed class TicketOptions
{
    public const string Section = "Ticket";

    public TimeSpan ExpireAfter { get; init; } = TimeSpan.FromSeconds(30);
    public TimeSpan PurgeAfter { get; init; } = TimeSpan.FromMinutes(3);
    public TimeSpan ScanInterval { get; init; } = TimeSpan.FromSeconds(1);
}
