using MediaRenamer.Core.Abstractions;
using MediaRenamer.Core.Models;
using MediaRenamer.Core.Services;
using Microsoft.Extensions.Options;

namespace MediaRenamer.Api.Background;

public class MediaWatcherService(
    ILogger<MediaWatcherService> logger,
    IMediaScanner scanner,
    MetadataResolver resolver,
    IRenameService renamer,
    IOptions<MediaSettings> mediaSettings,
    IServiceScopeFactory scopeFactory,
    IFileSystemService fileSystemService)
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
                var files = fileSystemService.GetFiles(_mediaSettings.WatchPath)
                    .Where(f => f.EndsWith(".mkv") || f.EndsWith(".mp4"));

                var pendingFiles = await proposalStore.GetPending();

                // Check if pending proposals still have their source files
                var idsToDelete = new List<Guid>();
                foreach (var pendingProposal in pendingFiles)
                {
                    if (fileSystemService.FileExists(pendingProposal.Source.OriginalPath)) continue;
                    if (logger.IsEnabled(LogLevel.Warning))
                    {
                        logger.LogWarning("File not found during scan, deleting proposal: {filePath}",
                            pendingProposal.Source.OriginalPath);
                    }
                    idsToDelete.Add(pendingProposal.Id);
                }

                // Batch delete proposals for missing files (more efficient than individual deletes)
                if (idsToDelete.Count > 0)
                {
                    await proposalStore.DeleteMany(idsToDelete);
                }

                // Refresh pending files after cleanup to ensure we have the current state
                // This is necessary because we just deleted some proposals and need to avoid
                // creating duplicate proposals for files that were already processed
                pendingFiles = await proposalStore.GetPending();

                foreach (var file in files)
                {
                    if (pendingFiles.Any(p => p.Source.OriginalPath == file))
                    {
                        if (logger.IsEnabled(LogLevel.Debug))
                            logger.LogDebug("File already has a pending proposal, skipping: {file}", file);
                        continue;
                    }

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