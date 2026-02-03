using MediaRenamer.Core.Abstractions;
using MediaRenamer.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MediaRenamer.Core.Services;

public class RenameService(
    IOptions<MediaSettings> settings,
    ILogger<RenameService> logger) : IRenameService
{
    private readonly MediaSettings _settings = settings.Value;

    public RenameProposal CreateProposal(MediaFile file)
    {
        var template = file.Type == MediaType.Episode
            ? _settings.TvSeriesTemplate
            : _settings.MovieTemplate;

        var proposedName = PathTemplateService.RenderTemplate(template, file);
        return new RenameProposal
        {
            Source = file,
            ProposedName = proposedName,
            ScanTime = DateTime.UtcNow,
            Status = ProposalStatus.Pending
        };
    }

    public Task ExecuteAsync(RenameProposal proposal)
    {
        try
        {
            var libraryPath = proposal.Source.Type == MediaType.Episode
                ? _settings.TvShowsPath ?? _settings.OutputPath
                : _settings.MoviesPath ?? _settings.OutputPath;

            var extension = Path.GetExtension(proposal.Source.OriginalPath);
            var targetPath = Path.Combine(libraryPath, proposal.ProposedName + extension);

            var targetDir = Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrEmpty(targetDir) && !Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
                if (logger.IsEnabled(LogLevel.Information))
                    logger.LogInformation("Created directory: {Directory}", targetDir);
            }

            if (File.Exists(targetPath))
            {
                throw new IOException($"Target file already exists: {targetPath}");
            }

            File.Move(proposal.Source.OriginalPath, targetPath);
            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("Renamed: {Source} → {Target}",
                    proposal.Source.OriginalPath, targetPath);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occured while renaming the file");
            throw new InvalidOperationException($"Failed to rename {proposal.Source.OriginalPath}", ex);
        }
    }
}