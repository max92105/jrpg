using Jrpg.Core.Items;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

public sealed class ContentFileReaderTests
{
    [Fact]
    public void Read_MalformedJson_ThrowsNamingTheFile()
    {
        var exception = Assert.Throws<ContentValidationException>(
            () => ContentFileReader.Read<IReadOnlyList<ItemDefinition>>(TestContentPath("malformed.json")));

        Assert.StartsWith("malformed.json", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Read_MissingRequiredField_ThrowsNamingTheFileAndField()
    {
        var exception = Assert.Throws<ContentValidationException>(
            () => ContentFileReader.Read<IReadOnlyList<ItemDefinition>>(TestContentPath("missing-allowed-class-ids.json")));

        Assert.StartsWith("missing-allowed-class-ids.json at $[0].equipment:", exception.Message, StringComparison.Ordinal);
        Assert.Contains("allowedClassIds", exception.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("null-name.json", "$[0].name")]
    [InlineData("integer-enum.json", "$[0].effect.kind")]
    [InlineData("duplicate-property.json", "$[0].id")]
    public void Read_StrictnessViolation_ThrowsNamingTheFileAndPath(string fileName, string jsonPath)
    {
        var exception = Assert.Throws<ContentValidationException>(
            () => ContentFileReader.Read<IReadOnlyList<ItemDefinition>>(TestContentPath(fileName)));

        Assert.StartsWith($"{fileName} at {jsonPath}", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Read_UnknownField_ThrowsNamingTheField()
    {
        var exception = Assert.Throws<ContentValidationException>(
            () => ContentFileReader.Read<IReadOnlyList<ItemDefinition>>(TestContentPath("unknown-field.json")));

        Assert.Contains("maxStak", exception.Message, StringComparison.Ordinal);
    }

    private static string TestContentPath(string fileName) => Path.Combine(AppContext.BaseDirectory, "TestContent", fileName);
}
