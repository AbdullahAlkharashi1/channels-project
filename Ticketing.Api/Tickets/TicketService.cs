using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Ticketing.Api.Audit;
using Ticketing.Api.Auth;
using Ticketing.Api.Common;
using Ticketing.Api.Configuration;
using Ticketing.Api.Data;

namespace Ticketing.Api.Tickets;

public sealed class TicketService(
    TicketingDbContext db,
    AuditService auditService,
    IOptions<TicketOptions> options,
    TimeProvider timeProvider)
{
    public async Task<TicketResponse> CreateAsync(
        CreateTicketRequest request, ClaimsPrincipal actor, CancellationToken ct)
    {
        var creator = await db.Users.SingleOrDefaultAsync(u => u.Id == actor.Id(), ct)
                      ?? throw new InvalidOperationException(
                          $"Authenticated user {actor.Id()} no longer exists");

        var now = timeProvider.GetUtcNow();
        var ticket = new Ticket(request.Text, creator, now, now.Add(options.Value.ExpireAfter));

        db.Tickets.Add(ticket);

        await using var transaction = await db.Database.BeginTransactionAsync(ct);

        await db.SaveChangesAsync(ct);

        auditService.RecordTicketEvent(ticket.Id, ticket.Body, TicketEvent.Created,
            null, TicketStatus.Open, actor.Id(), actor.Username());
        await db.SaveChangesAsync(ct);

        await transaction.CommitAsync(ct);

        return TicketResponse.From(ticket);
    }

    public async Task<PageResponse<TicketResponse>> ListAsync(
        TicketStatus? status, int page, int size, CancellationToken ct)
    {
        var query = db.Tickets.Include(t => t.CreatedBy).AsNoTracking();

        if (status is not null)
        {
            query = query.Where(t => t.Status == status);
        }

        var total = await query.LongCountAsync(ct);
        var rows = await query
            .OrderByDescending(t => t.CreatedAt)
            .ThenByDescending(t => t.Id)
            .Skip(page * size)
            .Take(size)
            .ToListAsync(ct);

        var items = rows.Select(TicketResponse.From).ToList();
        return PageResponse<TicketResponse>.Of(items, page, size, total);
    }

    public async Task<TicketResponse> GetAsync(long id, CancellationToken ct)
    {
        var ticket = await db.Tickets
                         .Include(t => t.CreatedBy)
                         .AsNoTracking()
                         .SingleOrDefaultAsync(t => t.Id == id, ct)
                     ?? throw new TicketNotFoundException(id);

        return TicketResponse.From(ticket);
    }

    public async Task<TicketResponse> UpdateStatusAsync(
        long id, TicketStatus target, ClaimsPrincipal actor, CancellationToken ct)
    {
        var ticket = await LoadTrackedAsync(id, ct);

        if (target != TicketStatus.Resolved)
        {
            throw new InvalidTicketTransitionException(id, ticket.Status, target);
        }

        var previous = ticket.Status;
        ticket.Resolve(timeProvider.GetUtcNow());

        auditService.RecordTicketEvent(ticket.Id, ticket.Body, TicketEvent.Resolved,
            previous, TicketStatus.Resolved, actor.Id(), actor.Username());

        await db.SaveChangesAsync(ct);
        return TicketResponse.From(ticket);
    }

    public async Task<TicketResponse> UpdateTextAsync(
        long id, UpdateTicketTextRequest request, ClaimsPrincipal actor, CancellationToken ct)
    {
        var ticket = await LoadTrackedAsync(id, ct);

        ticket.EditBody(request.Text);

        auditService.RecordTicketEvent(ticket.Id, ticket.Body, TicketEvent.Edited,
            ticket.Status, ticket.Status, actor.Id(), actor.Username());

        await db.SaveChangesAsync(ct);
        return TicketResponse.From(ticket);
    }

    private async Task<Ticket> LoadTrackedAsync(long id, CancellationToken ct) =>
        await db.Tickets.Include(t => t.CreatedBy).SingleOrDefaultAsync(t => t.Id == id, ct)
        ?? throw new TicketNotFoundException(id);
}
