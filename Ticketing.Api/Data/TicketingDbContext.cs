using Microsoft.EntityFrameworkCore;
using Ticketing.Api.Audit;
using Ticketing.Api.Common;
using Ticketing.Api.Tickets;
using Ticketing.Api.Users;

namespace Ticketing.Api.Data;

public class TicketingDbContext(DbContextOptions<TicketingDbContext> options, TimeProvider timeProvider)
    : DbContext(options)
{
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<TicketStatusLog> TicketStatusLogs => Set<TicketStatusLog>();
    public DbSet<LoginHistory> LoginHistory => Set<LoginHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>(user =>
        {
            user.ToTable("app_user", t => t.HasCheckConstraint(
                "app_user_role_chk", "role in ('Agent', 'Customer', 'Monitor')"));
            user.HasKey(u => u.Id);
            user.Property(u => u.Username).HasMaxLength(64).IsRequired();
            user.HasIndex(u => u.Username).IsUnique();
            user.Property(u => u.PasswordHash).HasMaxLength(100).IsRequired();
            user.Property(u => u.Role).HasConversion<string>().HasMaxLength(16).IsRequired();
            user.Property(u => u.Enabled).IsRequired();
            user.Property(u => u.CreatedAt).IsRequired();
        });

        modelBuilder.Entity<Ticket>(ticket =>
        {
            ticket.ToTable("ticket", t => t.HasCheckConstraint(
                "ticket_status_chk", "status in ('Open', 'Resolved', 'Expired')"));
            ticket.HasKey(t => t.Id);
            ticket.Property(t => t.Body).HasMaxLength(TicketTextRules.MaxLength).IsRequired();
            ticket.Property(t => t.Status).HasConversion<string>().HasMaxLength(16).IsRequired();
            ticket.Property(t => t.CreatedAt).IsRequired();
            ticket.Property(t => t.ExpiresAt).IsRequired();
            ticket.Property(t => t.StatusChangedAt).IsRequired();

            ticket.HasOne(t => t.CreatedBy)
                .WithMany()
                .HasForeignKey(t => t.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            ticket.HasQueryFilter(t => t.RemovedAt == null);

            ticket.Property(t => t.Version).IsConcurrencyToken().IsRequired();

            ticket.HasIndex(t => new { t.Status, t.ExpiresAt });
            ticket.HasIndex(t => t.CreatedById);
        });

        modelBuilder.Entity<TicketStatusLog>(log =>
        {
            log.ToTable("ticket_status_log", t => t.HasCheckConstraint(
                "ticket_status_log_event_chk",
                "event in ('Created', 'Edited', 'Resolved', 'Expired', 'Removed')"));
            log.HasKey(l => l.Id);
            log.Property(l => l.TicketId).IsRequired();
            log.Property(l => l.TicketText).HasMaxLength(TicketTextRules.MaxLength).IsRequired();
            log.Property(l => l.Event).HasConversion<string>().HasMaxLength(16).IsRequired();
            log.Property(l => l.FromStatus).HasConversion<string>().HasMaxLength(16);
            log.Property(l => l.ToStatus).HasConversion<string>().HasMaxLength(16);
            log.Property(l => l.ActorUsername).HasMaxLength(64);
            log.Property(l => l.OccurredAt).IsRequired();

            log.HasOne<Ticket>()
                .WithMany()
                .HasForeignKey(l => l.TicketId)
                .OnDelete(DeleteBehavior.Restrict);

            log.HasIndex(l => new { l.TicketId, l.OccurredAt });
            log.HasIndex(l => l.OccurredAt);
        });

        modelBuilder.Entity<LoginHistory>(entry =>
        {
            entry.ToTable("login_history", t => t.HasCheckConstraint(
                "login_history_outcome_chk",
                "outcome in ('Success', 'BadCredentials', 'UnknownUser', 'AccountDisabled')"));
            entry.HasKey(h => h.Id);
            entry.Property(h => h.Username).HasMaxLength(64).IsRequired();
            entry.Property(h => h.Outcome).HasConversion<string>().HasMaxLength(24).IsRequired();
            entry.Property(h => h.ClientIp).HasMaxLength(64);
            entry.Property(h => h.UserAgent).HasMaxLength(256);
            entry.Property(h => h.OccurredAt).IsRequired();

            entry.HasIndex(h => new { h.Username, h.OccurredAt });
            entry.HasIndex(h => h.OccurredAt);
        });
    }

    public override int SaveChanges()
    {
        PrepareChanges();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        PrepareChanges();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void PrepareChanges()
    {
        ConvertDeletesToSoftDeletes();
        GuardAppendOnlyEntities();
    }

    private void ConvertDeletesToSoftDeletes()
    {
        var now = timeProvider.GetUtcNow();

        foreach (var entry in ChangeTracker.Entries<ISoftDelete>().ToList())
        {
            if (entry.State != EntityState.Deleted)
            {
                continue;
            }

            entry.State = EntityState.Unchanged;
            entry.Entity.RemovedAt = now;
            entry.Property(nameof(ISoftDelete.RemovedAt)).IsModified = true;
        }
    }

    private void GuardAppendOnlyEntities()
    {
        foreach (var entry in ChangeTracker.Entries<IAppendOnly>())
        {
            if (entry.State is EntityState.Modified or EntityState.Deleted)
            {
                throw new InvalidOperationException(
                    $"{entry.Entity.GetType().Name} is append-only; " +
                    $"{entry.State} is not permitted on audit data.");
            }
        }
    }
}
