namespace MediaRenamer.Core.Models;

public class RenameProposal
{
    public MediaFile Source { get; set; } = default!;
    public string ProposedName { get; set; } = string.Empty;

    public bool RequiresApproval { get; set; } = true;
    public string? Reason { get; set; }
}