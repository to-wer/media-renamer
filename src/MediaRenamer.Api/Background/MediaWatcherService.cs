using System.Collections.Concurrent;
using MediaRenamer.Core.Abstractions;
using MediaRenamer.Core.Services;

namespace MediaRenamer.Api.Background;

public class MediaWatcherService : BackgroundService
{
    private readonly ILogger<MediaWatcherService> _logger;
    private readonly IMediaScanner _scanner;
    private readonly MetadataResolver _resolver;
    private readonly IRenameService _renamer;

    private readonly string _watchPath;
    private readonly TimeSpan _scanInterval = TimeSpan.FromSeconds(30);
    private readonly ConcurrentDictionary<string, bool> _processedFiles = new();

    public MediaWatcherService(
        ILogger<MediaWatcherService> logger,
        IMediaScanner scanner,
        MetadataResolver resolver,
        IRenameService renamer,
        IConfiguration config)
    {
        _logger = logger;
        _scanner = scanner;
        _resolver = resolver;
        _renamer = renamer;

        _watchPath = config["Media:WatchPath"] ?? "/media/incoming";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("MediaWatcherService started. Watching: {path}", _watchPath);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var files = Directory
                    .GetFiles(_watchPath, "*.*", SearchOption.AllDirectories)
                    .Where(f => f.EndsWith(".mkv") || f.EndsWith(".mp4"));

                foreach (var file in files)
                {
                    if (_processedFiles.ContainsKey(file))
                        continue;

                    _logger.LogInformation("New file detected: {file}", file);

                    var mediaFile = await _scanner.AnalyzeAsync(file);
                    var enriched = await _resolver.ResolveAsync(mediaFile);

                    var proposal = _renamer.CreateProposal(enriched!);

                    // TODO: optional: speichere Proposal in DB oder Queue für UI
                    _logger.LogInformation("Proposal created: {proposal}", proposal.ProposedName);

                    _processedFiles.TryAdd(file, true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MediaWatcherService");
            }

            await Task.Delay(_scanInterval, stoppingToken);
        }
    }
}