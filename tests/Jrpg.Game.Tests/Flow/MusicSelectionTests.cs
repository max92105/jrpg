using Jrpg.Core.Content;
using Jrpg.Core.Randomness;
using Jrpg.Core.Session;
using Jrpg.Game.Flow;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;
using static Jrpg.Game.Tests.Flow.FlowStarts;

namespace Jrpg.Game.Tests.Flow;

[Collection(RepositoryScreenText.CollectionName)]
public sealed class MusicSelectionTests
{
    [Fact]
    public void TrackFor_NoSession_IsTheTitleTheme()
    {
        Assert.Equal(UiSoundFiles.Music.Title, MusicSelection.TrackFor(null));
    }

    [Fact]
    public void TrackFor_OnAMapWithATrack_IsTheMapsTrack()
    {
        Assert.Equal("Own.wav", MusicSelection.TrackFor(SessionOn(WithStartTrack(StartMapScenarios.AfterTheIntro, "Own.wav"))));
    }

    [Fact]
    public void TrackFor_OnAMapWithoutATrack_IsSilence()
    {
        Assert.Null(MusicSelection.TrackFor(SessionOn(WithStartTrack(StartMapScenarios.AfterTheIntro, null))));
    }

    [Fact]
    public void TrackFor_EncounterPending_IsTheBattleTheme()
    {
        var flow = StartedFlow(catalog: StartMapScenarios.NextToForestWarpAlwaysRolling);
        WalkUntilAnEncounter(flow);

        Assert.Equal(UiSoundFiles.Music.Battle, MusicSelection.TrackFor(SessionOf(flow)));
    }

    // Every encounter names the theme, so whichever the forest rolls has it.
    [Fact]
    public void TrackFor_EncounterWithItsOwnTheme_IsThatTheme()
    {
        var catalog = StartMapScenarios.NextToForestWarpAlwaysRolling;
        var themed = catalog.Encounters.ToDictionary(pair => pair.Key, pair => pair.Value with { MusicFile = "Own.wav" });
        var flow = StartedFlow(catalog: catalog with { Encounters = themed });
        WalkUntilAnEncounter(flow);

        Assert.Equal("Own.wav", MusicSelection.TrackFor(SessionOf(flow)));
    }

    private static GameSession SessionOn(ContentCatalog catalog) => GameSession.StartNew(catalog, "sarxian", new SeededRandom(1));

}
