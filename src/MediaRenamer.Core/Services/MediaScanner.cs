using System.Text.RegularExpressions;
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

        return Task.FromResult(media);
    }
}