using MediaRenamer.Core.Abstractions;
using MediaRenamer.Core.Models;
using Microsoft.Extensions.Logging;

namespace MediaRenamer.Core.Services;

public class MetadataResolver(IEnumerable<IMetadataProvider> providers, ILogger<MetadataResolver> logger)
{
    public async Task<MediaFile?> ResolveAsync(MediaFile file)
    {
        if (!providers.Any())
        {
            logger.LogWarning("No metadata providers registered - skipping enrichment");
            return file;
        }

        foreach (var provider in providers)
        {
            var result = await provider.EnrichAsync(file);
            if (result?.Title != null)
                return result;
        }

        return null;
    }
}