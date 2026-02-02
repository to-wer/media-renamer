using MediaRenamer.Core.Abstractions;
using MediaRenamer.Core.Models;

namespace MediaRenamer.Core.Providers;

public class NoOpMetadataProvider : IMetadataProvider
{
    public Task<MediaFile?> EnrichAsync(MediaFile file)
    {
        return Task.FromResult(file);
    }
}