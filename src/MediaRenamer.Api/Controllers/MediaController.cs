using MediaRenamer.Core.Abstractions;
using MediaRenamer.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace MediaRenamer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MediaController(
    IMediaScanner scanner,
    MetadataResolver resolver,
    IRenameService renamer) : ControllerBase
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
}