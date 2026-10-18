namespace Ticketing.Api.Auth;

public sealed class InvalidCredentialsException() : Exception("Invalid username or password");

public sealed class AccountDisabledException() : Exception("Account is disabled");
