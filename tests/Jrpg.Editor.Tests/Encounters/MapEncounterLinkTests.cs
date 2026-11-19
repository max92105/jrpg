using Jrpg.Editor.Encounters;
using static Jrpg.Editor.Tests.Encounters.EncounterFixtures;

namespace Jrpg.Editor.Tests.Encounters;

// A map's settings with an encounter linked or taken out, as the Encounters page's Link and Remove change them.
public sealed class MapEncounterLinkTests
{
    [Fact]
    public void IsLinked_MapRollingIt_IsTrue()
    {
        Assert.True(MapEncounterLink.IsLinked(MapRolling("cave", "a_trio").Settings, "a_trio"));
    }

    [Fact]
    public void IsLinked_MapRollingAnother_IsFalse()
    {
        Assert.False(MapEncounterLink.IsLinked(MapRolling("cave", "b_pair").Settings, "a_trio"));
    }

    [Fact]
    public void IsLinked_MapWithoutBattles_IsFalse()
    {
        Assert.False(MapEncounterLink.IsLinked(MapWithoutBattles("town", null).Settings, "a_trio"));
    }

    [Fact]
    public void Linked_MapWithBattles_AddsItLastAndKeepsTheOdds()
    {
        var linked = MapEncounterLink.Linked(MapRolling("cave", "b_pair").Settings, "a_trio");

        Assert.Equal(["b_pair", "a_trio"], linked.RandomEncounters!.EncounterIds);
        Assert.Equal((12, 3), (linked.RandomEncounters.OneInSteps, linked.RandomEncounters.GraceSteps));
    }

    [Fact]
    public void Linked_MapWithoutBattles_Throws()
    {
        var settings = MapWithoutBattles("town", null).Settings;

        Assert.Throws<InvalidOperationException>(() => MapEncounterLink.Linked(settings, "a_trio"));
    }

    [Fact]
    public void Unlinked_OneOfTwo_KeepsTheOtherAndTheOdds()
    {
        var unlinked = MapEncounterLink.Unlinked(MapRolling("cave", "b_pair", "a_trio").Settings, "b_pair");

        Assert.Equal(["a_trio"], unlinked.RandomEncounters!.EncounterIds);
        Assert.Equal((12, 3), (unlinked.RandomEncounters.OneInSteps, unlinked.RandomEncounters.GraceSteps));
    }

    // The game refuses random battles that pick from nothing, so the last one out turns them off.
    [Fact]
    public void Unlinked_TheLastOne_TurnsRandomBattlesOffAndKeepsTheBackground()
    {
        var unlinked = MapEncounterLink.Unlinked(MapRolling("cave", "a_trio").Settings, "a_trio");

        Assert.Null(unlinked.RandomEncounters);
        Assert.Equal("Cave.png", unlinked.BattleBackgroundFile);
    }

    [Fact]
    public void Unlinked_MapWithoutBattles_IsUnchanged()
    {
        var settings = MapWithoutBattles("town", null).Settings;

        Assert.Same(settings, MapEncounterLink.Unlinked(settings, "a_trio"));
    }
}
