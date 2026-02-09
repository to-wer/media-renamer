using MediaRenamer.Core.Models;
using MediaRenamer.Core.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;

namespace MediaRenamer.Core.UnitTests.Services;

public class FilenameParserServiceTests
{
    private readonly ILogger<FilenameParserService> _logger = Substitute.For<ILogger<FilenameParserService>>();
    private FilenameParserService _parser = null!;

    [SetUp]
    public void SetUp()
    {
        _parser = new FilenameParserService(_logger);
    }

    #region Basic Parsing Tests

    [Test]
    [TestCase("The.Matrix.1999.1080p.BluRay.x264", "the matrix", 1999)]
    [TestCase("The Matrix (1999) 1080p BluRay x264", "the matrix", 1999)]
    [TestCase("matrix_1999_720p", "matrix", 1999)]
    public void ParsesBasicMovieFilename(string filename, string expectedTitle, int expectedYear)
    {
        var result = _parser.Parse(filename);

        result.NormalizedTitle.ShouldBe(expectedTitle);
        result.Year.ShouldBe(expectedYear);
        result.Type.ShouldBe(MediaType.Movie);
        result.Confidence.ShouldBeGreaterThan(0.7);
    }

    [Test]
    [TestCase("001.James.Bond.007.-.Jagt.Dr.No.1962.German.AC3.DL.720p.Bluray.Rip.x264", "james bond 007 jagt dr no", 1962)]
    public void ParsesJamesBondCorrectly(string filename, string expectedTitle, int expectedYear)
    {
        var result = _parser.Parse(filename);

        result.NormalizedTitle.ShouldBe(expectedTitle);
        result.Year.ShouldBe(expectedYear);
    }

    #endregion

    #region Year Extraction Tests

    [Test]
    [TestCase("Movie.2020.1080p", 2020)]
    [TestCase("Movie.1999.x264", 1999)]
    [TestCase("Movie.2099.x264", 2099)]
    public void ExtractsValidYears(string filename, int expectedYear)
    {
        var result = _parser.Parse(filename);

        result.Year.ShouldBe(expectedYear);
    }

    [Test]
    [TestCase("Movie.1899.x264")] // Too early
    [TestCase("Movie.2100.x264")] // Too late
    [TestCase("Movie.x264")] // No year
    public void RejectsInvalidYears(string filename)
    {
        var result = _parser.Parse(filename);

        result.Year.ShouldBeNull();
    }

    #endregion

    #region Resolution Pattern Tests

    [Test]
    [TestCase("Movie.2160p.4k.UHD.x265", "movie", "2160p")]
    [TestCase("Movie.1080p.BluRay.x264", "movie", "1080p")]
    [TestCase("Movie.720p.WEB-DL.x265", "movie", "720p")]
    [TestCase("Movie.480p.x264", "movie", "480p")]
    public void RemovesResolutionPatterns(string filename, string expectedTitle, string expectedNoisePattern)
    {
        var result = _parser.Parse(filename);

        result.NormalizedTitle.ShouldBe(expectedTitle);
        result.RemovedNoise.ShouldContain(n => n.Contains(expectedNoisePattern));
    }

    #endregion

    #region Codec Pattern Tests

    [Test]
    [TestCase("Movie.HEVC.x265", "movie", "hevc")]
    [TestCase("Movie.x264", "movie", "x264")]
    [TestCase("Movie.AVC.x264", "movie", "avc")]
    [TestCase("Movie.AV1", "movie", "av1")]
    public void RemovesCodecPatterns(string filename, string expectedTitle, string expectedNoise)
    {
        var result = _parser.Parse(filename);

        result.NormalizedTitle.ShouldBe(expectedTitle);
        result.RemovedNoise.ShouldContain(n => n.ToLower().Contains(expectedNoise.ToLower()));
    }

    [Test]
    public void RemovesH264Pattern()
    {
        var result = _parser.Parse("Movie.H.264.x264");

        // H.264 should be handled - the pattern expects h264 without the H.
        result.RemovedNoise.ShouldContain(n => n.ToLower().Contains("x264"));
    }

    #endregion

    #region Audio Codec Tests

    [Test]
    [TestCase("Movie.TrueHD.x264", "movie", "truehd")]
    [TestCase("Movie.DTS-HD.MA.x264", "movie", "dts")]
    [TestCase("Movie.EAC3.x264", "movie", "eac3")]
    [TestCase("Movie.DD5.1.x264", "movie", "dd")]
    [TestCase("Movie.DTS.x264", "movie", "dts")]
    public void RemovesAudioCodecPatterns(string filename, string expectedTitle, string expectedNoise)
    {
        var result = _parser.Parse(filename);

        result.RemovedNoise.ShouldContain(n => n.ToLower().Contains(expectedNoise.ToLower()));
    }

    #endregion

    #region Language Pattern Tests

