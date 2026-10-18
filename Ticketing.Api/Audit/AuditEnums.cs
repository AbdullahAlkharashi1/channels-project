namespace Ticketing.Api.Audit;

public enum TicketEvent
{
    Created,
    Edited,
    Resolved,
    Expired,
    Removed
}

public enum LoginOutcome
{
    Success,
    BadCredentials,
    UnknownUser,
    AccountDisabled
}
