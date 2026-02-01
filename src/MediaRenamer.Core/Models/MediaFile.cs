namespace MediaRenamer.Core.Models;

public class MediaFile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string OriginalPath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string? ParsedTitle { get; set; }

    public MediaType Type { get; set; }

    public string? Title { get; set; }
    public int? Year { get; set; }

    public int? Season { get; set; }
    public int? Episode { get; set; }
    public string? EpisodeTitle { get; set; }

    public string? Resolution { get; set; }
    public string? Codec { get; set; }
}