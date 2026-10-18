using System.ComponentModel.DataAnnotations;

namespace Ticketing.Api.Tickets;

public sealed record CreateTicketRequest(
    [Required]
    [StringLength(TicketTextRules.MaxLength, ErrorMessage = TicketTextRules.LengthMessage)]
    [RegularExpression(TicketTextRules.LettersAndSpaces,
        ErrorMessage = TicketTextRules.ViolationMessage)]
    string Text);

public sealed record UpdateTicketTextRequest(
    [Required]
    [StringLength(TicketTextRules.MaxLength, ErrorMessage = TicketTextRules.LengthMessage)]
    [RegularExpression(TicketTextRules.LettersAndSpaces,
        ErrorMessage = TicketTextRules.ViolationMessage)]
    string Text);

public sealed record UpdateTicketStatusRequest(
    [Required] TicketStatus Status);

public sealed record TicketResponse(
    long Id,
    string Text,
    TicketStatus Status,
    string CreatedByUsername,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt,
    DateTimeOffset StatusChangedAt)
{
    public static TicketResponse From(Ticket ticket) => new(
        ticket.Id,
        ticket.Body,
        ticket.Status,
        ticket.CreatedBy.Username,
        ticket.CreatedAt,
        ticket.ExpiresAt,
        ticket.StatusChangedAt);
}
