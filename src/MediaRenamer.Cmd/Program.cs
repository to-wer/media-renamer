using System.Text.RegularExpressions;
using MediaInfo;
using TMDbLib.Client;

var tmdbApiKey = Environment.GetEnvironmentVariable("MediaRenamer_TMDB_API_KEY");
var openAiApiKey = "";
bool test = false;
// TODO: support multiple providers

string inputPath = args.Length > 0 ? args[0] : throw new ArgumentException("Input path is required");
string outputPath = args.Length > 1 ? args[1] : throw new ArgumentException("Output path is required");

Console.WriteLine($"Input: {inputPath}");
Console.WriteLine($"Output: {outputPath}\n");

var tmdb = new TMDbClient(tmdbApiKey);

var mediaInfo = new MediaInfo.MediaInfo();


var files = Directory.GetFiles(inputPath, "*.*", SearchOption.AllDirectories)
    .Where(f => f.EndsWith(".mkv") || f.EndsWith(".mp4"))
    .ToList();

Console.WriteLine($"🔍 {files.Count} Dateien gefunden...\n");

foreach (var file in files)
{
    await ProcessFile(file, outputPath, tmdb, mediaInfo, test);
}

Console.WriteLine("\n✅ Fertig!");

static async Task ProcessFile(string filePath, string outputPath, TMDbClient tmdb, MediaInfo.MediaInfo mediaInfo,
    bool test)
{
    var filename = Path.GetFileNameWithoutExtension(filePath);
    Console.WriteLine($"Processing: {filename}");

    // MediaInfo für Resolution/Codec
    var media = GetMediaInfo(mediaInfo, filePath);

    // Serie oder Film erkennen
    var episodeMatch = Regex.Match(filename, @"S(\d+)E(\d+)", RegexOptions.IgnoreCase);
    var seriesMatch = Regex.Match(filename, @"^(.*?)(?i)(s\d+e\d+|$)", RegexOptions.IgnoreCase);

    // TMDB Suche
    string newName = "";
    if (episodeMatch.Success)
    {
        // TV Serie Suche (deutsch)
        var seriesName = seriesMatch.Success ? seriesMatch.Groups[1].Value.Trim() : filename;
        seriesName = seriesName.Replace(".", " ").Trim();
        Console.WriteLine(
            $"Detected TV: '{seriesName}' S{episodeMatch.Groups[1].Value}E{episodeMatch.Groups[2].Value}");

        var tvSearch = await tmdb.SearchTvShowAsync(seriesName.Trim(), language: "de-DE");
        if (tvSearch?.TotalResults > 0 && tvSearch.Results?[0] != null)
        {
            var tvShowId = tvSearch.Results[0].Id;
            var tvDetails = await tmdb.GetTvShowAsync(tvShowId, language: "de-DE");
            var seasonNum = int.Parse(episodeMatch.Groups[1].Value);
            var episodeNum = int.Parse(episodeMatch.Groups[2].Value);

            // **Echter Episode-Titel von TMDB!**
            var season = await tmdb.GetTvSeasonAsync(tvShowId, seasonNum, language: "de-DE");
            var episodeDetails = await tmdb.GetTvEpisodeAsync(tvShowId, seasonNum, episodeNum, language: "de-DE");

            newName =
                $"{tvDetails.Name} - S{seasonNum:00}E{episodeNum:00} - {episodeDetails.Name} [{media.resolution}] [{media.codec}]";
        }
        else
        {
            newName = "UNKOWN";
        }
    }
    else
    {
        // Film Suche (deutsch)
        var movieSearch = await tmdb.SearchMovieAsync(filename, language: "de-DE");
        if (movieSearch.TotalResults > 0)
        {
            var movie = await tmdb.GetMovieAsync(movieSearch.Results[0].Id, language: "de-DE");
            newName = $"{movie.Title} ({movie.ReleaseDate?.Year ?? 0}) [{media.resolution}] [{media.codec}]";
        }
        else
        {
            newName = "UNKOWN";
        }
    }

    if (newName == "UNKOWN")
    {
        // OpenAI Fallback
        newName = $"UNKNOWN TITLE [{media.resolution}] [{media.codec}]";
        // var prompt = $"Media-Datei: '{filename}'. Finde korrekten Titel (Jahr) für Plex.";
        // var chat = openai.ChatEndpoint.GetCompletionAsync(new ChatRequest(prompt, model: "gpt-4o-mini"));
        // newName = chat.Result.Choices[0].Message.Content.Trim();
        // newName += $" [{res.resolution}] [{res.codec}]";
    }

    if (newName != "UNKNOWN")
    {
        newName = newName.Replace("?", "").Replace(":", "");
        var newFilePath = Path.Combine(outputPath, $"{newName}{Path.GetExtension(filePath)}");
        Console.WriteLine($"{filePath} --> {newFilePath}\n");
        if (!test)
        {
            File.Move(filePath, newFilePath);
        }
    }
}


static (string resolution, string codec) GetMediaInfo(MediaInfo.MediaInfo mi, string file)
{
    mi.Open(file);
    var width = mi.Get(StreamKind.Video, 0, "Width");
    var height = mi.Get(StreamKind.Video, 0, "Height");
    var codec = mi.Get(StreamKind.Video, 0, "Format");

    int.TryParse(height, out var h);

    var res = h switch
    {
        > 2160 => "4K",
        > 1080 => "1440p",
        > 720 => "1080p",
        > 480 => "720p",
        _ => "SD"
    };

    return (res, codec == "HEVC" ? "x265" : codec?.ToLower() ?? "unknown");
}