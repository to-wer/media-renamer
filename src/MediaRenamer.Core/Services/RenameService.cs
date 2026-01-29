using MediaRenamer.Core.Abstractions;
using MediaRenamer.Core.Models;

namespace MediaRenamer.Core.Services;

public class RenameService : IRenameService
{
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

        return new RenameProposal
        {
            Source = file,
            ProposedName = name,
            RequiresApproval = true
        };
    }

    public Task ExecuteAsync(RenameProposal proposal)
    {
        var target = Path.Combine(
            Path.GetDirectoryName(proposal.Source.OriginalPath)!,
            proposal.ProposedName + Path.GetExtension(proposal.Source.OriginalPath)
        );

        File.Move(proposal.Source.OriginalPath, target);
        return Task.CompletedTask;
    }
}