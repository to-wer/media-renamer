using MediaRenamer.Api.Data;
using MediaRenamer.Api.Services;
using MediaRenamer.Core.Abstractions;
using MediaRenamer.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace MediaRenamer.Api.Background;

public class MediaWatcherService(
    ILogger<MediaWatcherService> logger,
    IMediaScanner scanner,
    MetadataResolver resolver,
    IRenameService renamer,
    IConfiguration config,
    IServiceScopeFactory scopeFactory)
    : BackgroundService
{
    private readonly string _watchPath = config["Media:WatchPath"] ?? "/media/incoming";
    private readonly TimeSpan _scanInterval = TimeSpan.FromSeconds(config.GetValue<int>("Media:ScanInterval"));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("MediaWatcherService started. Watching: {path}", _watchPath);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ProposalDbContext>();
                await db.Database.MigrateAsync(stoppingToken);

                var proposalStore = scope.ServiceProvider.GetRequiredService<ProposalStore>();

                // Scan for media files
                var files = Directory
                    .GetFiles(_watchPath, "*.*", SearchOption.AllDirectories)
                    .Where(f => f.EndsWith(".mkv") || f.EndsWith(".mp4"));

                var pendingFiles = await proposalStore.GetPending();

                foreach (var file in files)
                {
                    if (pendingFiles.Any(p => p.Source.OriginalPath == file))
                        continue;

                    // TODO: skip rejected files with same proposed name

                    if (logger.IsEnabled(LogLevel.Information))
                        logger.LogInformation("New file detected: {file}", file);

                    // Analyse file and resolve metadata
                    var mediaFile = await scanner.AnalyzeAsync(file);
                    var enriched = await resolver.ResolveAsync(mediaFile);

                    if (enriched == null)
                    {
                        if (logger.IsEnabled(LogLevel.Warning))
                            logger.LogWarning("Metadata could not be resolved for file: {file}", file);
                        continue;
                    }

                    // Create rename proposal
                    var proposal = renamer.CreateProposal(enriched);
                    await proposalStore.Add(proposal);

                    if (logger.IsEnabled(LogLevel.Information))
                        logger.LogInformation("Proposal created (awaiting approval): {proposal}",
                            proposal.ProposedName);
                }
            }
            catch (Exception ex)
            {
                if (logger.IsEnabled(LogLevel.Error)) logger.LogError(ex, "Error in MediaWatcherService");
            }

            // Wait before next scan
            await Task.Delay(_scanInterval, stoppingToken);
        }
    }
}