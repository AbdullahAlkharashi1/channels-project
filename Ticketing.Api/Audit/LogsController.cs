using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ticketing.Api.Common;
using Ticketing.Api.Data;
using Ticketing.Api.Users;

namespace Ticketing.Api.Audit;

[ApiController]
[Route("api/logs")]
[Authorize(Roles = nameof(Role.Monitor))]
public sealed class LogsController(TicketingDbContext db) : ControllerBase
{
    [HttpGet("ticket-status")]
    public async Task<PageResponse<TicketStatusLogResponse>> TicketStatusLog(
        [FromQuery] long? ticketId,
        [FromQuery] int page = 0,
        [FromQuery] [Range(1, 500)] int size = 50,
        CancellationToken ct = default)
    {
        var query = db.TicketStatusLogs.AsNoTracking();

        if (ticketId is not null)
        {
            query = query.Where(l => l.TicketId == ticketId)
                .OrderBy(l => l.OccurredAt).ThenBy(l => l.Id);
        }
        else
        {
            query = query.OrderByDescending(l => l.OccurredAt).ThenByDescending(l => l.Id);
        }

        var total = await query.LongCountAsync(ct);
        var rows = await query.Skip(page * size).Take(size).ToListAsync(ct);

        return PageResponse<TicketStatusLogResponse>.Of(
            rows.Select(TicketStatusLogResponse.From).ToList(), page, size, total);
    }

    [HttpGet("logins")]
    public async Task<PageResponse<LoginHistoryResponse>> LoginHistoryLog(
        [FromQuery] string? username,
        [FromQuery] int page = 0,
        [FromQuery] [Range(1, 500)] int size = 50,
        CancellationToken ct = default)
    {
        var query = db.LoginHistory.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(username))
        {
            query = query.Where(h => h.Username == username);
        }

        query = query.OrderByDescending(h => h.OccurredAt).ThenByDescending(h => h.Id);

        var total = await query.LongCountAsync(ct);
        var rows = await query.Skip(page * size).Take(size).ToListAsync(ct);

        return PageResponse<LoginHistoryResponse>.Of(
            rows.Select(LoginHistoryResponse.From).ToList(), page, size, total);
    }
}
