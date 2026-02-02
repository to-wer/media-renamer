using MediaRenamer.Core.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;

namespace MediaRenamer.Core.UnitTests.Services;

public class FilenameParserServiceTests
{
    private readonly ILogger<FilenameParserService> _logger = Substitute.For<ILogger<FilenameParserService>>();
    private FilenameParserService _parser;

    [SetUp]
    public void SetUp()
    {
        _parser = new FilenameParserService(_logger);
    }
    
    [Test]
    [TestCase("The.Matrix.1999.1080p.BluRay.x264", "the matrix", 1999)]
    [TestCase("001.James.Bond.007.-.Jagt.Dr.No.1962.German.AC3.DL.720p.Bluray.Rip.x264", "001 james bond 007 jagt dr no", 1962)]
    public void ParsesJamesBondCorrectly(string filePath, string expectedName, int expectedYear)
    {
        // Arrange
        
        // Act
        var result = _parser.Parse(filePath);
    
        // Assert
        result.NormalizedTitle.ShouldBe(expectedName);
        result.Year.ShouldBe(expectedYear);
    }
}