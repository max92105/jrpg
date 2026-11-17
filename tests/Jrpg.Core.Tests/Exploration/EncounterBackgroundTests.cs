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
}
