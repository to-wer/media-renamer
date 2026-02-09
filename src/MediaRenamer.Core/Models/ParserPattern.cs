namespace MediaRenamer.Core.Models;

/// <summary>
/// Represents a pattern category for parsing media filenames.
/// </summary>
public enum ParserPatternCategory
{
    /// <summary>Video resolution patterns (1080p, 720p, 4k, etc.)</summary>
    Resolution,
    
    /// <summary>Video codec patterns (x264, x265, HEVC, etc.)</summary>
    Codec,
    
    /// <summary>Audio codec patterns (AC3, DTS, etc.)</summary>
    AudioCodec,
    
    /// <summary>Language patterns (German, English, etc.)</summary>
    Language,
    
    /// <summary>Release group/source patterns (RARBG, YTS, BluRay, etc.)</summary>
    ReleaseSource,
    
    /// <summary>File size patterns (1GB, 500MB, etc.)</summary>
    FileSize,
    
    /// <summary>Quality modifiers (PROPER, REPACK, etc.)</summary>
    QualityModifier,
    
    /// <summary>Episode patterns (S01E01, Season, Episode, etc.)</summary>
    Episode,
    
    /// <summary>Custom/additional patterns</summary>
    Custom
}

/// <summary>
/// Represents a configurable pattern for media filename parsing.
/// </summary>
public class ParserPattern
{
    /// <summary>
    /// Unique identifier for the pattern.
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// The regex pattern string.
    /// </summary>
    public string Pattern { get; set; } = string.Empty;
    
    /// <summary>
    /// Category of the pattern for organization.
    /// </summary>
    public ParserPatternCategory Category { get; set; }
    
    /// <summary>
    /// Whether this pattern should be removed from the title.
    /// </summary>
    public bool RemoveFromTitle { get; set; } = true;
    
    /// <summary>
    /// Whether the pattern matching should be case-insensitive.
    /// </summary>
    public bool CaseInsensitive { get; set; } = true;
    
    /// <summary>
    /// Priority for pattern matching order (lower = higher priority).
    /// </summary>
    public int Priority { get; set; }
    
    /// <summary>
    /// Human-readable description of the pattern.
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Whether this pattern is enabled.
    /// </summary>
    public bool IsEnabled { get; set; } = true;
}
