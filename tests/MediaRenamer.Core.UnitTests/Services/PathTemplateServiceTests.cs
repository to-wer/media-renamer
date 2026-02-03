using MediaRenamer.Core.Models;
using MediaRenamer.Core.Services;
using Shouldly;

namespace MediaRenamer.Core.UnitTests.Services;

public class PathTemplateServiceTests
{
    [Test]
    public void RenderTemplate_ShouldHandleTvSeriesTemplate()
    {
        // Arrange
        var template = "{SeriesName} ({Year})/Season {Season:D2}/{SeriesName} S{Season:D2}E{Episode:D2} {EpisodeName} [{Resolution}] [{Codec}]";
        var file = new MediaFile
        {
            Title = "Breaking Bad",
            Year = 2008,
            Type = MediaType.Episode,
            Season = 1,
            Episode = 1,
            EpisodeTitle = "Pilot",
            Resolution = "1080p",
            Codec = "x264"
        };

        // Act
        var result = PathTemplateService.RenderTemplate(template, file);

        // Assert
        result.ShouldBe("Breaking Bad (2008)/Season 01/Breaking Bad S01E01 Pilot [1080p] [x264]");
    }

    [Test]
    public void RenderTemplate_ShouldHandleMovieTemplate()
    {
        // Arrange
        var template = "{Title} ({Year})/{Title} ({Year}) [{Resolution}] [{Codec}]";
        var file = new MediaFile
        {
            Title = "Inception",
            Year = 2010,
            Type = MediaType.Movie,
            Resolution = "4K",
            Codec = "HEVC"
        };

        // Act
        var result = PathTemplateService.RenderTemplate(template, file);

        // Assert
        result.ShouldBe("Inception (2010)/Inception (2010) [4K] [HEVC]");
    }

    [Test]
    public void RenderTemplate_ShouldRemoveEmptyBrackets()
    {
        // Arrange
        var template = "{Title} ({Year}) [{Resolution}] [{Codec}]";
        var file = new MediaFile
        {
            Title = "Test Movie",
            Year = 2020,
            Type = MediaType.Movie
            // Resolution und Codec fehlen
        };

        // Act
        var result = PathTemplateService.RenderTemplate(template, file);

        // Assert
        result.ShouldBe("Test Movie (2020)");
    }
}