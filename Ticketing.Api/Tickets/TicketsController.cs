using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ticketing.Api.Common;

namespace Ticketing.Api.Tickets;

[ApiController]
[Route("api/tickets")]
[Authorize]
public sealed class TicketsController(TicketService ticketService) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = nameof(Users.Role.Customer))]
    public async Task<ActionResult<TicketResponse>> Create(
        CreateTicketRequest request, CancellationToken ct)
    {
        var created = await ticketService.CreateAsync(request, User, ct);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpGet]
    [Authorize(Roles = nameof(Users.Role.Agent))]
    public Task<PageResponse<TicketResponse>> List(
        [FromQuery] TicketStatus? status,
        [FromQuery] int page = 0,
        [FromQuery] [Range(1, 200)] int size = 20,
        CancellationToken ct = default) =>
        ticketService.ListAsync(status, page, size, ct);

    [HttpGet("{id:long}")]
    [Authorize(Roles = nameof(Users.Role.Agent))]
    public Task<TicketResponse> Get(long id, CancellationToken ct) =>
        ticketService.GetAsync(id, ct);

    [HttpPatch("{id:long}/status")]
    [Authorize(Roles = nameof(Users.Role.Agent))]
    public Task<TicketResponse> UpdateStatus(
        long id, UpdateTicketStatusRequest request, CancellationToken ct) =>
        ticketService.UpdateStatusAsync(id, request.Status, User, ct);

    [HttpPatch("{id:long}")]
    [Authorize(Roles = nameof(Users.Role.Agent))]
    public Task<TicketResponse> UpdateText(
        long id, UpdateTicketTextRequest request, CancellationToken ct) =>
        ticketService.UpdateTextAsync(id, request, User, ct);
}
