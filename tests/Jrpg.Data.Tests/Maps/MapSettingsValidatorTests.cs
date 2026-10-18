using Jrpg.Core.Maps;
using Jrpg.Data.Content;
using Jrpg.Data.Maps;

namespace Jrpg.Data.Tests.Maps;

public sealed class MapSettingsValidatorTests
{
    private const string FileName = "maps/forest.json";

    [Fact]
    public void Validate_IdDiffersFromFileName_ThrowsNamingTheFile()
    {
        var exception = Assert.Throws<ContentValidationException>(
            () => MapSettingsValidator.Validate(FileName, new MapSettings { Id = "woods", DisplayName = "Forest" }));

        Assert.Equal("maps/forest.json: id must equal the file name 'forest' (was 'woods').", exception.Message);
    }

    [Fact]
    public void Validate_EmptyDisplayName_ThrowsNamingTheField()
    {
        var exception = Assert.Throws<ContentValidationException>(
            () => MapSettingsValidator.Validate(FileName, new MapSettings { Id = "forest", DisplayName = " " }));

        Assert.Equal("maps/forest.json: displayName must not be empty.", exception.Message);
    }
}
