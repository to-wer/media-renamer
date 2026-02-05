using MediaRenamer.Api.Data;
using MediaRenamer.Core.Abstractions;
using MediaRenamer.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace MediaRenamer.Api.Services;

public class ProposalStore(ProposalDbContext dbContext) : IProposalStore
{
    public async Task Add(RenameProposal proposal)
    {
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
                ? await query.OrderByDescending(p =>
                    p.Status == ProposalStatus.Approved ? 2 : p.Status == ProposalStatus.Rejected ? 1 : 0).ToListAsync()
                : await query.OrderBy(p =>
                        p.Status == ProposalStatus.Approved ? 2 : p.Status == ProposalStatus.Rejected ? 1 : 0)
                    .ToListAsync(),
            _ => await query.OrderByDescending(p => p.ScanTime).ToListAsync()
        };
    }

    public async Task<RenameProposal?> GetById(Guid id)
    {
        return await dbContext.Proposals
            .Include(p => p.Source)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task Approve(Guid id)
    {
        var prop = await GetById(id);
        if (prop is { Status: ProposalStatus.Pending })
        {
            prop.Status = ProposalStatus.Approved;
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task Reject(Guid id)
    {
        var prop = await GetById(id);
        if (prop is { Status: ProposalStatus.Pending })
        {
            prop.Status = ProposalStatus.Rejected;
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task Delete(Guid id)
    {
        var prop = await GetById(id);
        if (prop != null)
        {
            dbContext.Proposals.Remove(prop);
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task DeleteMany(IEnumerable<Guid> ids)
    {
        var idList = ids.ToList();
        if (idList.Count == 0)
            return;

        var proposals = await dbContext.Proposals
            .Where(p => idList.Contains(p.Id))
            .ToListAsync();

        if (proposals.Count > 0)
        {
            dbContext.Proposals.RemoveRange(proposals);
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task Clear()
    {
        dbContext.Proposals.RemoveRange(await dbContext.Proposals.ToListAsync());
        await dbContext.SaveChangesAsync();
    }

    public async Task<ProposalStats> GetStats()
    {
        var pending = await dbContext.Proposals.CountAsync(p => p.Status == ProposalStatus.Pending);
        var approved = await dbContext.Proposals.CountAsync(p => p.Status == ProposalStatus.Approved);
        var rejected = await dbContext.Proposals.CountAsync(p => p.Status == ProposalStatus.Rejected);

        return new ProposalStats
        {
            Pending = pending,
            Approved = approved,
            Rejected = rejected
        };
    }
    
    public async Task<List<RenameProposal>> GetPending() =>
        await dbContext.Proposals.Where(p => p.Status == ProposalStatus.Pending).OrderByDescending(p => p.ScanTime)
            .Include(p => p.Source).ToListAsync();

    public async Task<List<RenameProposal>> GetHistory() =>
        await dbContext.Proposals.Where(p => p.Status != ProposalStatus.Pending)
            .OrderByDescending(p => p.ScanTime)
            .Include(p => p.Source)
            .ToListAsync();

    public async Task SetStatus(Guid id, ProposalStatus status)
    {
        var prop = await GetById(id);
        if (prop != null)
        {
            prop.Status = status;
            await dbContext.SaveChangesAsync();
        }
    }
}