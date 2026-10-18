using Ticketing.Api.Common;
using Ticketing.Api.Users;

namespace Ticketing.Api.Tickets;

public class Ticket : ISoftDelete
{
    public long Id { get; private set; }

    public string Body { get; private set; } = null!;

    public TicketStatus Status { get; private set; }

    public long CreatedById { get; private set; }
    public AppUser CreatedBy { get; private set; } = null!;

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset StatusChangedAt { get; private set; }

    public DateTimeOffset? RemovedAt { get; set; }

    public long Version { get; private set; }

    private Ticket()
    {
    }

    public Ticket(string body, AppUser createdBy, DateTimeOffset createdAt, DateTimeOffset expiresAt)
    {
        Body = body;
        CreatedBy = createdBy;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
        Status = TicketStatus.Open;
        StatusChangedAt = createdAt;
    }

    public void Resolve(DateTimeOffset at)
    {
        RequireOpen(TicketStatus.Resolved);
        Status = TicketStatus.Resolved;
        StatusChangedAt = at;
        Version++;
    }

    public void Expire(DateTimeOffset at)
    {
        RequireOpen(TicketStatus.Expired);
        Status = TicketStatus.Expired;
        StatusChangedAt = at;
        Version++;
    }

    public void MarkRemoved(DateTimeOffset at)
    {
        if (Status != TicketStatus.Expired)
        {
            throw new InvalidOperationException(
                $"Only expired tickets may be removed; ticket {Id} is {Status}");
        }

        RemovedAt = at;
        Version++;
    }

    public void EditBody(string newBody)
    {
        RequireOpen(TicketStatus.Open);
        Body = newBody;
        Version++;
    }

    private void RequireOpen(TicketStatus target)
    {
        if (Status != TicketStatus.Open)
        {
            throw new InvalidTicketTransitionException(Id, Status, target);
        }
    }
}
