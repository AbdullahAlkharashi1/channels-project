using Microsoft.Extensions.Options;
using Ticketing.Api.Configuration;

namespace Ticketing.Api.Tickets;

public sealed class TicketLifecycleWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<TicketOptions> options,
    ILogger<TicketLifecycleWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(options.Value.ScanInterval);

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await SweepAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Lifecycle worker stopping");
        }
    }

    private async Task SweepAsync(CancellationToken stoppingToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var lifecycle = scope.ServiceProvider.GetRequiredService<TicketLifecycleService>();

        try
        {
            var expired = await lifecycle.ExpireDueTicketsAsync(stoppingToken);
            if (expired > 0)
            {
                logger.LogInformation("Expired {Count} ticket(s)", expired);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Expiry sweep failed; retrying next tick");
        }

        try
        {
            var removed = await lifecycle.RemoveExpiredTicketsAsync(stoppingToken);
            if (removed > 0)
            {
                logger.LogInformation("Removed {Count} expired ticket(s)", removed);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Removal sweep failed; retrying next tick");
        }
    }
}
