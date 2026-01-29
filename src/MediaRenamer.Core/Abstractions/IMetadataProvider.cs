using MediaRenamer.Core.Models;

namespace MediaRenamer.Core.Abstractions;

public interface IMetadataProvider
{
    Task<MediaFile?> EnrichAsync(MediaFile file);
}