using MediaRenamer.Api.Background;
using MediaRenamer.Core.Abstractions;
using MediaRenamer.Core.Models;
using MediaRenamer.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace MediaRenamer.Api.UnitTests.Background;

public class MediaWatcherServiceTests
{
    private readonly ILogger<MediaWatcherService> _logger = Substitute.For<ILogger<MediaWatcherService>>();
    private readonly IMediaScanner _mediaScanner = Substitute.For<IMediaScanner>();
    private readonly IRenameService _renameService = Substitute.For<IRenameService>();
    private readonly IOptions<MediaSettings> _mediaSettings = Substitute.For<IOptions<MediaSettings>>();
    private readonly IProposalStore _proposalStore = Substitute.For<IProposalStore>();
    private readonly IServiceScopeFactory _serviceScopeFactory = Substitute.For<IServiceScopeFactory>();
    private readonly IMetadataProvider _metadataProvider = Substitute.For<IMetadataProvider>();
    private MediaWatcherService _mediaWatcherService;
    
    private string _testDirectory;
    private string _inputDirectory;
    private string _outputDirectory;

    [SetUp]
    public void SetUp()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"RenameServiceTests_{Guid.NewGuid()}");
        _inputDirectory = Path.Combine(_testDirectory, "input");
        _outputDirectory = Path.Combine(_testDirectory, "output");

        Directory.CreateDirectory(_inputDirectory);
        Directory.CreateDirectory(_outputDirectory);
        
        _mediaSettings.Value.Returns(new MediaSettings()
        {
            WatchPath = _inputDirectory,
            OutputPath = _outputDirectory,
            ScanInterval = 30
        });
        //_configuration.GetValue<int>("Media:ScanInterval").Returns(30);

        var serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider.GetService(typeof(IProposalStore)).Returns(_proposalStore);
        var scope = Substitute.For<IServiceScope>();
        scope.ServiceProvider.Returns(serviceProvider);
        _serviceScopeFactory.CreateScope().Returns(scope);

        List<IMetadataProvider> providers = [_metadataProvider];
        var resolver = new MetadataResolver(providers, Substitute.For<ILogger<MetadataResolver>>());
        _mediaWatcherService = new MediaWatcherService(_logger, _mediaScanner, resolver, _renameService, _mediaSettings,
            _serviceScopeFactory);
    }

    [TearDown]
    public void TearDown()
    {
        _mediaWatcherService.Dispose();
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, recursive: true);
        }
    }

    [Test]
    public async Task ProcessFile_ShouldSkipFile_WhenMetadataResolutionFails()
    {
        // Arrange
        var testFilePath = @$"{_inputDirectory}\Unknown.Movie.2024.mkv";
        var mediaFile = new MediaFile
        {
            OriginalPath = testFilePath,
            FileName = "Unknown.Movie.2024"
        };
        
        var testContent = "test file content";
        await File.WriteAllTextAsync(testFilePath, testContent);

        _mediaScanner.AnalyzeAsync(testFilePath).Returns(Task.FromResult(mediaFile));
        _metadataProvider.EnrichAsync(mediaFile).Returns(Task.FromResult<MediaFile?>(null));
        _proposalStore.GetPending().Returns(Task.FromResult(new List<RenameProposal>()));
        
        // Act
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2)); // Timeout!
        _mediaWatcherService.StartAsync(cts.Token);
    
        try
        {
            // 1 Sekunde warten, damit der Service die Datei verarbeitet
            await Task.Delay(1000, cts.Token);
        }
        finally
        {
            await _mediaWatcherService.StopAsync(cts.Token);
        }
        
        // Assert
        await _proposalStore.Received(1).Add(Arg.Is<RenameProposal>(p =>
            p.Status == ProposalStatus.Error));

        _renameService.DidNotReceive().CreateProposal(Arg.Any<MediaFile>());
    }
}