namespace MediaRenamer.Core.Abstractions;

public interface IFilenameParserService
{
    string Normalize(string filename);
}