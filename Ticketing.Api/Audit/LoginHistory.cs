using Ticketing.Api.Common;

namespace Ticketing.Api.Audit;

public class LoginHistory : IAppendOnly
{
    public long Id { get; private set; }

    public string Username { get; private set; } = null!;

    public long? UserId { get; private set; }
    public LoginOutcome Outcome { get; private set; }
    public string? ClientIp { get; private set; }
    public string? UserAgent { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }

    private LoginHistory()
    {
    }

    public LoginHistory(string username, long? userId, LoginOutcome outcome,
        string? clientIp, string? userAgent, DateTimeOffset occurredAt)
    {
        Username = username;
        UserId = userId;
        Outcome = outcome;
        ClientIp = clientIp;
        UserAgent = userAgent;
        OccurredAt = occurredAt;
    }
}
