namespace Jrpg.ContentConverter.Tests;

public sealed class LegacyIdsTests
{
    private const string SourceFile = "Objects/Objects.json";

    [Theory]
    [InlineData("Chest Opened", "chest_opened")]
    [InlineData("House1", "house_1")]
    [InlineData("Door_Close", "door_close")]
    [InlineData("Interior1", "interior_1")]
    [InlineData("ChestOpened", "chest_opened")]
    [InlineData("Custom  Map-2", "custom_map_2")]
    [InlineData("tree", "tree")]
    public void FromName_MixedSeparatorsAndDigits_ReturnsSnakeCase(string name, string expectedId)
    {
        var id = LegacyIds.FromName(SourceFile, name);

        Assert.Equal(expectedId, id);
    }

    [Fact]
    public void FromName_CharacterAnIdCannotHold_ThrowsNamingFileAndName()
    {
        var exception = Assert.Throws<ConversionException>(() => LegacyIds.FromName(SourceFile, "Café"));

        Assert.Equal("Objects/Objects.json: name 'Café' has 'é', which an id can't hold.", exception.Message);
    }

    [Fact]
    public void FromName_OnlySeparators_ThrowsNamingFileAndName()
    {
        var exception = Assert.Throws<ConversionException>(() => LegacyIds.FromName(SourceFile, " _ "));

        Assert.Equal("Objects/Objects.json: name ' _ ' gives an empty id.", exception.Message);
    }
}