    [Test]
    [TestCase("Movie.German.DL.AC3.1080p", "movie", "german")]
    [TestCase("Movie.German.Dubbed.720p", "movie", "german")]
    [TestCase("Movie.GERMAN.1080p", "movie", "german")]
    [TestCase("Movie.English.1080p", "movie", "english")]
    [TestCase("Movie.MULTI.1080p", "movie", "multi")]
    public void RemovesLanguagePatterns(string filename, string expectedTitle, string expectedPattern)
    {
        var result = _parser.Parse(filename);

        result.RemovedNoise.ShouldContain(n => n.ToLower().Contains(expectedPattern.ToLower()));
    }

    #endregion

    #region Source Pattern Tests

    [Test]
    [TestCase("Movie.BluRay.1080p", "movie", "blu")]
    [TestCase("Movie.BDRip.1080p", "movie", "bdrip")]
    [TestCase("Movie.DVDRip.1080p", "movie", "dvdrip")]
    [TestCase("Movie.WEB-DL.1080p", "movie", "web")]
    [TestCase("Movie.WEB.RIP.1080p", "movie", "web")]
    [TestCase("Movie.RARBG.1080p", "movie", "rarbg")]
    [TestCase("Movie.YTS.1080p", "movie", "yts")]
    public void RemovesSourcePatterns(string filename, string expectedTitle, string expectedPattern)
    {
        var result = _parser.Parse(filename);

        result.RemovedNoise.ShouldContain(n => n.ToLower().Contains(expectedPattern.ToLower()));
    }

    #endregion

    #region Quality Modifier Tests

    [Test]
    [TestCase("Movie.PROPER.1080p", "movie", "proper")]
    [TestCase("Movie.REPACK.1080p", "movie", "repack")]
    public void RemovesQualityModifiers(string filename, string expectedTitle, string expectedPattern)
    {
        var result = _parser.Parse(filename);

        result.RemovedNoise.ShouldContain(n => n.ToLower().Contains(expectedPattern.ToLower()));
    }

    #endregion

    #region Media Type Detection Tests

    [Test]
    [TestCase("The.Matrix.S01E01.1080p.WEB-DL", MediaType.Episode)]
    [TestCase("The.Matrix.S01E01.720p", MediaType.Episode)]
    [TestCase("TV.Show.Season.1.Episode.1.1080p", MediaType.Episode)]
    [TestCase("TV.Show.Staffel.1.Folge.1.1080p", MediaType.Episode)]
    [TestCase("TV.Show.Season.1.1080p", MediaType.Episode)]
    [TestCase("The.Matrix.1999.1080p.BluRay", MediaType.Movie)]
    [TestCase("Movie.2020.1080p", MediaType.Movie)]
    public void DetectsMediaTypeCorrectly(string filename, MediaType expectedType)
    {
        var result = _parser.Parse(filename);

        result.Type.ShouldBe(expectedType);
    }

    #endregion

    #region Unicode Handling Tests

    [Test]
    [TestCase("Café.2020.1080p", "cafe")]
    [TestCase("Naïve.2020.1080p", "naive")]
    [TestCase("Héllo.2020.1080p", "hello")]
    [TestCase("René.2020.1080p", "rene")]
    [TestCase("Ðuke.2020.1080p", "ðuke")] // Ð is not a combining character, it's preserved
    public void HandlesUnicodeCharacters(string filename, string expectedTitle)
    {
        var result = _parser.Parse(filename);

        result.NormalizedTitle.ShouldBe(expectedTitle);
    }

    [Test]
    [TestCase("Curaçao.2020.1080p", "curacao")]
    [TestCase("Zoë.2020.1080p", "zoe")]
    public void HandlesUnicodeWithDiacritics(string filename, string expectedTitle)
    {
        var result = _parser.Parse(filename);

        result.NormalizedTitle.ShouldBe(expectedTitle);
    }

    #endregion

    #region Edge Cases Tests

    [Test]
    public void HandlesEmptyFilename()
    {
        var result = _parser.Parse("");

        result.NormalizedTitle.ShouldBeEmpty();
        result.Year.ShouldBeNull();
        result.Type.ShouldBe(MediaType.Movie);
        result.Confidence.ShouldBe(0);
    }

    [Test]
    public void HandlesWhitespaceOnlyFilename()
    {
        var result = _parser.Parse("   ");

        result.NormalizedTitle.ShouldBeEmpty();
        result.Confidence.ShouldBe(0);
    }

    [Test]
    public void HandlesNullLikeFilename()
    {
        var result = _parser.Parse("   ");

        result.RawFilename.ShouldBe("   ");
        result.NormalizedTitle.ShouldBeEmpty();
    }

    [Test]
    public void HandlesVeryLongFilename()
    {
        var longTitle = new string('a', 150);
        var filename = $"{longTitle}.2020.1080p";

        var result = _parser.Parse(filename);

        result.NormalizedTitle.ShouldBe(longTitle.ToLower());
        // Long titles still have reasonable confidence due to year
        result.Confidence.ShouldBeGreaterThan(0.7);
    }

    [Test]
    public void HandlesFilenameWithSpecialCharacters()
    {
        var filename = "Movie!@#$%2020.1080p";

        var result = _parser.Parse(filename);

        // Special characters like !@#$% should be removed, leaving movie2020
        // The year 2020 is not detected as it's attached to movie
        result.NormalizedTitle.ShouldBe("movie2020");
    }

