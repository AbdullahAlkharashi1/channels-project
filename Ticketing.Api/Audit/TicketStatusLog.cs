using Ticketing.Api.Common;
using Ticketing.Api.Tickets;

namespace Ticketing.Api.Audit;

public class TicketStatusLog : IAppendOnly
{
    public long Id { get; private set; }

    public long TicketId { get; private set; }

    public string TicketText { get; private set; } = null!;

    public TicketEvent Event { get; private set; }
    public TicketStatus? FromStatus { get; private set; }
    public TicketStatus? ToStatus { get; private set; }

    public long? ActorUserId { get; private set; }

    public string? ActorUsername { get; private set; }

    public DateTimeOffset OccurredAt { get; private set; }

    private TicketStatusLog()
    {
    }

    public TicketStatusLog(long ticketId, string ticketText, TicketEvent @event,
        TicketStatus? fromStatus, TicketStatus? toStatus,
        long? actorUserId, string? actorUsername, DateTimeOffset occurredAt)
    {
        TicketId = ticketId;
        TicketText = ticketText;
        Event = @event;
        FromStatus = fromStatus;
        ToStatus = toStatus;
        ActorUserId = actorUserId;
        ActorUsername = actorUsername;
        OccurredAt = occurredAt;
    }
}
