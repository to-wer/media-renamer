using MediaRenamer.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace MediaRenamer.Api.Data;

public class ProposalDbContext(DbContextOptions<ProposalDbContext> options) : DbContext(options)
{
    public DbSet<RenameProposal> Proposals { get; set; }
    public DbSet<MediaFile> Files { get; set; }
}