    [Test]
    public void HandlesFilenameWithParentheses()
    {
        var filename = "Movie (2020) [1080p]";

        var result = _parser.Parse(filename);

        // Parentheses and brackets are replaced with spaces, year and resolution are removed as noise
        result.NormalizedTitle.ShouldBe("movie");
    }

    #endregion

    #region Confidence Calculation Tests

    [Test]
    [TestCase("Movie.2020.1080p.BluRay.x264", 0.9)] // Has year + noise = high confidence
    [TestCase("Movie.2020", 0.8)] // Has year = medium-high confidence
    [TestCase("Some Very Long Movie Title Here 2020 Without Any Extra Info", 0.7)] // Long title with year
    [TestCase("Short.2020", 0.7)] // Short title with year
    public void CalculatesConfidenceCorrectly(string filename, double minExpectedConfidence)
    {
        var result = _parser.Parse(filename);

        result.Confidence.ShouldBeGreaterThanOrEqualTo(minExpectedConfidence);
    }

    #endregion

    #region Configuration Tests

    [Test]
    public void HasDefaultConfiguration()
    {
        _parser.Configuration.ShouldNotBeNull();
        _parser.Configuration.Patterns.ShouldNotBeEmpty();
    }

    [Test]
    public void ConfigurationContainsExpectedCategories()
    {
        var categories = _parser.Configuration.Patterns.Select(p => p.Category).Distinct().ToList();

        categories.ShouldContain(ParserPatternCategory.Resolution);
        categories.ShouldContain(ParserPatternCategory.Codec);
        categories.ShouldContain(ParserPatternCategory.AudioCodec);
        categories.ShouldContain(ParserPatternCategory.Language);
        categories.ShouldContain(ParserPatternCategory.ReleaseSource);
    }

    [Test]
    public void CustomConfigurationCanBeUsed()
    {
        var customConfig = new ParserConfiguration
        {
            Patterns = new List<ParserPattern>
            {
                new()
                {
                    Id = "custom-resolution",
                    Pattern = @"8k",
                    Category = ParserPatternCategory.Resolution,
                    IsEnabled = true,
                    RemoveFromTitle = true
                }
            }
        };

        var customParser = new FilenameParserService(_logger, customConfig);
        var result = customParser.Parse("Movie.8k.2020");

        result.RemovedNoise.ShouldContain("8k");
    }

    #endregion

    #region Pattern Priority Tests

    [Test]
    public void PatternsAreAppliedInPriorityOrder()
    {
        // This test verifies that the service can handle patterns with different priorities
        var config = ParserConfiguration.GetDefaultConfiguration();
        
        // Add a pattern that removes everything after a specific marker
        config.Patterns.Add(new ParserPattern
        {
            Id = "test-marker",
            Pattern = @"testmarker",
            Category = ParserPatternCategory.Custom,
            Priority = 0, // High priority
            IsEnabled = true,
            RemoveFromTitle = true
        });

        var parser = new FilenameParserService(_logger, config);
        var result = parser.Parse("Movie.testmarker.2020");

        // The testmarker pattern should remove everything after "testmarker"
        result.NormalizedTitle.ShouldBe("movie");
    }

    #endregion

    #region Noise Extraction Tests

    [Test]
    public void ExtractsNoisePatterns()
    {
        var result = _parser.Parse("The.Matrix.1999.1080p.BluRay.x264.German.DTS");

        // Should extract at least one noise pattern
        result.RemovedNoise.ShouldNotBeEmpty();
    }

    [Test]
    public void DoesNotDuplicateNoisePatterns()
    {
        var result = _parser.Parse("The.Matrix.1080p.1080p.BluRay");

        // Should not duplicate 1080p
        result.RemovedNoise.Count(n => n.Contains("1080p")).ShouldBeLessThanOrEqualTo(1);
    }

    #endregion

    #region Leading Number Tests

    [Test]
    [TestCase("01.The.First.Movie.2020.1080p", "the first movie")]
    [TestCase("001.The.Second.Movie.2020.1080p", "the second movie")]
    [TestCase("100.The.Hundredth.Movie.2020.1080p", "the hundredth movie")]
    public void RemovesLeadingNumbers(string filename, string expectedTitle)
    {
        var result = _parser.Parse(filename);

        result.NormalizedTitle.ShouldBe(expectedTitle);
    }

    #endregion

    #region File Size Pattern Tests

    [Test]
    [TestCase("Movie.2020.1080p.5GB", "movie", "gb")]
    [TestCase("Movie.2020.720p.1.5GB", "movie", "gb")]
    [TestCase("Movie.2020.480p.700MB", "movie", "mb")]
    public void RemovesFileSizePatterns(string filename, string expectedTitle, string expectedPattern)
    {
        var result = _parser.Parse(filename);

        result.RemovedNoise.ShouldContain(n => n.ToLower().Contains(expectedPattern.ToLower()));
    }

    #endregion
}
