using MediaRenamer.Core.Abstractions;
using MediaRenamer.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MediaRenamer.Core.Services;

public class RenameService(IOptions<MediaSettings> settings, 
    ILogger<RenameService> logger) : IRenameService
{
    private readonly MediaSettings _settings = settings.Value;
    
    public RenameProposal CreateProposal(MediaFile file)
    {
        var name = file.Type switch
        {
            MediaType.Movie =>
                $"{file.Title} ({file.Year})",

            MediaType.Episode =>
                $"{file.Title} - S{file.Season:00}E{file.Episode:00} - {file.EpisodeTitle}",

            _ => file.FileName
        };

        if (!string.IsNullOrEmpty(file.Resolution))
        {
            name = $"{name} [{file.Resolution}]";
        }
        
        if (!string.IsNullOrEmpty(file.Codec))
        {
            name = $"{name} [{file.Codec}]";
        }

        return new RenameProposal
        {
            Source = file,
            ProposedName = name,
            Status = ProposalStatus.Pending
        };
    }

    public Task ExecuteAsync(RenameProposal proposal)
    {
        try
        {
            var target = Path.Combine(
                _settings.OutputPath,
                proposal.ProposedName + Path.GetExtension(proposal.Source.OriginalPath)
            );


            var targetDir = Path.GetDirectoryName(target);
            if (!string.IsNullOrEmpty(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            if (File.Exists(target))
            {
                throw new IOException($"Target file already exists: {target}");
            }

            File.Move(proposal.Source.OriginalPath, target);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occured while renaming the file");
            throw new InvalidOperationException(
                $"Failed to rename '{proposal.Source.OriginalPath}' to '{proposal.ProposedName}'", 
                ex
            );
        }
    }
}