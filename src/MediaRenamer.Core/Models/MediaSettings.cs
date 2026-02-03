namespace MediaRenamer.Core.Models;

public class MediaSettings
{
    public string WatchPath { get; set; } = string.Empty;
    public string OutputPath { get; set; } = string.Empty;
    public int ScanInterval { get; set; } = 30;
}
