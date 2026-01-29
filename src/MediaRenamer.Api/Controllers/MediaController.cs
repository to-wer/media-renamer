using MediaRenamer.Api.Services;
using MediaRenamer.Core.Abstractions;
using MediaRenamer.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace MediaRenamer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MediaController(
    IMediaScanner scanner,
    MetadataResolver resolver,
    IRenameService renamer,
    ProposalStore proposalStore) : ControllerBase
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
    public IActionResult GetProposals()
        => Ok(proposalStore.Proposals);
    
    [HttpPost("approve")]
    public async Task<IActionResult> Approve([FromQuery] string filePath)
    {
        var proposal = proposalStore.Get(filePath);
        if (proposal == null)
            return NotFound();

        proposalStore.Approve(filePath);
        await renamer.ExecuteAsync(proposal);

        return Ok(proposal);
    }
    
    [HttpPost("reject")]
    public IActionResult Reject([FromQuery] string filePath)
    {
        proposalStore.Reject(filePath);
        return Ok();
    }
}