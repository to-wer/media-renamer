using System.Text.RegularExpressions;
using MediaRenamer.Core.Abstractions;
using MediaRenamer.Core.Extensions;
using MediaRenamer.Core.Models;
using Microsoft.Extensions.Logging;

namespace MediaRenamer.Core.Services;

/// <summary>
/// Service for parsing media filenames using configurable patterns.
/// </summary>
public class FilenameParserService : IFilenameParserService
{
    private readonly ILogger<FilenameParserService> _logger;
    private readonly ParserConfiguration _configuration;
    private readonly Lazy<Regex> _yearRegex;
    private readonly Lazy<Regex> _episodeRegex;

    /// <summary>
    /// Creates a new instance of FilenameParserService with default configuration.
    /// </summary>
    public FilenameParserService(ILogger<FilenameParserService> logger) : this(logger, ParserConfiguration.GetDefaultConfiguration())
    {
    }

    /// <summary>
    /// Creates a new instance of FilenameParserService with custom configuration.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="configuration">The parser configuration to use.</param>
    public FilenameParserService(ILogger<FilenameParserService> logger, ParserConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        
        // Pre-compile commonly used regexes for better performance
        _yearRegex = new Lazy<Regex>(() => new Regex(@"\b(19|20)\d{2}\b", RegexOptions.Compiled));
        _episodeRegex = new Lazy<Regex>(() => new Regex(@"s\d+e\d+|staffel\s*\d+|season\s*\d+|folge\s*\d+|episode\s*\d+", RegexOptions.Compiled | RegexOptions.IgnoreCase));
    }

    public ParsedMediaTitle Parse(string filename)
    {
        if (string.IsNullOrWhiteSpace(filename))
        {
            return new ParsedMediaTitle(
                RawFilename: filename,
                NormalizedTitle: string.Empty,
                Year: null,
                Type: MediaType.Movie,
                Confidence: 0,
                RemovedNoise: new List<string>()
            );
        }

        var normalized = filename.NormalizeForParsing();
        var (title, year) = ExtractTitleAndYear(normalized);
        var noise = ExtractNoise(filename);

        var confidence = CalculateConfidence(title.Length, year.HasValue, noise.Count);

        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation(
                "Parsed filename '{Filename}' to title '{Title}' (Year: {Year}, Confidence: {Confidence:P2})",
                filename, title, year.HasValue ? year.ToString() : "N/A", confidence);

        return new ParsedMediaTitle(
            RawFilename: filename,
            NormalizedTitle: title,
            Year: year,
            Type: DetectMediaType(title, filename),
            Confidence: confidence,
            RemovedNoise: noise
        );
    }

    private (string title, int? year) ExtractTitleAndYear(string normalized)
    {
        // Remove leading numbering (e.g., "01.", "1. ")
        normalized = Regex.Replace(normalized, @"^\s*\d+\.?\s*", "");

        // Get patterns that should be removed from title, sorted by priority
        var removablePatterns = _configuration.Patterns
            .Where(p => p is { IsEnabled: true, RemoveFromTitle: true })
            .OrderBy(p => p.Priority)
            .ToList();

        normalized = removablePatterns.Aggregate(normalized, ApplyPatternReplacement);

        // Extract year
        var yearMatch = _yearRegex.Value.Match(normalized);
        if (yearMatch.Success && int.TryParse(yearMatch.Value, out var year) && year >= 1900 && year <= 2099)
        {
            var titleEndIndex = yearMatch.Index;
            var title = normalized[..titleEndIndex].Trim();
            return (title, year);
        }

        return (normalized.Trim(), null);
    }

    private string ApplyPatternReplacement(string input, ParserPattern pattern)
    {
        try
        {
            var options = RegexOptions.IgnoreCase | RegexOptions.Compiled;
            return Regex.Replace(input, $@"\b{pattern.Pattern}\b.*?$", "", options);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid regex pattern '{Pattern}' for pattern '{Id}'", pattern.Pattern, pattern.Id);
            return input;
        }
    }

    private List<string> ExtractNoise(string original)
    {
        var noise = new List<string>();
        var processedPatterns = new HashSet<string>();

        foreach (var pattern in _configuration.Patterns.Where(p => p.IsEnabled))
        {
            try
            {
                var options = pattern.CaseInsensitive ? RegexOptions.IgnoreCase : RegexOptions.None;
                var match = Regex.Match(original, pattern.Pattern, options);
                
                if (match.Success && !processedPatterns.Contains(match.Value))
                {
                    noise.Add(match.Value);
                    processedPatterns.Add(match.Value);
                }
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid regex pattern '{Pattern}' for pattern '{Id}'", pattern.Pattern, pattern.Id);
            }
        }

        return noise.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static double CalculateConfidence(int titleLength, bool hasYear, int noiseCount)
    {
        var score = 0.5; // Base score

        if (hasYear) score += 0.3;
        if (titleLength > 5 && titleLength < 100) score += 0.2;
        if (noiseCount > 0) score += 0.1;

        return Math.Max(0, Math.Min(1, score));
    }

    private MediaType DetectMediaType(string title, string originalFilename)
    {
        // Check for episode patterns in the original filename (preserves case)
        if (_episodeRegex.Value.IsMatch(originalFilename))
        {
            return MediaType.Episode;
        }

        // Also check normalized title
        var lower = title.ToLower();
        if (Regex.IsMatch(lower, @"s\d+e\d+|staffel|season|episode|folge"))
        {
            return MediaType.Episode;
        }

        return MediaType.Movie;
    }
}
