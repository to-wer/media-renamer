using MediaRenamer.Core.Models;
using MediaRenamer.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;

namespace MediaRenamer.Core.UnitTests.Services;

public class RenameServiceTests
{
    private readonly IOptions<MediaSettings> _settings = Substitute.For<IOptions<MediaSettings>>();
    private readonly ILogger<RenameService> _logger = Substitute.For<ILogger<RenameService>>();
    private RenameService _renameService;

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

        var mediaSettings = new MediaSettings
        {
            WatchPath = _inputDirectory,
            OutputPath = _outputDirectory
        };
        _settings.Value.Returns(mediaSettings);

        _renameService = new RenameService(_settings, _logger);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, recursive: true);
        }
    }

    [Test]
    public async Task ExecuteAsync_ShouldMoveFile_WhenTargetFileDoesNotExist()
    {
        // Arrange
        var sourceFileName = "SourceFile.mkv";
        var targetFileName = "NewFileName";

        var sourceFilePath = Path.Combine(_testDirectory, sourceFileName);
        var targetFilePath = Path.Combine(_outputDirectory, targetFileName + ".mkv");

        var testContent = "test file content";
        await File.WriteAllTextAsync(sourceFilePath, testContent);

        var renameProposal = new RenameProposal
        {
            Id = Guid.NewGuid(),
            ProposedName = targetFileName,
            Source = new MediaFile
            {
                OriginalPath = sourceFilePath,
                FileName = sourceFileName
            },
            Status = ProposalStatus.Pending,
            ScanTime = DateTime.UtcNow
        };

        // Act
        await _renameService.ExecuteAsync(renameProposal);

        // Assert
        File.Exists(sourceFilePath).ShouldBeFalse("Source file should be moved");
        File.Exists(targetFilePath).ShouldBeTrue("Target file should exist");

        var movedContent = await File.ReadAllTextAsync(targetFilePath);
        movedContent.ShouldBe(testContent);
    }

    [Test]
    public async Task ExecuteAsync_ShouldSkipFile_WhenTargetFileAlreadyExists_AndSkipStrategy()
    {
        // Arrange
        var sourceFileName = "SourceFile.mkv";
        var targetFileName = "ExistingFileName";

        var sourceFilePath = Path.Combine(_inputDirectory, sourceFileName);
        var targetFilePath = Path.Combine(_outputDirectory, targetFileName + ".mkv");

        await File.WriteAllTextAsync(sourceFilePath, "source content");
        await File.WriteAllTextAsync(targetFilePath, "existing content");

        var renameProposal = new RenameProposal
        {
            Id = Guid.NewGuid(),
            ProposedName = targetFileName,
            Source = new MediaFile
            {
                OriginalPath = sourceFilePath,
                FileName = sourceFileName
            },
            Status = ProposalStatus.Pending,
            ScanTime = DateTime.UtcNow
        };

        // Act
        await _renameService.ExecuteAsync(renameProposal);

        // Assert
        File.Exists(sourceFilePath).ShouldBeTrue("Source file should still exist (not moved)");
        File.Exists(targetFilePath).ShouldBeTrue("Target file should still exist");
        var existingContent = await File.ReadAllTextAsync(targetFilePath);
        existingContent.ShouldBe("existing content", "Target file should not be modified");
    }

    [Test]
    public async Task ExecuteAsync_ShouldOverwriteFile_WhenTargetFileAlreadyExists_AndOverwriteStrategy()
    {
        // Arrange
        var sourceFileName = "SourceFile.mkv";
        var targetFileName = "ExistingFileName";

        var sourceFilePath = Path.Combine(_inputDirectory, sourceFileName);
        var targetFilePath = Path.Combine(_outputDirectory, targetFileName + ".mkv");

        await File.WriteAllTextAsync(sourceFilePath, "source content");
        await File.WriteAllTextAsync(targetFilePath, "existing content");

        var renameProposal = new RenameProposal
        {
            Id = Guid.NewGuid(),
            ProposedName = targetFileName,
            Source = new MediaFile
            {
                OriginalPath = sourceFilePath,
                FileName = sourceFileName
            },
            Status = ProposalStatus.Pending,
            ScanTime = DateTime.UtcNow
        };

        // Update settings to use Overwrite strategy
        var mediaSettings = new MediaSettings
        {
            WatchPath = _inputDirectory,
            OutputPath = _outputDirectory,
            DuplicateFileHandling = DuplicateFileHandling.Overwrite
        };
        _settings.Value.Returns(mediaSettings);
        _renameService = new RenameService(_settings, _logger);

        // Act
        await _renameService.ExecuteAsync(renameProposal);

        // Assert
        File.Exists(sourceFilePath).ShouldBeFalse("Source file should be moved");
        File.Exists(targetFilePath).ShouldBeTrue("Target file should exist");
        var overwrittenContent = await File.ReadAllTextAsync(targetFilePath);
        overwrittenContent.ShouldBe("source content", "Target file should be overwritten");
    }

    [Test]
    public async Task ExecuteAsync_ShouldRenameWithSuffix_WhenTargetFileAlreadyExists_AndRenameWithSuffixStrategy()
    {
        // Arrange
        var sourceFileName = "SourceFile.mkv";
        var targetFileName = "ExistingFileName";

        var sourceFilePath = Path.Combine(_inputDirectory, sourceFileName);
        var targetFilePath = Path.Combine(_outputDirectory, targetFileName + ".mkv");
        var expectedTargetPath = Path.Combine(_outputDirectory, targetFileName + "_1.mkv");

        await File.WriteAllTextAsync(sourceFilePath, "source content");
        await File.WriteAllTextAsync(targetFilePath, "existing content");

        var renameProposal = new RenameProposal
        {
            Id = Guid.NewGuid(),
            ProposedName = targetFileName,
            Source = new MediaFile
            {
                OriginalPath = sourceFilePath,
                FileName = sourceFileName
            },
            Status = ProposalStatus.Pending,
            ScanTime = DateTime.UtcNow
        };

        // Update settings to use RenameWithSuffix strategy
        var mediaSettings = new MediaSettings
        {
            WatchPath = _inputDirectory,
            OutputPath = _outputDirectory,
            DuplicateFileHandling = DuplicateFileHandling.RenameWithSuffix
        };
        _settings.Value.Returns(mediaSettings);
        _renameService = new RenameService(_settings, _logger);

        // Act
        await _renameService.ExecuteAsync(renameProposal);

        // Assert
        File.Exists(sourceFilePath).ShouldBeFalse("Source file should be moved");
        File.Exists(targetFilePath).ShouldBeTrue("Original target file should still exist");
        File.Exists(expectedTargetPath).ShouldBeTrue("New file with suffix should exist");
        var movedContent = await File.ReadAllTextAsync(expectedTargetPath);
        movedContent.ShouldBe("source content", "File should be moved with suffix");
    }

    [Test]
    public async Task ExecuteAsync_ShouldHandleSubdirectories_WhenProposedNameContainsPath()
    {
        // Arrange
        var sourceFileName = "SeriesName.S01E01.mkv";
        var proposedName = "SeriesName - S01E01 - EpisodeTitle";

        var sourceFilePath = Path.Combine(_inputDirectory, sourceFileName);
        var targetFilePath = Path.Combine(_outputDirectory, proposedName + ".mkv");

        await File.WriteAllTextAsync(sourceFilePath, "episode content");

        var renameProposal = new RenameProposal
        {
            Id = Guid.NewGuid(),
            ProposedName = proposedName,
            Source = new MediaFile
            {
                OriginalPath = sourceFilePath,
                FileName = sourceFileName
            },
            Status = ProposalStatus.Pending,
            ScanTime = DateTime.UtcNow
        };

        // Act
        await _renameService.ExecuteAsync(renameProposal);

        // Assert
        File.Exists(targetFilePath).ShouldBeTrue("Target file should exist in subdirectory");
        Directory.Exists(Path.GetDirectoryName(targetFilePath)).ShouldBeTrue("Subdirectory should be created");
    }
}