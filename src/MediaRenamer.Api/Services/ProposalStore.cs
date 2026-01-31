using MediaRenamer.Api.Data;
using MediaRenamer.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace MediaRenamer.Api.Services;

public class ProposalStore(ProposalDbContext dbContext)
{
    private readonly List<RenameProposal> _proposals = new();

    public IReadOnlyList<RenameProposal> Proposals => _proposals.AsReadOnly();

    public async Task Add(RenameProposal proposal)
    {
        _proposals.Add(proposal);
        await dbContext.Proposals.AddAsync(proposal);
        await dbContext.SaveChangesAsync();
    }

    public async Task<List<RenameProposal>> GetAll(string? sortBy = "ScanTime", bool descending = true)
    {
        var query = dbContext.Proposals
            .Include(p => p.Source)
            .AsQueryable();

        return sortBy?.ToLower() switch
        {
            "scantime" => descending
                ? await query.OrderByDescending(p => p.ScanTime).ToListAsync()
                : await query.OrderBy(p => p.ScanTime).ToListAsync(),
            "filename" => descending
                ? await query.OrderByDescending(p => p.Source.OriginalPath).ToListAsync()
                : await query.OrderBy(p => p.Source.OriginalPath).ToListAsync(),
            "status" => descending
                ? await query.OrderByDescending(p => p.Status == ProposalStatus.Approved ? 2 : p.Status == ProposalStatus.Rejected ? 1 : 0).ToListAsync()
                : await query.OrderBy(p => p.Status == ProposalStatus.Approved ? 2 : p.Status == ProposalStatus.Rejected ? 1 : 0).ToListAsync(),
            _ => await query.OrderByDescending(p => p.ScanTime).ToListAsync()
        };
    }

    public async Task<RenameProposal?> GetByPath(string filePath)
    {
        return await dbContext.Proposals
            .Include(p => p.Source)
            .FirstOrDefaultAsync(p => p.Source.OriginalPath == filePath);
    }

    public async Task Approve(string filePath)
    {
        var prop = await GetByPath(filePath);
        if (prop is { Status: ProposalStatus.Pending })
        {
            prop.Status = ProposalStatus.Approved;
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task Reject(string filePath)
    {
        var prop = await GetByPath(filePath);
        if (prop is { Status: ProposalStatus.Pending })
        {
            prop.Status = ProposalStatus.Rejected;
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task Clear()
    {
        dbContext.Proposals.RemoveRange(await dbContext.Proposals.ToListAsync());
        await dbContext.SaveChangesAsync();
    }

    public async Task<List<RenameProposal>> GetPending() =>
        await dbContext.Proposals.Where(p => p.Status == ProposalStatus.Pending).OrderByDescending(p => p.ScanTime)
            .Include(p => p.Source).ToListAsync();

    public async Task<List<RenameProposal>> GetHistory() =>
        await dbContext.Proposals.Where(p => p.Status != ProposalStatus.Pending)
            .OrderByDescending(p => p.ScanTime)
            .Include(p => p.Source)
            .ToListAsync();
}