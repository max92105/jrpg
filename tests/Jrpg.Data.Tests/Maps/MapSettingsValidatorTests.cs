using Jrpg.Core.Maps;
using Jrpg.Data.Content;
using Jrpg.Data.Maps;

namespace Jrpg.Data.Tests.Maps;

public sealed class MapSettingsValidatorTests
{
    private const string FileName = "maps/forest.json";

    private static readonly HashSet<string> _encounterIds = ["goblin_trio"];

    private static readonly MapSettings _forest = new()
    {
        Id = "forest",
        DisplayName = "Forest",
        RandomEncounters = new RandomEncounterTable { OneInSteps = 25, GraceSteps = 3, EncounterIds = ["goblin_trio"] },
    };

    public static TheoryData<MapSettings, string> InvalidSettings => new()
    {
        { _forest with { DisplayName = " " }, "displayName must not be empty." },
        { WithTable(_forest.RandomEncounters! with { GraceSteps = 0 }), "randomEncounters.graceSteps must be at least 1 (was 0)." },
        { WithTable(_forest.RandomEncounters! with { EncounterIds = [] }), "randomEncounters.encounterIds must list at least one encounter." },
        { WithTable(_forest.RandomEncounters! with { EncounterIds = ["goblin_trio", null!] }), "randomEncounters.encounterIds holds a null entry." },
    };

    [Fact]
    public void Validate_ValidSettings_DoesNotThrow()
    {
        var exception = Record.Exception(() => MapSettingsValidator.Validate(FileName, _forest, _encounterIds));

        Assert.Null(exception);
    }

    [Fact]
    public void Validate_LowestAcceptedOddsAndGrace_DoesNotThrow()
    {
        var settings = WithTable(_forest.RandomEncounters! with { OneInSteps = 1, GraceSteps = 1 });

        var exception = Record.Exception(() => MapSettingsValidator.Validate(FileName, settings, _encounterIds));

        Assert.Null(exception);
    }

    [Fact]
    public void Validate_NoRandomEncounters_DoesNotThrow()
    {
        var exception = Record.Exception(() => MapSettingsValidator.Validate(FileName, _forest with { RandomEncounters = null }, _encounterIds));

        Assert.Null(exception);
    }

    [Fact]
    public void Validate_IdDiffersFromFileName_ThrowsNamingTheFile()
    {
        var exception = Assert.Throws<ContentValidationException>(
            () => MapSettingsValidator.Validate(FileName, _forest with { Id = "woods" }, _encounterIds));

        Assert.Equal("maps/forest.json: id must equal the file name 'forest' (was 'woods').", exception.Message);
    }

    [Fact]
    public void Validate_UnknownEncounterId_ThrowsNamingTheMap()
    {
        var settings = WithTable(_forest.RandomEncounters! with { EncounterIds = ["dragon"] });

        var exception = Assert.Throws<ContentValidationException>(() => MapSettingsValidator.Validate(FileName, settings, _encounterIds));

        Assert.Equal("maps/forest.json: randomEncounters.encounterIds has unknown encounter id 'dragon'.", exception.Message);
    }

    [Fact]
    public void Validate_ZeroOneInSteps_ThrowsNamingTheField()
    {
        var settings = WithTable(_forest.RandomEncounters! with { OneInSteps = 0 });

        var exception = Assert.Throws<ContentValidationException>(() => MapSettingsValidator.Validate(FileName, settings, _encounterIds));

        Assert.Equal("maps/forest.json: randomEncounters.oneInSteps must be at least 1 (was 0).", exception.Message);
    }

    [Theory]
    [MemberData(nameof(InvalidSettings))]
    public void Validate_InvalidField_ThrowsNamingTheField(MapSettings settings, string problem)
    {
        var exception = Assert.Throws<ContentValidationException>(() => MapSettingsValidator.Validate(FileName, settings, _encounterIds));

        Assert.Equal($"maps/forest.json: {problem}", exception.Message);
    }

    private static MapSettings WithTable(RandomEncounterTable table) => _forest with { RandomEncounters = table };
}
