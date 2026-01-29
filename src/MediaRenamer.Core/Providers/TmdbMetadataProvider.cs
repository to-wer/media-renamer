using System.Diagnostics;
using System.Text.RegularExpressions;
using MediaRenamer.Core.Abstractions;
using MediaRenamer.Core.Models;
using TMDbLib.Client;

namespace MediaRenamer.Core.Providers;

public class TmdbMetadataProvider : IMetadataProvider
{
    private readonly TMDbClient _client;

    public TmdbMetadataProvider(string apiKey)
    {
        _client = new TMDbClient(apiKey);
    }

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
        var result = await _client.SearchMovieAsync(file.FileName, language: "de-DE");

        var movie = result.Results.FirstOrDefault();
        if (movie == null)
            return null;

        file.Title = movie.Title;
        file.Year = movie.ReleaseDate?.Year;

        return file;
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
        file.Year = episode.AirDate?.Year;
        file.EpisodeTitle = episode.Name;

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