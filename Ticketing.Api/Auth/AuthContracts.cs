using System.ComponentModel.DataAnnotations;
using Ticketing.Api.Users;

namespace Ticketing.Api.Auth;

public sealed record LoginRequest(
    [Required] string Username,
    [Required] string Password);

public sealed record LoginResponse(
    string AccessToken,
    string TokenType,
    DateTimeOffset ExpiresAt,
    string Username,
    Role Role);
