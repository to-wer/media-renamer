using MediaRenamer.Core.Models;

namespace MediaRenamer.Core.Abstractions;

/// <summary>
/// Interface for parsing media filenames.
/// </summary>
public interface IFilenameParserService
{
    /// <summary>
    /// Parses a filename and extracts media title information.
    /// </summary>
    /// <param name="filename">The filename to parse.</param>
    /// <returns>A parsed media title object.</returns>
    ParsedMediaTitle Parse(string filename);
}
