using Jrpg.Game.Flow;
using Jrpg.Game.Hosting;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Saving;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;
using static Jrpg.Game.Tests.Flow.FlowStarts;

namespace Jrpg.Game.Tests.Flow;

// The flow writes the track its state calls for after every update (ADR 0035). Seed-independent: the forest always
// rolls.
[Collection(RepositoryScreenText.CollectionName)]
public sealed class GameFlowMusicTests
{
    [Fact]
    public void Update_OnTheTitle_AsksForTheTitleTheme()
    {
        var flow = new GameFlow(StartMapScenarios.AfterTheIntro, RepositoryScreenText.Text, UnwrittenSaveSlots.Create(), new PlayerSettings());

        flow.Update(OneUpdate, default);

        Assert.Equal(UiSoundFiles.Music.Title, flow.Audio.Music.Track);
    }

    [Fact]
    public void Update_DuringTheFlash_AsksForTheBattleTheme()
    {
        var flow = StartedFlow(catalog: StartMapScenarios.NextToForestWarpAlwaysRolling);
        WalkUntilAnEncounter(flow);

        // The update that started the flash wrote it already; this one must write it again, with the scenes waiting.
        flow.Audio.Music.Want(null);
        flow.Update(OneUpdate, default);

        Assert.True(flow.IsFlashing);
        Assert.Equal(UiSoundFiles.Music.Battle, flow.Audio.Music.Track);
    }

    [Fact]
    public void Update_NewGameStarted_AsksForTheStartMapsTrack()
    {
        var flow = StartedFlow(catalog: WithStartTrack(StartMapScenarios.AfterTheIntro, "Own.wav"));

        Assert.Equal("Own.wav", flow.Audio.Music.Track);
    }
}
