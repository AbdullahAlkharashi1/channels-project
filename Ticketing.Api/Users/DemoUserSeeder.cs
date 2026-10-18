using Microsoft.EntityFrameworkCore;
using Ticketing.Api.Data;

namespace Ticketing.Api.Users;

public static class DemoUserSeeder
{
    public static async Task SeedAsync(IServiceProvider services, CancellationToken ct = default)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<TicketingDbContext>();
        var timeProvider = scope.ServiceProvider.GetRequiredService<TimeProvider>();
        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger(typeof(DemoUserSeeder));

        if (await db.Users.AnyAsync(ct))
        {
            return;
        }

        var now = timeProvider.GetUtcNow();
        db.Users.AddRange(
            new AppUser("customer", BCrypt.Net.BCrypt.HashPassword("customer123"), Role.Customer, now),
            new AppUser("agent", BCrypt.Net.BCrypt.HashPassword("agent123"), Role.Agent, now),
            new AppUser("monitor", BCrypt.Net.BCrypt.HashPassword("monitor123"), Role.Monitor, now));

        await db.SaveChangesAsync(ct);

        logger.LogWarning("Seeded demo users (customer/agent/monitor). " +
                          "Disable SeedDemoUsers outside development.");
    }
}
