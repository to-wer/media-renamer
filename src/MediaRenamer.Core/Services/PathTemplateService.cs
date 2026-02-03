using System.Text.RegularExpressions;
using MediaRenamer.Core.Models;

namespace MediaRenamer.Core.Services;

public class PathTemplateService
{
    /// <summary>
    /// Ersetzt Platzhalter im Template mit tatsächlichen Werten
    /// </summary>
    public static string RenderTemplate(string template, MediaFile file)
    {
        var result = template;

        // Ersetze alle Platzhalter
        result = RenderPlaceholder(result, "SeriesName", file.Title);
        result = RenderPlaceholder(result, "Title", file.Title);
        result = RenderPlaceholder(result, "Year", file.Year?.ToString() ?? "0000");
        result = RenderPlaceholder(result, "EpisodeName", file.EpisodeTitle);
        result = RenderPlaceholder(result, "Resolution", file.Resolution);
        result = RenderPlaceholder(result, "Codec", file.Codec);
        
        // Format-Specifier für Zahlen (z.B. {Season:D2} → 01)
        result = RenderNumberPlaceholder(result, "Season", file.Season);
        result = RenderNumberPlaceholder(result, "Episode", file.Episode);

        // Entferne leere Klammern und überflüssige Leerzeichen
        result = CleanupPath(result);

        return result;
    }

    private static string RenderPlaceholder(string template, string key, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return template.Replace($"{{{key}}}", "");
        
        return template.Replace($"{{{key}}}", SanitizeFileName(value));
    }

    private static string RenderNumberPlaceholder(string template, string key, int? value)
    {
        if (!value.HasValue)
            return template.Replace($"{{{key}:D2}}", "").Replace($"{{{key}}}", "");

        // Regex: {Season:D2} → formatiere mit D2
        var pattern = $@"\{{{key}:D(\d+)\}}";
        var match = Regex.Match(template, pattern);
        
        if (match.Success)
        {
            var width = int.Parse(match.Groups[1].Value);
            return Regex.Replace(template, pattern, value.Value.ToString($"D{width}"));
        }

        // Fallback ohne Format
        return template.Replace($"{{{key}}}", value.Value.ToString());
    }

    private static string CleanupPath(string path)
    {
        // Entferne leere Klammern: [] oder ()
        path = Regex.Replace(path, @"\[\s*\]", "");
        path = Regex.Replace(path, @"\(\s*\)", "");
        
        // Mehrfache Leerzeichen entfernen
        path = Regex.Replace(path, @"\s+", " ");
        
        // Leerzeichen vor/nach Schrägstrichen entfernen
        path = Regex.Replace(path, @"\s*/\s*", "/");
        
        return path.Trim();
    }

    private static string SanitizeFileName(string fileName)
    {
        // Entferne ungültige Zeichen für Dateinamen
        var invalid = Path.GetInvalidFileNameChars();
        foreach (var c in invalid)
        {
            fileName = fileName.Replace(c, '_');
        }
        return fileName.Trim();
    }
}