using MediaRenamer.Core.Models;

namespace MediaRenamer.Core.Abstractions;

public interface IFilenameParserService
{
    ParsedMediaTitle Parse(string filename);
}