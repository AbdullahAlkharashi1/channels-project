using Microsoft.EntityFrameworkCore;
using Ticketing.Api.Audit;
using Ticketing.Api.Data;

namespace Ticketing.Api.Auth;

public sealed class AuthService(
    TicketingDbContext db,
    JwtTokenService tokenService,
    AuditService auditService)
{
    private static readonly string DummyHash =
        BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString());

    public async Task<LoginResponse> LoginAsync(
        LoginRequest request, string? clientIp, string? userAgent, CancellationToken ct)
    {
        var user = await db.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.Username == request.Username, ct);

        if (user is null)
        {
            BCrypt.Net.BCrypt.Verify(request.Password, DummyHash);
            await auditService.RecordLoginAttemptAsync(
                request.Username, null, LoginOutcome.UnknownUser, clientIp, userAgent, ct);
            throw new InvalidCredentialsException();
        }

        if (!user.Enabled)
        {
            await auditService.RecordLoginAttemptAsync(
                user.Username, user.Id, LoginOutcome.AccountDisabled, clientIp, userAgent, ct);
            throw new AccountDisabledException();
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            await auditService.RecordLoginAttemptAsync(
                user.Username, user.Id, LoginOutcome.BadCredentials, clientIp, userAgent, ct);
            throw new InvalidCredentialsException();
        }

        await auditService.RecordLoginAttemptAsync(
            user.Username, user.Id, LoginOutcome.Success, clientIp, userAgent, ct);

        var (token, expiresAt) = tokenService.Issue(user);
        return new LoginResponse(token, "Bearer", expiresAt, user.Username, user.Role);
    }
}
