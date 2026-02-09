using MediaRenamer.Core.Extensions;
using Shouldly;

namespace MediaRenamer.Core.UnitTests.Extensions;

public class StringExtensionsTests
{
    #region ReplaceMultipleSpaces Tests

    [Test]
    [TestCase("hello world", "hello world")]
    [TestCase("hello  world", "hello world")]
    [TestCase("hello   world", "hello world")]
    [TestCase("hello world  ", "hello world")]
    [TestCase("  hello world", "hello world")]
    [TestCase("  hello   world  ", "hello world")]
    public void ReplaceMultipleSpaces_CollapsesMultipleSpaces(string input, string expected)
    {
        var result = input.ReplaceMultipleSpaces();

        result.ShouldBe(expected);
    }

    [Test]
    public void ReplaceMultipleSpaces_HandlesEmptyString()
    {
        var result = string.Empty.ReplaceMultipleSpaces();

        result.ShouldBeEmpty();
    }

    [Test]
    public void ReplaceMultipleSpaces_HandlesWhitespaceOnly()
    {
        var result = "   ".ReplaceMultipleSpaces();

        result.ShouldBeEmpty();
    }

    [Test]
    public void ReplaceMultipleSpaces_HandlesNull()
    {
        string? input = null;
        var result = input!.ReplaceMultipleSpaces();

        result.ShouldBeEmpty();
    }

    #endregion

    #region NormalizeForParsing Tests

    [Test]
    [TestCase("The.Matrix.1999", "the matrix 1999")]
    [TestCase("The-Matrix-1999", "the matrix 1999")]
    [TestCase("The_Matrix_1999", "the matrix 1999")]
    [TestCase("The (Matrix) 1999", "the matrix 1999")]
    [TestCase("The [Matrix] 1999", "the matrix 1999")]
    public void NormalizeForParsing_ReplacesSeparators(string input, string expected)
    {
        var result = input.NormalizeForParsing();

        result.ShouldBe(expected);
    }

    [Test]
    [TestCase("Movie!@#$%2020", "movie2020")] // Special chars are removed
    [TestCase("Movie\"test\"2020", "movietest2020")] // Quotes are removed
    [TestCase("Movie'test'2020", "movietest2020")] // Single quotes are removed
    public void NormalizeForParsing_RemovesSpecialCharacters(string input, string expected)
    {
        var result = input.NormalizeForParsing();

        result.ShouldBe(expected);
    }

    [Test]
    [TestCase("Café.2020", "cafe 2020")] // Diacritics are normalized
    [TestCase("Naïve.2020", "naive 2020")]
    [TestCase("Héllo.2020", "hello 2020")]
    [TestCase("René.2020", "rene 2020")]
    public void NormalizeForParsing_NormalizesUnicodeCharacters(string input, string expected)
    {
        var result = input.NormalizeForParsing();

        result.ShouldBe(expected);
    }

    [Test]
    [TestCase("Curaçao.2020", "curacao 2020")]
    [TestCase("Zoë.2020", "zoe 2020")]
    [TestCase("Jötunheim.2020", "jotunheim 2020")]
    public void NormalizeForParsing_NormalizesUnicodeWithDiacritics(string input, string expected)
    {
        var result = input.NormalizeForParsing();

        result.ShouldBe(expected);
    }

    [Test]
    public void NormalizeForParsing_ConvertsToLowercase()
    {
        var result = "THE MATRIX 1999".NormalizeForParsing();

        result.ShouldBe("the matrix 1999");
    }

    [Test]
    public void NormalizeForParsing_TrimsWhitespace()
    {
        var result = "   The Matrix 1999   ".NormalizeForParsing();

        result.ShouldBe("the matrix 1999");
    }

    [Test]
    public void NormalizeForParsing_HandlesEmptyString()
    {
        var result = string.Empty.NormalizeForParsing();

        result.ShouldBeEmpty();
    }

    [Test]
    public void NormalizeForParsing_HandlesWhitespaceOnly()
    {
        var result = "   ".NormalizeForParsing();

        result.ShouldBeEmpty();
    }

    [Test]
    public void NormalizeForParsing_HandlesNull()
    {
        string? input = null;
        var result = input!.NormalizeForParsing();

        result.ShouldBeEmpty();
    }

    [Test]
    [TestCase("Movie.2020.1080p", "movie 2020 1080p")]
    [TestCase("Movie_2020_720p", "movie 2020 720p")]
    [TestCase("Movie-2020-480p", "movie 2020 480p")]
    public void NormalizeForParsing_CombinesAllTransformations(string input, string expected)
    {
        var result = input.NormalizeForParsing();

        result.ShouldBe(expected);
    }

    [Test]
    public void NormalizeForParsing_HandlesUnicodeSpaces()
    {
        // Non-breaking space and other Unicode spaces should be handled
        var input = "The\u00A0Matrix\u1680 2020"; // \u00A0 = NBSP, \u1680 = Ogham space mark
        var result = input.NormalizeForParsing();

        result.ShouldContain("the");
        result.ShouldContain("matrix");
        result.ShouldContain("2020");
    }

    #endregion

    #region NormalizeForDisplay Tests

    [Test]
    [TestCase("The   Matrix  2020", "The Matrix 2020")]
    [TestCase("The  Matrix", "The Matrix")]
    public void NormalizeForDisplay_CollapsesSpaces(string input, string expected)
    {
        var result = input.NormalizeForDisplay();

        result.ShouldBe(expected);
    }

    [Test]
    public void NormalizeForDisplay_TrimsWhitespace()
    {
        var result = "   The Matrix 2020   ".NormalizeForDisplay();

        result.ShouldBe("The Matrix 2020");
    }

    [Test]
    public void NormalizeForDisplay_HandlesEmptyString()
    {
        var result = string.Empty.NormalizeForDisplay();

        result.ShouldBeEmpty();
    }

    [Test]
    public void NormalizeForDisplay_PreservesUnicodeCharacters()
    {
        var result = "Café 2020".NormalizeForDisplay();

        result.ShouldBe("Café 2020");
    }

    [Test]
    public void NormalizeForDisplay_DoesNotConvertToLowercase()
    {
        var result = "The Matrix 2020".NormalizeForDisplay();

        result.ShouldBe("The Matrix 2020");
    }

    #endregion
}
