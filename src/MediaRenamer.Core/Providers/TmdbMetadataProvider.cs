using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks.Dataflow;
using MediaRenamer.Core.Abstractions;
using MediaRenamer.Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TMDbLib.Client;

namespace MediaRenamer.Core.Providers;

public class TmdbMetadataProvider(IOptions<MetadataProviderSettings> metadataProviderSettings, ILogger<TmdbMetadataProvider> logger) : IMetadataProvider
{
    private readonly TMDbClient _client = new(metadataProviderSettings.Value.TmdbApiKey ?? throw new ArgumentException("MetadataProviders:TmdbApiKey"));

    public async Task<MediaFile?> EnrichAsync(MediaFile file)
    {
        return file.Type switch
        {
            MediaType.Movie => await EnrichMovie(file),
            MediaType.Episode => await EnrichEpisode(file),
            _ => file
        };
    }

    private async Task<MediaFile?> EnrichMovie(MediaFile file)
    {
        var (baseTitle, year) = ExtractMovieInfo(file.FileName);

        var result = await _client.SearchMovieAsync(file.ParsedTitle ?? baseTitle, language: "de-DE");

        if ((result?.TotalResults ?? 0) == 0)
        {
            // TODO: try to get name from llm
            if(logger.IsEnabled(LogLevel.Debug))
                logger.LogDebug("TMDb search for '{Title}' returned no results, retrying without year.", baseTitle);
            
            // Retry without year
            var yearlessName = Regex.Replace(file.FileName, @"\s\(\d{4}\)$", "");
            result = await _client.SearchMovieAsync(yearlessName, language: "de-DE");
        }
        
        var movie = result.Results.FirstOrDefault(x => string.IsNullOrEmpty(year) || x.ReleaseDate?.Year.ToString() == year);
        if (movie == null)
        {
            if(logger.IsEnabled(LogLevel.Debug))
                logger.LogDebug("TMDb search for '{Title}' returned no results.", baseTitle);
            return null;
        }

        if (year != null && movie.ReleaseDate?.Year.ToString() != year)
            return null;
        
        file.Title = movie.Title;
        file.Year = movie.ReleaseDate?.Year;

        return file;
    }
    
    private static (string title, string? year) ExtractMovieInfo(string filename)
    {
        var movieMatch = Regex.Match(filename, @"^(.+?)\.(\d{4})\.(German|AC3|DD|AAC|ENG|English|DL|MULTi|DTS).*?$", RegexOptions.IgnoreCase);
        if (movieMatch.Success)
        {
            return (movieMatch.Groups[1].Value.Trim('.'), movieMatch.Groups[2].Value);
        }
        
        var fallback = Regex.Match(filename, @"(.+?)\.(\d{4})");
        return fallback.Success ? (fallback.Groups[1].Value.Trim('.'), fallback.Groups[2].Value) : (filename, null);
    }

    private async Task<MediaFile?> EnrichEpisode(MediaFile file)
    {
        var seriesName = ExtractSeriesName(file.FileName);

        var search = await _client.SearchTvShowAsync(seriesName, language: "de-DE");
        var show = search.Results.FirstOrDefault();

        if (show == null || !file.Season.HasValue || !file.Episode.HasValue)
            return null;

        var episode = await _client.GetTvEpisodeAsync(
            show.Id,
            file.Season.Value,
            file.Episode.Value,
            language: "de-DE"
        );

        file.Title = show.Name;
        file.Year = show.FirstAirDate?.Year;
        file.EpisodeTitle = episode?.Name;

        return file;
    }
    
    private string ExtractSeriesName(string filename)
    {
        var episodeMatch = Regex.Match(filename, @"S(\d+)E(\d+)", RegexOptions.IgnoreCase);

        if (!episodeMatch.Success)
            return filename;

        var seriesName = filename.Substring(0, episodeMatch.Index)
            .Replace('.', ' ')
            .Trim();

        return seriesName;
    }
}