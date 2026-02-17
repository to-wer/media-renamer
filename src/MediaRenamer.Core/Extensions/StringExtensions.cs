using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace MediaRenamer.Core.Extensions;

public static class StringExtensions
{
    /// <summary>
    /// Characters that should be replaced with spaces during normalization.
    /// </summary>
    private static readonly HashSet<char> Separators = ['.', '-', '_', ' ', '(', ')', '[', ']', '{', '}'];
    
    /// <summary>
    /// Characters that should be completely removed during normalization.
    /// </summary>
    private static readonly HashSet<char> RemovableCharacters = ['"', '\'', '`', '~', '^', '°', '§', '$', '%', '&', '/', '\\', '|', '<', '>', '*', '+', '=', '?', '!', ',', ';', ':', '@', '#'];
    
    /// <summary>
    /// Regex for normalizing Unicode characters (combining diacritical marks, etc.)
    /// </summary>
    private static readonly Regex UnicodeNormalizationRegex = new(
        @"[\u0300-\u036f\u0483-\u0489\u1dc0-\u1dff\u20d0-\u20ff\ufe20-\ufe2f]",
        RegexOptions.Compiled);
    
    /// <summary>
    /// Regex for multiple spaces (including various Unicode space characters).
    /// </summary>
    private static readonly Regex MultipleSpacesRegex = new(
        @"[ \u00A0\u1680\u2000-\u200A\u202F\u205F\u3000\uFEFF]+",
        RegexOptions.Compiled);

    /// <summary>
    /// Replaces multiple consecutive space characters with a single space.
    /// </summary>
    /// <param name="input">The input string.</param>
    /// <returns>A string with multiple spaces replaced by a single space.</returns>
    public static string ReplaceMultipleSpaces(this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;
            
        return MultipleSpacesRegex.Replace(input, " ").Trim();
    }
    
    /// <summary>
    /// Normalizes a filename for parsing by:
    /// - Converting to lowercase
    /// - Replacing common separators with spaces
    /// - Removing special characters
    /// - Normalizing Unicode characters
    /// - Collapsing multiple spaces
    /// </summary>
    /// <param name="filename">The filename to normalize.</param>
    /// <returns>A normalized filename suitable for parsing.</returns>
    public static string NormalizeForParsing(this string filename)
    {
        if (string.IsNullOrWhiteSpace(filename))
            return string.Empty;
            
        // Use string builder for efficient string manipulation
        var result = new StringBuilder(filename.Length);
        
        foreach (var c in filename)
        {
            if (Separators.Contains(c))
            {
                result.Append(' ');
            }
            else if (RemovableCharacters.Contains(c))
            {
                // Skip special characters
            }
            else
            {
                result.Append(c);
            }
        }
        
        // Normalize Unicode characters (decompose and normalize)
        var normalized = result.ToString().Normalize(NormalizationForm.FormD);
        
        // Remove combining diacritical marks
        normalized = UnicodeNormalizationRegex.Replace(normalized, "");
        
        // Convert to lowercase using invariant culture for consistent behavior
        normalized = normalized.ToLowerInvariant();
        
        // Replace multiple spaces and trim
        return normalized.ReplaceMultipleSpaces();
    }
    
    /// <summary>
    /// Normalizes a string for display purposes (preserves more characters than NormalizeForParsing).
    /// </summary>
    /// <param name="input">The input string.</param>
    /// <returns>A normalized string for display.</returns>
    public static string NormalizeForDisplay(this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;
            
        // Just collapse spaces and trim for display
        return input.ReplaceMultipleSpaces();
    }
    
    public static string ExtractSeriesName(this string filename)
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
