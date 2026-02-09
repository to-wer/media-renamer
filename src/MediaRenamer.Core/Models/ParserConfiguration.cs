namespace MediaRenamer.Core.Models;

/// <summary>
/// Configuration for the filename parser containing all pattern definitions.
/// </summary>
public class ParserConfiguration
{
    /// <summary>
    /// Collection of patterns used for parsing filenames.
    /// </summary>
    public List<ParserPattern> Patterns { get; set; } = new();
    
    /// <summary>
    /// Default configuration with standard media patterns.
    /// </summary>
    public static ParserConfiguration GetDefaultConfiguration()
    {
        var config = new ParserConfiguration();
        
        // Resolution patterns
        config.Patterns.AddRange(new[]
        {
            new ParserPattern
            {
                Id = "4k-uhd",
                Pattern = @"2160[p|i]|4k|uhd",
                Category = ParserPatternCategory.Resolution,
                Priority = 1,
                Description = "4K/UHD resolution"
            },
            new ParserPattern
            {
                Id = "1080p",
                Pattern = @"1080[p|i]",
                Category = ParserPatternCategory.Resolution,
                Priority = 2,
                Description = "1080p resolution"
            },
            new ParserPattern
            {
                Id = "720p",
                Pattern = @"720[p|i]",
                Category = ParserPatternCategory.Resolution,
                Priority = 3,
                Description = "720p resolution"
            },
            new ParserPattern
            {
                Id = "480p",
                Pattern = @"480[p|i]",
                Category = ParserPatternCategory.Resolution,
                Priority = 4,
                Description = "480p resolution"
            }
        });
        
        // Codec patterns
        config.Patterns.AddRange(new[]
        {
            new ParserPattern
            {
                Id = "h265-hevc",
                Pattern = @"h265|hevc|x265",
                Category = ParserPatternCategory.Codec,
                Priority = 1,
                Description = "H.265/HEVC codec"
            },
            new ParserPattern
            {
                Id = "h264-avc",
                Pattern = @"h264|avc|x264",
                Category = ParserPatternCategory.Codec,
                Priority = 2,
                Description = "H.264/AVC codec"
            },
            new ParserPattern
            {
                Id = "av1",
                Pattern = @"av1",
                Category = ParserPatternCategory.Codec,
                Priority = 3,
                Description = "AV1 codec"
            }
        });
        
        // Audio codec patterns
        config.Patterns.AddRange(new[]
        {
            new ParserPattern
            {
                Id = "truehd",
                Pattern = @"truehd",
                Category = ParserPatternCategory.AudioCodec,
                Priority = 1,
                Description = "TrueHD audio"
            },
            new ParserPattern
            {
                Id = "dts-hd",
                Pattern = @"dts.?hd|dtshd",
                Category = ParserPatternCategory.AudioCodec,
                Priority = 2,
                Description = "DTS-HD audio"
            },
            new ParserPattern
            {
                Id = "eac3",
                Pattern = @"eac3",
                Category = ParserPatternCategory.AudioCodec,
                Priority = 3,
                Description = "E-AC3 audio"
            },
            new ParserPattern
            {
                Id = "dd51",
                Pattern = @"dd5\.1",
                Category = ParserPatternCategory.AudioCodec,
                Priority = 4,
                Description = "Dolby Digital 5.1"
            },
            new ParserPattern
            {
                Id = "dd71",
                Pattern = @"dd7\.1",
                Category = ParserPatternCategory.AudioCodec,
                Priority = 5,
                Description = "Dolby Digital 7.1"
            },
            new ParserPattern
            {
                Id = "dts",
                Pattern = @"dts",
                Category = ParserPatternCategory.AudioCodec,
                Priority = 6,
                Description = "DTS audio"
            }
        });
        
        // Language patterns
        config.Patterns.AddRange(new[]
        {
            new ParserPattern
            {
                Id = "german",
                Pattern = @"german|deutsch|german\.dubbed|dl\.german",
                Category = ParserPatternCategory.Language,
                Priority = 1,
                Description = "German language"
            },
            new ParserPattern
            {
                Id = "english",
                Pattern = @"english|eng",
                Category = ParserPatternCategory.Language,
                Priority = 2,
                Description = "English language"
            },
            new ParserPattern
            {
                Id = "multilingual",
                Pattern = @"multi",
                Category = ParserPatternCategory.Language,
                Priority = 3,
                Description = "Multiple languages"
            },
            new ParserPattern
            {
                Id = "forced",
                Pattern = @"forced",
                Category = ParserPatternCategory.Language,
                Priority = 4,
                Description = "Forced subtitles"
            }
        });
        
        // Release source patterns
        config.Patterns.AddRange(new[]
        {
            new ParserPattern
            {
                Id = "blu-ray",
                Pattern = @"blu.?ray|bdrip|bdr",
                Category = ParserPatternCategory.ReleaseSource,
                Priority = 1,
                Description = "BluRay source"
            },
            new ParserPattern
            {
                Id = "dvd",
                Pattern = @"dvdrip|dvd",
                Category = ParserPatternCategory.ReleaseSource,
                Priority = 2,
                Description = "DVD source"
            },
            new ParserPattern
            {
                Id = "web-dl",
                Pattern = @"web[-.]?(dl|rip)",
                Category = ParserPatternCategory.ReleaseSource,
                Priority = 3,
                Description = "Web download/rip"
            },
            new ParserPattern
            {
                Id = "rarbg",
                Pattern = @"rarbg",
                Category = ParserPatternCategory.ReleaseSource,
                Priority = 4,
                Description = "RARBG release"
            },
            new ParserPattern
            {
                Id = "yts",
                Pattern = @"yts",
                Category = ParserPatternCategory.ReleaseSource,
                Priority = 5,
                Description = "YTS release"
            },
            new ParserPattern
            {
                Id = "torrentgalaxy",
                Pattern = @"torrentgalaxy",
                Category = ParserPatternCategory.ReleaseSource,
                Priority = 6,
                Description = "TorrentGalaxy release"
            }
        });
        
        // Quality modifier patterns
        config.Patterns.AddRange(new[]
        {
            new ParserPattern
            {
                Id = "proper",
                Pattern = @"proper",
                Category = ParserPatternCategory.QualityModifier,
                Priority = 1,
                Description = "Proper release"
            },
            new ParserPattern
            {
                Id = "repack",
                Pattern = @"repack",
                Category = ParserPatternCategory.QualityModifier,
                Priority = 2,
                Description = "Repack release"
            }
        });
        
        // File size patterns
        config.Patterns.AddRange(new[]
        {
            new ParserPattern
            {
                Id = "file-size",
                Pattern = @"\d+\.?\d*\.?(gb|mb|kb)",
                Category = ParserPatternCategory.FileSize,
                Priority = 1,
                Description = "File size indicator"
            }
        });
        
        // Episode patterns (for detection, not removal)
        config.Patterns.AddRange(new[]
        {
            new ParserPattern
            {
                Id = "episode-standard",
                Pattern = @"s\d+e\d+|season\s*\d+|episode\s*\d+",
                Category = ParserPatternCategory.Episode,
                Priority = 1,
                RemoveFromTitle = false,
                Description = "Standard episode format"
            },
            new ParserPattern
            {
                Id = "episode-german",
                Pattern = @"staffel\s*\d+|folge\s*\d+",
                Category = ParserPatternCategory.Episode,
                Priority = 2,
                RemoveFromTitle = false,
                Description = "German episode format"
            }
        });
        
        return config;
    }
}
