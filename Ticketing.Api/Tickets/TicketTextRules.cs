namespace Ticketing.Api.Tickets;

public static class TicketTextRules
{
    public const int MaxLength = 250;

    public const string LettersAndSpaces = @"^[\p{L} ]+$";

    public const string ViolationMessage =
        "Ticket text may contain letters and spaces only — no digits or symbols";

    public const string LengthMessage =
        "Ticket text must not exceed 250 characters";
}
