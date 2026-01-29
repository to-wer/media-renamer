using System.Text.RegularExpressions;
using MediaInfo;
using MediaRenamer.Core.Abstractions;
using MediaRenamer.Core.Models;

namespace MediaRenamer.Core.Services;

public class MediaScanner : IMediaScanner
{
    public Task<MediaFile> AnalyzeAsync(string filePath)
    {
        var fileName = Path.GetFileNameWithoutExtension(filePath);

        var episodeMatch = Regex.Match(fileName, @"S(\d+)E(\d+)", RegexOptions.IgnoreCase);

        var media = new MediaFile
        {
            OriginalPath = filePath,
            FileName = fileName,
            Type = episodeMatch.Success ? MediaType.Episode : MediaType.Movie
        };

        if (episodeMatch.Success)
        {
            media.Season = int.Parse(episodeMatch.Groups[1].Value);
            media.Episode = int.Parse(episodeMatch.Groups[2].Value);
        }

        try
        {
            using var mi = new MediaInfo.MediaInfo();
            mi.Open(filePath);
    
            // Resolution
            var height = mi.Get(StreamKind.Video, 0, "Height");
            if (int.TryParse(height, out var h))
            {
                media.Resolution = h switch
                {
                    >= 2160 => "4K",
                    >= 1080 => "1080p",
                    >= 720 => "720p",
                    _ => $"{h}p"
                };
            }
    
            // Codec
            string codec = mi.Get(StreamKind.Video, 0, "Format");
            
            media.Codec = codec == "HEVC" ? "x265" : codec?.ToLower() ?? "unknown";
        }
        catch { /* fallback */ }
        
        return Task.FromResult(media);
    }
}