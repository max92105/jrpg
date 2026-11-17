using Jrpg.Core.Battle;
using Jrpg.Core.Exploration;
using Jrpg.Core.Maps;

namespace Jrpg.Core.Tests.Exploration;

public sealed class EncounterBackgroundTests
{
    private static readonly EncounterDefinition _goblins = new() { Id = "goblin_trio", EnemyIds = ["goblin"] };
    private static readonly MapSettings _field = new() { Id = "field", DisplayName = "Field" };

    [Fact]
    public void On_EncounterNamesItsOwn_ReturnsTheEncounters()
    {
        var arena = _goblins with { BackgroundFile = "Arena.png" };
        var field = _field with { BattleBackgroundFile = "Field.png" };

        var background = EncounterBackground.On(arena, field);

        Assert.Equal("Arena.png", background);
    }

    [Fact]
    public void On_EncounterNamesNone_ReturnsTheMaps()
    {
        var field = _field with { BattleBackgroundFile = "Field.png" };

        var background = EncounterBackground.On(_goblins, field);

        Assert.Equal("Field.png", background);
    }

    [Fact]
    public void On_NeitherNamesOne_ReturnsNull()
    {
        var background = EncounterBackground.On(_goblins, _field);

        Assert.Null(background);
    }

    [Fact]
    public void PossibleOn_MapAndEncountersWithTheirOwn_ListsEachFileOnce()
    {
        var encounters = EncountersOf(
            _goblins with { BackgroundFile = "Arena.png" },
            new EncounterDefinition { Id = "boss", EnemyIds = ["ogre"], BackgroundFile = "Arena.png" },
            new EncounterDefinition { Id = "wolves", EnemyIds = ["wolf"], BackgroundFile = "Den.png" });
        var field = WithBattles(_field with { BattleBackgroundFile = "Field.png" }, "goblin_trio", "boss", "wolves");

        var files = EncounterBackground.PossibleOn(field, encounters);

        Assert.Equal(["Field.png", "Arena.png", "Den.png"], files);
    }

    [Fact]
    public void PossibleOn_EncounterWithoutItsOwn_AddsNothing()
    {
        var field = WithBattles(_field with { BattleBackgroundFile = "Field.png" }, "goblin_trio");

        var files = EncounterBackground.PossibleOn(field, EncountersOf(_goblins));

        Assert.Equal(["Field.png"], files);
    }

    [Fact]
    public void PossibleOn_NoBackgroundAndNoBattles_IsEmpty()
    {
        var files = EncounterBackground.PossibleOn(_field, EncountersOf(_goblins with { BackgroundFile = "Arena.png" }));

        Assert.Empty(files);
    }

    private static Dictionary<string, EncounterDefinition> EncountersOf(params EncounterDefinition[] encounters) =>
        encounters.ToDictionary(encounter => encounter.Id);

    private static MapSettings WithBattles(MapSettings map, params string[] encounterIds) =>
        map with { RandomEncounters = new RandomEncounterTable { OneInSteps = 10, GraceSteps = 3, EncounterIds = encounterIds } };
}
