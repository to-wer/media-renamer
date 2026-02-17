using MediaRenamer.Core.Abstractions;
using MediaRenamer.Core.Models;

namespace MediaRenamer.Core.Providers;

public class NoOpMetadataProvider : IMetadataProvider
{
    public Task<MediaFile?> EnrichAsync(MediaFile file)
    {
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
        return Task.FromResult(file);
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
    }
}