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

    public ProposalStatus Execute(RenameProposal proposal)
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

            // Handle duplicate files based on the configured strategy
            if (File.Exists(targetPath))
            {
                switch (_settings.DuplicateFileHandling)
                {
                    case Models.DuplicateFileHandling.Skip:
                        logger.LogWarning("Skipping rename: Target file already exists: {TargetPath}", targetPath);
                        return ProposalStatus.Skipped;

                    case Models.DuplicateFileHandling.Overwrite:
                        logger.LogInformation("Overwriting existing file: {TargetPath}", targetPath);
                        File.Delete(targetPath);
                        break;

                    case Models.DuplicateFileHandling.RenameWithSuffix:
                        targetPath = GetUniqueFilePath(targetPath);
                        logger.LogInformation("Using alternative filename to avoid conflict: {TargetPath}", targetPath);
                        break;
                }
            }

            File.Move(proposal.Source.OriginalPath, targetPath);
            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("Renamed: {Source} → {Target}",
                    proposal.Source.OriginalPath, targetPath);
            return ProposalStatus.Processed;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occured while renaming the file");
            throw new InvalidOperationException($"Failed to rename {proposal.Source.OriginalPath}", ex);
        }
    }

    private static string GetUniqueFilePath(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath) ?? string.Empty;
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
        var extension = Path.GetExtension(filePath);
        var counter = 1;

        while (File.Exists(filePath))
        {
            filePath = Path.Combine(directory, $"{fileNameWithoutExtension}_{counter}{extension}");
            counter++;
        }

        return filePath;
    }
}
