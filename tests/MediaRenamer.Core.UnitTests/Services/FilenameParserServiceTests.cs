using MediaRenamer.Core.Services;
using Shouldly;

namespace MediaRenamer.Core.UnitTests.Services;

public class FilenameParserServiceTests
{
    private readonly FilenameParserService _parser = new FilenameParserService();
    
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