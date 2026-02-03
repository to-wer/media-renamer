using MediaRenamer.Core.Models;

namespace MediaRenamer.Core.Abstractions;

public interface IProposalStore
{
    Task Add(RenameProposal proposal);
    Task<List<RenameProposal>> GetAll(string? sortBy = "ScanTime", bool descending = true);
    Task<RenameProposal?> GetById(Guid id);
    Task Approve(Guid id);
    Task Reject(Guid id);
    Task Clear();
    Task<ProposalStats> GetStats();
    Task<List<RenameProposal>> GetPending();
    Task<List<RenameProposal>> GetHistory();
    Task SetStatus(Guid id, ProposalStatus status);
}