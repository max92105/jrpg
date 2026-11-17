using Jrpg.Editor.Encounters;
using static Jrpg.Editor.Tests.Encounters.EncounterFixtures;

namespace Jrpg.Editor.Tests.Encounters;

// An encounter's Background and Music boxes (decision 906): the map's background and the battle theme first, a file no
// longer in its folder kept listed so opening the page never changes the encounter, both firsts written as null.
public sealed class EncounterViewModelBoxTests
{
    [Fact]
    public void From_EncounterWithoutItsOwnBackground_ChoosesTheMaps()
    {
        var encounter = EncounterViewModel.From(GoblinPair, Choices);

        Assert.Equal(EncounterViewModel.MapsBackground, encounter.SelectedBackground);
        Assert.Equal([EncounterViewModel.MapsBackground, "Cave.png", "Forest.png"], encounter.BackgroundChoices);
    }

    [Fact]
    public void From_EncounterWithItsOwnBackground_ChoosesIt()
    {
        var encounter = EncounterViewModel.From(GoblinPair with { BackgroundFile = "Forest.png" }, Choices);

        Assert.Equal("Forest.png", encounter.SelectedBackground);
        Assert.Equal([EncounterViewModel.MapsBackground, "Cave.png", "Forest.png"], encounter.BackgroundChoices);
    }

    [Fact]
    public void From_BackgroundNoLongerInTheFolder_StaysListed()
    {
        var encounter = EncounterViewModel.From(GoblinPair with { BackgroundFile = "Gone.png" }, Choices);

        Assert.Equal("Gone.png", encounter.SelectedBackground);
        Assert.Equal([EncounterViewModel.MapsBackground, "Gone.png", "Cave.png", "Forest.png"], encounter.BackgroundChoices);
    }

    [Fact]
    public void From_NoMusic_ChoosesTheBattleTheme()
    {
        var encounter = EncounterViewModel.From(GoblinPair, Choices);

        Assert.Equal(EncounterViewModel.BattleTheme, encounter.SelectedMusic);
        Assert.Equal([EncounterViewModel.BattleTheme, "boss/loop.wav"], encounter.MusicChoices);
    }

    [Fact]
    public void From_MusicNoLongerInTheFolder_StaysListed()
    {
        var encounter = EncounterViewModel.From(GoblinPair with { MusicFile = "gone/loop.wav" }, Choices);

        Assert.Equal("gone/loop.wav", encounter.SelectedMusic);
        Assert.Equal([EncounterViewModel.BattleTheme, "gone/loop.wav", "boss/loop.wav"], encounter.MusicChoices);
    }

    // Starts from an encounter with its own of both, so a null written is the box's, not the record's.
    [Fact]
    public void ToEncounter_MapsBackgroundAndBattleTheme_WritesNulls()
    {
        var arena = GoblinPair with { BackgroundFile = "Cave.png", MusicFile = "boss/loop.wav" };
        var encounter = EncounterViewModel.From(arena, Choices);
        encounter.SelectedBackground = EncounterViewModel.MapsBackground;
        encounter.SelectedMusic = EncounterViewModel.BattleTheme;

        var written = encounter.ToEncounter();

        Assert.Null(written.BackgroundFile);
        Assert.Null(written.MusicFile);
    }

    [Fact]
    public void ToEncounter_Untouched_EqualsTheRecordButForItsEnemyList()
    {
        var arena = GoblinPair with { BackgroundFile = "Gone.png", MusicFile = "boss/loop.wav" };

        var written = EncounterViewModel.From(arena, Choices).ToEncounter();

        Assert.Equal(arena, written with { EnemyIds = arena.EnemyIds });
        Assert.Equal(arena.EnemyIds, written.EnemyIds);
    }

    [Fact]
    public void Changed_BackgroundPicked_IsRaisedOnce()
    {
        var encounter = EncounterViewModel.From(GoblinPair, Choices);
        var raised = 0;
        encounter.Changed += (_, _) => raised++;

        encounter.SelectedBackground = "Cave.png";

        Assert.Equal(1, raised);
    }

    [Fact]
    public void Changed_MusicPicked_IsRaisedOnce()
    {
        var encounter = EncounterViewModel.From(GoblinPair, Choices);
        var raised = 0;
        encounter.Changed += (_, _) => raised++;

        encounter.SelectedMusic = "boss/loop.wav";

        Assert.Equal(1, raised);
    }
}
