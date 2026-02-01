namespace MediaRenamer.Core.Models;

public record ParsedMediaTitle(
    string RawFilename,
    string NormalizedTitle,
    int? Year,
    MediaType Type,
    double Confidence,
    List<string> RemovedNoise
);