using System.Text.RegularExpressions;
using MediaRenamer.Core.Abstractions;
using MediaRenamer.Core.Extensions;
using MediaRenamer.Core.Models;

namespace MediaRenamer.Core.Services;

public class FilenameParserService : IFilenameParserService
{
    private static readonly List<string> NoisePatterns =
    [
        @"1080[p|i]", @"2160[p|i]", @"4k", @"720[p|i]", @"480[p|i]",
        @"x264", @"x265", @"h264", @"h265", @"hevc", @"avc", @"av1",
        @"ac3", @"eac3", @"dd5\.1", @"dd7\.1", @"dts", @"truehd",

        // Sprachen
        @"german", @"deutsch", @"german\.dubbed", @"dl\.german",
        @"english", @"eng", @"multi", @"forced",

        // Releases/Gruppen
        @"rarbg", @"yts", @"torrentgalaxy", @"blu", @"dvdrip", @"bdrip",
        @"web[-.]?(dl|rip)", @"proper", @"repack",

        // Dateigröße
        @"\d+\.?\d*\.?(gb|mb|kb)"
    ];

    private static readonly Regex YearRegex = new(@"\b(19|20)\d{2}\b", RegexOptions.Compiled);

    public ParsedMediaTitle Parse(string filename)
    {
        var normalized = filename.NormalizeForParsing();
        var (title, year) = ExtractTitleAndYear(normalized);
        var noise = ExtractNoise(filename, normalized);

        var confidence = CalculateConfidence(title.Length, year.HasValue, noise.Count);

        return new ParsedMediaTitle(
            RawFilename: filename,
            NormalizedTitle: title,
            Year: year,
            Type: DetectMediaType(title),
            Confidence: confidence,
            RemovedNoise: noise
        );
    }

    private (string title, int? year) ExtractTitleAndYear(string normalized)
    {
        normalized = Regex.Replace(normalized, @"^\s*\d+\.?\s*", "");

        foreach (var pattern in NoisePatterns)
        {
            normalized = Regex.Replace(normalized, $@"\b{pattern}\b.*?$", "", RegexOptions.IgnoreCase);
        }

        var yearMatch = YearRegex.Match(normalized);
        if (yearMatch.Success && int.TryParse(yearMatch.Value, out var year) && year >= 1900 && year <= 2099)
        {
            var titleEndIndex = yearMatch.Index;
            var title = normalized[..titleEndIndex].Trim();
            return (title, year);
        }

        return (normalized.Trim(), null);
    }

    private static List<string> ExtractNoise(string original, string normalized)
    {
        var noise = new List<string>();
        foreach (var pattern in NoisePatterns)
        {
            var match = Regex.Match(original, pattern, RegexOptions.IgnoreCase);
            if (match.Success) noise.Add(match.Value);
        }

        return noise.Distinct().ToList();
    }
    
    private static double CalculateConfidence(int titleLength, bool hasYear, int noiseCount)
    {
        var score = 0.5;  // Basis
        
        if (hasYear) score += 0.3;
        if (titleLength > 5 && titleLength < 100) score += 0.2;
        if (noiseCount > 0) score += 0.1;
        
        return Math.Max(0, Math.Min(1, score));
    }

    private static MediaType DetectMediaType(string title)
    {
        var lower = title.ToLower();
        if (Regex.IsMatch(lower, @"s\d+e\d+|staffel|season|episode")) return MediaType.Episode;
        // if (Regex.IsMatch(lower, @"staffel|season")) return MediaType.Episode;
        return MediaType.Movie;
    }
}