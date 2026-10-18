namespace Ticketing.Api.Tickets;

public sealed class InvalidTicketTransitionException(long ticketId, TicketStatus from, TicketStatus to)
    : Exception($"Ticket {ticketId} cannot move from {from} to {to}");

public sealed class TicketNotFoundException(long ticketId)
    : Exception($"No ticket with id {ticketId}");
