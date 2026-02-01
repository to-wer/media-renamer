using System.Text.RegularExpressions;

namespace MediaRenamer.Core.Extensions;

public static class StringExtensions
{
    public static string ReplaceMultipleSpaces(this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;
            
        return Regex.Replace(input, @"\s+", " ").Trim();
    }
    
    public static string NormalizeForParsing(this string filename)
    {
        return filename
            .ToLowerInvariant()
            .Replace('.', ' ')
            .Replace('-', ' ')
            .Replace('_', ' ')
            .ReplaceMultipleSpaces()
            .Trim();
    }
}