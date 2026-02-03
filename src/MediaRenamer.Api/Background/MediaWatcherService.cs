using MediaRenamer.Api.Data;
using MediaRenamer.Api.Services;
using MediaRenamer.Core.Abstractions;
using MediaRenamer.Core.Models;
using MediaRenamer.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MediaRenamer.Api.Background;

public class MediaWatcherService(
    ILogger<MediaWatcherService> logger,
    IMediaScanner scanner,
    MetadataResolver resolver,
    IRenameService renamer,
    IOptions<MediaSettings> mediaSettings,
    IServiceScopeFactory scopeFactory)
    : BackgroundService
{
    private readonly MediaSettings _mediaSettings = mediaSettings.Value;
    private TimeSpan ScanInterval => TimeSpan.FromSeconds(_mediaSettings.ScanInterval);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("MediaWatcherService started. Watching: {path}", _mediaSettings.WatchPath);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();

                var proposalStore = scope.ServiceProvider.GetRequiredService<IProposalStore>();

                // Scan for media files
                var files = Directory
                    .GetFiles(_mediaSettings.WatchPath, "*.*", SearchOption.AllDirectories)
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
                        var errorProposal = new RenameProposal
                        {
                            Source = mediaFile,
                            ProposedName = string.Empty,
                            Status = ProposalStatus.Error
                        };
                        await proposalStore.Add(errorProposal);
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
            await Task.Delay(ScanInterval, stoppingToken);
        }
    }
}