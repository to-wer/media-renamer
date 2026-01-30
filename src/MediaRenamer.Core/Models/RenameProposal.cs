using System.ComponentModel.DataAnnotations;

namespace MediaRenamer.Core.Models;

public class RenameProposal
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime ScanTime { get; set; } = DateTime.UtcNow;
    public MediaFile Source { get; set; } = default!;
    public string ProposedName { get; set; } = string.Empty;

    public bool RequiresApproval { get; set; } = true;
    public bool IsApproved { get; set; } = false;
    public bool IsRejected { get; set; } = false;
}