using MediaRenamer.Api.Services;
using MediaRenamer.Core.Abstractions;
using MediaRenamer.Core.Models;
using MediaRenamer.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace MediaRenamer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MediaController(
    IMediaScanner scanner,
    MetadataResolver resolver,
    IRenameService renamer,
    ProposalStore proposalStore,
    ILogger<MediaController> logger) : ControllerBase
{
    [HttpPost("scan")]
    public async Task<IActionResult> Scan([FromBody] string path)
    {
        var file = await scanner.AnalyzeAsync(path);
        var enriched = await resolver.ResolveAsync(file);
        if (enriched == null) return NotFound("Metadata could not be resolved.");
        var proposal = renamer.CreateProposal(enriched);

        return Ok(proposal);
    }

    [HttpGet("proposals")]
    public async Task<ActionResult<List<RenameProposal>>> GetProposals(
        [FromQuery] string? sortBy = "ScanTime",
        [FromQuery] bool descending = true)
    {
        var proposals = await proposalStore.GetAll(sortBy, descending);
        return Ok(proposals);
    }

    [HttpPost("approve/{id:guid}")]
    public async Task<IActionResult> Approve(Guid id)
    {
        var proposal = await proposalStore.GetById(id);
        if (proposal == null)
            return NotFound();

        try
        {
            await renamer.ExecuteAsync(proposal);
            await proposalStore.Approve(proposal.Id);
        }
        catch (Exception ex)
        {
            // TODO: set proposal to error state
            // proposal.Status = ProposalStatus.Error;
            if (logger.IsEnabled(LogLevel.Error))
            {
                logger.LogError("Error approving proposal for {filePath}: {error}", proposal.Source.OriginalPath, ex.Message);
            }

            return StatusCode(500, $"Error approving proposal: {ex.Message}");
        }

        return Ok(proposal);
    }

    [HttpPost("reject/{id:guid}")]
    public async Task<IActionResult> Reject(Guid id)
    {
        await proposalStore.Reject(id);
        return Ok();
    }

    [HttpDelete("clear")]
    public async Task<IActionResult> Clear()
    {
        await proposalStore.Clear();
        return Ok();
    }

    [HttpGet("stats")]
    public async Task<ActionResult<object>> GetStats()
    {
        var store = HttpContext.RequestServices.GetRequiredService<ProposalStore>();
        var pending = (await store.GetAll()).Count(p => p.Status == ProposalStatus.Pending);
        var approved = (await store.GetAll()).Count(p => p.Status == ProposalStatus.Approved);
        var rejected = (await store.GetAll()).Count(p => p.Status == ProposalStatus.Rejected);

        return Ok(new ProposalStats() { Pending = pending, Approved = approved, Rejected = rejected });
    }

    [HttpGet("pending")]
    public async Task<ActionResult<List<RenameProposal>>> GetPending() =>
        Ok(await proposalStore.GetPending());

    [HttpGet("history")]
    public async Task<ActionResult<List<RenameProposal>>> GetHistory() =>
        Ok(await proposalStore.GetHistory());
}