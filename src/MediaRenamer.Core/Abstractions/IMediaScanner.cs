using MediaRenamer.Core.Models;

namespace MediaRenamer.Core.Abstractions;

public interface IMediaScanner
{
    Task<MediaFile> AnalyzeAsync(string filePath);
}