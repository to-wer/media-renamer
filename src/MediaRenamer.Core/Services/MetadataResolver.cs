using MediaRenamer.Core.Abstractions;
using MediaRenamer.Core.Models;

namespace MediaRenamer.Core.Services;

public class MetadataResolver
{
    private readonly IEnumerable<IMetadataProvider> _providers;

    public MetadataResolver(IEnumerable<IMetadataProvider> providers)
    {
        _providers = providers;
    }

    public async Task<MediaFile?> ResolveAsync(MediaFile file)
    {
        foreach (var provider in _providers)
        {
            var result = await provider.EnrichAsync(file);
            if (result?.Title != null)
                return result;
        }

        return null;
    }
}