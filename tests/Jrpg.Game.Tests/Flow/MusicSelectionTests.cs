using Jrpg.Core.Content;
using Jrpg.Core.Randomness;
using Jrpg.Core.Session;
using Jrpg.Game.Battle;
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
        Assert.Equal(UiSoundFiles.Music.Title, MusicSelection.TrackFor(null, null));
    }

    [Fact]
    public void TrackFor_OnAMapWithATrack_IsTheMapsTrack()
    {
        Assert.Equal("Own.wav", MusicSelection.TrackFor(SessionOn(WithStartTrack(StartMapScenarios.AfterTheIntro, "Own.wav")), null));
    }

    [Fact]
    public void TrackFor_OnAMapWithoutATrack_IsSilence()
    {
        Assert.Null(MusicSelection.TrackFor(SessionOn(WithStartTrack(StartMapScenarios.AfterTheIntro, null)), null));
    }

    [Fact]
    public void TrackFor_EncounterPending_IsTheBattleTheme()
    {
        var flow = StartedFlow(catalog: StartMapScenarios.NextToForestWarpAlwaysRolling);
        WalkUntilAnEncounter(flow);

        Assert.Equal(UiSoundFiles.Music.Battle, MusicSelection.TrackFor(SessionOf(flow), null));
    }

    // Every encounter names the theme, so whichever the forest rolls has it.
    [Fact]
    public void TrackFor_EncounterWithItsOwnTheme_IsThatTheme()
    {
        var catalog = StartMapScenarios.NextToForestWarpAlwaysRolling;
        var themed = catalog.Encounters.ToDictionary(pair => pair.Key, pair => pair.Value with { MusicFile = "Own.wav" });
        var flow = StartedFlow(catalog: catalog with { Encounters = themed });
        WalkUntilAnEncounter(flow);

        Assert.Equal("Own.wav", MusicSelection.TrackFor(SessionOf(flow), null));
    }

    // With the encounter still pending, so only the cue can give these.
    [Fact]
    public void TrackFor_BattleCueSilence_IsSilence()
    {
        Assert.Null(MusicSelection.TrackFor(SessionWithAPendingEncounter(), BattleMusicCue.Silence));
    }

    [Fact]
    public void TrackFor_BattleCueField_IsTheMapsTrack()
    {
        var session = SessionWithAPendingEncounter();

        Assert.Equal(session.Exploration.CurrentMap.Settings.MusicFile, MusicSelection.TrackFor(session, BattleMusicCue.Field));
        Assert.NotNull(session.Exploration.CurrentMap.Settings.MusicFile);
    }

    [Fact]
    public void TrackFor_BattleCueTheme_IsTheBattleTheme()
    {
        Assert.Equal(UiSoundFiles.Music.Battle, MusicSelection.TrackFor(SessionWithAPendingEncounter(), BattleMusicCue.Theme));
    }

    private static GameSession SessionWithAPendingEncounter()
    {
        var flow = StartedFlow(catalog: StartMapScenarios.NextToForestWarpAlwaysRolling);
        WalkUntilAnEncounter(flow);
        return SessionOf(flow);
    }

    private static GameSession SessionOn(ContentCatalog catalog) => GameSession.StartNew(catalog, "sarxian", new SeededRandom(1));

}
