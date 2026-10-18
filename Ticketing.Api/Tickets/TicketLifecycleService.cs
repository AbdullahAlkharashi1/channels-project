using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Ticketing.Api.Audit;
using Ticketing.Api.Configuration;
using Ticketing.Api.Data;

namespace Ticketing.Api.Tickets;

public sealed class TicketLifecycleService(
    TicketingDbContext db,
    AuditService auditService,
    IOptions<TicketOptions> options,
    TimeProvider timeProvider)
{
    private const int BatchSize = 200;

    public async Task<int> ExpireDueTicketsAsync(CancellationToken ct)
    {
        var now = timeProvider.GetUtcNow();

        var due = await db.Tickets
            .Where(t => t.Status == TicketStatus.Open && t.ExpiresAt <= now)
            .OrderBy(t => t.ExpiresAt)
            .Take(BatchSize)
            .ToListAsync(ct);

        foreach (var ticket in due)
        {
            ticket.Expire(now);

            auditService.RecordTicketEvent(ticket.Id, ticket.Body, TicketEvent.Expired,
                TicketStatus.Open, TicketStatus.Expired, null, null);
        }

        if (due.Count > 0)
        {
            await db.SaveChangesAsync(ct);
        }

        return due.Count;
    }

    public async Task<int> RemoveExpiredTicketsAsync(CancellationToken ct)
    {
        var now = timeProvider.GetUtcNow();
        var cutoff = now - options.Value.PurgeAfter;

        var due = await db.Tickets
            .Where(t => t.Status == TicketStatus.Expired && t.StatusChangedAt <= cutoff)
            .OrderBy(t => t.StatusChangedAt)
            .Take(BatchSize)
            .ToListAsync(ct);

        foreach (var ticket in due)
        {
            auditService.RecordTicketEvent(ticket.Id, ticket.Body, TicketEvent.Removed,
                TicketStatus.Expired, null, null, null);

            ticket.MarkRemoved(now);
        }

        if (due.Count > 0)
        {
            await db.SaveChangesAsync(ct);
        }

        return due.Count;
    }
}
