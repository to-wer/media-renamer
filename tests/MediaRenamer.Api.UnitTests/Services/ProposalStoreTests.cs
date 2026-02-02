using MediaRenamer.Api.Data;
using MediaRenamer.Api.Services;
using MediaRenamer.Core.Models;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace MediaRenamer.Api.UnitTests.Services;

public class ProposalStoreTests
{
    private ProposalDbContext _context;
    private ProposalStore _store;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ProposalDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ProposalDbContext(options);
        _store = new ProposalStore(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Test]
    public async Task GetStats_ShouldReturnCorrectCounts()
    {
        // Arrange
        var pending = CreateProposal(ProposalStatus.Pending);
        var approved = CreateProposal(ProposalStatus.Approved);
        var rejected = CreateProposal(ProposalStatus.Rejected);
        
        await _context.Proposals.AddRangeAsync(pending, approved, rejected);
        await _context.SaveChangesAsync();
    
        // Act
        var stats = await _store.GetStats();
    
        // Assert
        stats.Pending.ShouldBe(1);
        stats.Approved.ShouldBe(1);
        stats.Rejected.ShouldBe(1);
    }

    [Test]
    public async Task GetPending_ShouldOnlyReturnPendingProposals()
    {
        // Arrange
        var pending = CreateProposal(ProposalStatus.Pending);
        var approved = CreateProposal(ProposalStatus.Approved);
        await _context.Proposals.AddRangeAsync(pending, approved);
        await _context.SaveChangesAsync();

        // Act
        var result = await _store.GetPending();

        // Assert
        result.Count.ShouldBe(1);
        result.First().Status.ShouldBe(ProposalStatus.Pending);
    }

    [Test]
    public async Task Approve_ShouldUpdateStatus()
    {
        // Arrange
        var proposal = CreateProposal(ProposalStatus.Pending);
        await _context.Proposals.AddAsync(proposal);
        await _context.SaveChangesAsync();

        // Act
        await _store.Approve(proposal.Id);

        // Assert
        var updated = await _context.Proposals.FindAsync(proposal.Id);
        updated.ShouldNotBeNull();
        updated.Status.ShouldBe(ProposalStatus.Approved);
    }

    private RenameProposal CreateProposal(ProposalStatus status)
    {
        return new RenameProposal
        {
            Id = Guid.NewGuid(),
            ScanTime = DateTime.UtcNow,
            Status = status,
            ProposedName = "Test.Movie.2024.mkv",
            Source = new MediaFile
            {
                Id = Guid.NewGuid(),
                OriginalPath = "C:\\temp\\test.mkv",
                ParsedTitle = "Test Movie"
            }
        };
    }
}