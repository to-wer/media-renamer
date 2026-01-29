using MediaRenamer.Core.Models;

namespace MediaRenamer.Api.Services;

public class ProposalStore
{
    private readonly List<RenameProposal> _proposals = new();

    public IReadOnlyList<RenameProposal> Proposals => _proposals.AsReadOnly();

    public void Add(RenameProposal proposal)
    {
        _proposals.Add(proposal);
    }

    public RenameProposal? Get(string filePath)
        => _proposals.FirstOrDefault(p => p.Source.OriginalPath == filePath);

    public void Approve(string filePath)
    {
        var proposal = Get(filePath);
        if (proposal != null)
            proposal.RequiresApproval = false;
    }

    public void Reject(string filePath)
    {
        var proposal = Get(filePath);
        if (proposal != null)
            _proposals.Remove(proposal);
    }
}
