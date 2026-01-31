using System.ComponentModel.DataAnnotations;

namespace MediaRenamer.Core.Models;

public class RenameProposal
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime ScanTime { get; set; } = DateTime.UtcNow;
    public MediaFile Source { get; set; } = default!;
    public string ProposedName { get; set; } = string.Empty;
    public ProposalStatus Status { get; set; }
}