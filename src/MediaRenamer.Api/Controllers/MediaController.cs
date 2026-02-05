using MediaRenamer.Core.Abstractions;
using MediaRenamer.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace MediaRenamer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MediaController(
    IRenameService renamer,
    IProposalStore proposalStore,
    ILogger<MediaController> logger) : ControllerBase
{
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

        // Check if the source file still exists
        if (!System.IO.File.Exists(proposal.Source.OriginalPath))
        {
            if (logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning("File not found during approval, deleting proposal: {filePath}",
                    proposal.Source.OriginalPath);
            }

            await proposalStore.Delete(id);
            return NotFound("Source file no longer exists");
        }

        try
        {
            await proposalStore.Approve(proposal.Id);
            var proposalStatus = renamer.Execute(proposal);
            await proposalStore.SetStatus(id, proposalStatus);
        }
        catch (Exception ex)
        {
            await proposalStore.SetStatus(id, ProposalStatus.Error);
            if (logger.IsEnabled(LogLevel.Error))
            {
                logger.LogError("Error approving proposal for {filePath}: {error}", proposal.Source.OriginalPath,
                    ex.Message);
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
        var proposalStats = await proposalStore.GetStats();
        return Ok(proposalStats);
    }

    [HttpGet("pending")]
    public async Task<ActionResult<List<RenameProposal>>> GetPending() =>
        Ok(await proposalStore.GetPending());

    [HttpGet("history")]
    public async Task<ActionResult<List<RenameProposal>>> GetHistory() =>
        Ok(await proposalStore.GetHistory());
}