using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using MediaRenamer.Core.Abstractions;
using MediaRenamer.Core.Models;

namespace MediaRenamer.Core.Services;

public class MediaScanner : IMediaScanner
{
    public async Task<MediaFile> AnalyzeAsync(string filePath)
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
            // FFprobe JSON-Ausgabe: Erstes Video-Stream für Resolution/Codec
            var ffprobeArgs = "-v error -select_streams v:0 -show_entries stream=width,height,codec_name -of json";
            var jsonOutput = await RunFfprobeAsync(filePath, ffprobeArgs);

            using var doc = JsonDocument.Parse(jsonOutput);
            var root = doc.RootElement;
            if (root.TryGetProperty("streams", out var streams) && streams.GetArrayLength() > 0)
            {
                var videoStream = streams[0];
                // mediaFile.Width = videoStream.GetProperty("width").GetInt32();
                int height = videoStream.GetProperty("height").GetInt32();
                media.Resolution = height switch
                {
                    >= 2160 => "4K",
                    >= 1080 => "1080p",
                    >= 720 => "720p",
                    _ => "SD"
                };
                string codecName = videoStream.GetProperty("codec_name").GetString() ?? "unknown";
                media.Codec = codecName == "hevc" ? "x265" : codecName;
            }
        }
        catch
        {
            /* fallback */
        }

        return media;
    }

    private static async Task<string> RunFfprobeAsync(string filePath, string arguments)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "ffprobe",
            Arguments = $"{arguments} \"{filePath}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = psi };
        process.Start();

        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            Console.WriteLine($"FFprobe error: {error}");
            return string.Empty;
        }

        return output;
    }
}