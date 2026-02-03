using Microsoft.EntityFrameworkCore;

namespace MediaRenamer.Api.Data;

public static class ProposalDbContextExtensions
{
    public static async Task EnsureMigratedAsync(this ProposalDbContext db, ILogger logger,
        CancellationToken ct = default)
    {
        var pendingMigrations = await db.Database.GetPendingMigrationsAsync(ct);
        var migrations = pendingMigrations as string[] ?? pendingMigrations.ToArray();
        if (migrations.Any())
        {
            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("Applying {Count} pending migrations...", migrations.Count());
            await db.Database.MigrateAsync(ct);
            if (logger.IsEnabled(LogLevel.Debug)) logger.LogDebug("Database migrations completed.");
        }
        else
        {
            if (logger.IsEnabled(LogLevel.Debug)) logger.LogDebug("Database is up to date.");
        }
    }
}