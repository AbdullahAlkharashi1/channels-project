using Ticketing.Api.Tickets;

namespace Ticketing.Api.Audit;

public sealed record TicketStatusLogResponse(
    long Id,
    long TicketId,
    string TicketText,
    TicketEvent Event,
    TicketStatus? FromStatus,
    TicketStatus? ToStatus,
    long? ActorUserId,
    string? ActorUsername,
    DateTimeOffset OccurredAt)
{
    public static TicketStatusLogResponse From(TicketStatusLog log) => new(
        log.Id, log.TicketId, log.TicketText, log.Event,
        log.FromStatus, log.ToStatus, log.ActorUserId, log.ActorUsername, log.OccurredAt);
}

public sealed record LoginHistoryResponse(
    long Id,
    string Username,
    long? UserId,
    LoginOutcome Outcome,
    string? ClientIp,
    string? UserAgent,
    DateTimeOffset OccurredAt)
{
    public static LoginHistoryResponse From(LoginHistory entry) => new(
        entry.Id, entry.Username, entry.UserId, entry.Outcome,
        entry.ClientIp, entry.UserAgent, entry.OccurredAt);
}
