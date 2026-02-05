using MediaRenamer.Core.Models;

namespace MediaRenamer.Core.Abstractions;

public interface IRenameService
{
    RenameProposal CreateProposal(MediaFile file);
    ProposalStatus Execute(RenameProposal proposal);
}