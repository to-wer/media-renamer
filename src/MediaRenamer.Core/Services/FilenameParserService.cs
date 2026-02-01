using System.Text.RegularExpressions;
using MediaRenamer.Core.Abstractions;
using MediaRenamer.Core.Extensions;

namespace MediaRenamer.Core.Services;

public class FilenameParserService : IFilenameParserService
{
    private readonly List<string> _noisePatterns =
    [
        // Qualität/Codec
        @"1080[p|i]", @"2160[p|i]", @"4K", @"720[p|i]", @"480[p|i]",
        @"x264", @"x265", @"h264", @"h265", @"hevc", @"avc", @"av1",
        @"ac3", @"eac3", @"dd5.1", @"dd7.1", @"dts", @"truehd",

        // Sprachen
        @"german", @"deutsch", @"english", @"german.dubbed", @"dl.german",

        // Release-Gruppen (häufig am Ende)
        @"rarbg", @"torrentgalaxy", @"yts", @"blu", @"dvdrip", @"bdrip",

        // Sonstiges
        @"\d+\.?\d*\.?(gb|mb|kb)", @"web[-.]?(dl|rip)", @"proper"
    ];

    public string Normalize(string filename)
    {
        return filename
            .ToLowerInvariant()
            .Replace('.', ' ')
            .Replace('-', ' ')
            .Replace('_', ' ')
            .ReplaceMultipleSpaces()
            .Trim();
    }

    public (string title, int? year) ExtractTitleAndYear(string normalized)
    {
        foreach (var pattern in _noisePatterns)
        {
            normalized = Regex.Replace(normalized, $@"\b{pattern}\b.*$", "", RegexOptions.IgnoreCase);
        }

        var yearMatch = Regex.Match(normalized, @"\b(19|20)\d{2}\b");
        var year = yearMatch.Success ? int.Parse(yearMatch.Value) : (int?)null;

        if (!year.HasValue) return (normalized, null);
        var title = normalized.Substring(0, yearMatch.Index).Trim();
        return (title, year);
    }
}