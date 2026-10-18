using Ticketing.Api.Data;
using Ticketing.Api.Tickets;

namespace Ticketing.Api.Audit;

public sealed class AuditService(TicketingDbContext db, TimeProvider timeProvider)
{
    public void RecordTicketEvent(long ticketId, string ticketText, TicketEvent @event,
        TicketStatus? fromStatus, TicketStatus? toStatus,
        long? actorUserId, string? actorUsername)
    {
        db.TicketStatusLogs.Add(new TicketStatusLog(
            ticketId, ticketText, @event, fromStatus, toStatus,
            actorUserId, actorUsername, timeProvider.GetUtcNow()));
    }

    public async Task RecordLoginAttemptAsync(string username, long? userId, LoginOutcome outcome,
        string? clientIp, string? userAgent, CancellationToken ct)
    {
        db.LoginHistory.Add(new LoginHistory(
            username, userId, outcome, clientIp, userAgent, timeProvider.GetUtcNow()));

        await db.SaveChangesAsync(ct);
    }
}
