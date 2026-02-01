using MediaRenamer.Core.Services;
using Shouldly;

namespace MediaRenamer.Core.UnitTests.Services;

public class FilenameParserServiceTests
{
    private readonly FilenameParserService _parser = new FilenameParserService();
    
    [Test]
    public void ParsesJamesBondCorrectly()
    {
        // Arrange
        
        // Act
        string normalized = _parser.Normalize("001.James.Bond.007.-.Jagt.Dr.No.1962.German.AC3.DL.mkv");
        var result = _parser.ExtractTitleAndYear(normalized);
    
        // Assert
        result.title.ShouldBe("001 james bond 007 jagt dr no");
        result.year.ShouldBe(1962);
    }
}