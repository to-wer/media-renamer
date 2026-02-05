namespace MediaRenamer.Core.Models;

public enum DuplicateFileHandling
{
    Skip,
    Overwrite,
    RenameWithSuffix
}

public class MediaSettings
{
    public string WatchPath { get; set; } = string.Empty;
    public string OutputPath { get; set; } = string.Empty;
    public string? MoviesPath { get; set; }
    public string? TvShowsPath { get; set; }
    public int ScanInterval { get; set; } = 30;
    public DuplicateFileHandling DuplicateFileHandling { get; set; } = DuplicateFileHandling.Skip;
    
    public string TvSeriesTemplate { get; set; } = "{SeriesName} ({Year})/Season {Season:D2}/{SeriesName} - S{Season:D2}E{Episode:D2} - {EpisodeName} [{Resolution}] [{Codec}]";
    public string MovieTemplate { get; set; } = "{Title} ({Year}}/{Title} ({Year}) [{Resolution}] [{Codec}]";
}
