using Jrpg.Core.Events;
using Jrpg.Core.Exploration;
using Jrpg.Core.Randomness;
using Jrpg.Core.Session;
using Jrpg.Game.Audio;
using Jrpg.Game.Exploration;
using Jrpg.Game.Hosting;
using Jrpg.Game.Input;
using Jrpg.Game.Tests.Input;
using Jrpg.Game.Tests.Ui;

namespace Jrpg.Game.Tests.Exploration;

[Collection(RepositoryScreenText.CollectionName)]
public sealed class EventPresenterTests
{
    private static readonly InputFrame _confirmPressed =
        new(MoveIntent.None, ButtonSet.Empty.With(GameButton.Confirm), ButtonSet.Empty.With(GameButton.Confirm));

    [Fact]
    public void Update_WarpStep_ContinuesOnlyOnceBlack()
    {
        var exploration = StartMapScenarios.StartExploring(StartMapScenarios.NextToForestWarp);
        for (var update = 0; update < 120 && !exploration.IsEventRunning; update++)
        {
            exploration.Walk(StartMapScenarios.HoldingLeft.Move, StartMapScenarios.OneUpdate);
        }

        Assert.IsType<WarpStep>(exploration.CurrentEventStep);
        var presenter = EventPresenters.Over(exploration);
        presenter.Update(StartMapScenarios.OneUpdate, default);
        presenter.Update(TimeSpan.FromSeconds(0.2), default);
        Assert.Equal("start", exploration.CurrentMap.Id);

        presenter.Update(TimeSpan.FromSeconds(0.1), default);

        Assert.Equal("forest", exploration.CurrentMap.Id);
        Assert.Equal(1f, presenter.Blackness);
        Assert.True(presenter.IsBusy);
    }

    [Fact]
    public void Update_ConfirmOnFoundItem_ContinuesEvent()
    {
        var exploration = StartMapScenarios.StartExploring(StartMapScenarios.BelowChest);
        var chest = exploration.CurrentMap.Layout.Objects.Single(placed => placed.ObjectId == "chest");
        Assert.True(exploration.TryInteract());
        var presenter = EventPresenters.Over(exploration);
        presenter.Update(StartMapScenarios.OneUpdate, default);
        Assert.IsType<GiveItemStep>(exploration.CurrentEventStep);

        presenter.Update(StartMapScenarios.OneUpdate, _confirmPressed);

        Assert.False(exploration.IsEventRunning);
        Assert.Equal("chest_opened", exploration.CurrentDefinitionOf(chest).Id);
    }

    [Fact]
    public void Update_RestStep_ShowsRestLineThenContinues()
    {
        var exploration = StartMapScenarios.StartExploring(StartMapScenarios.BelowRestingChest);
        exploration.TryInteract();
        var presenter = EventPresenters.Over(exploration);
        presenter.Update(StartMapScenarios.OneUpdate, default);
        Assert.IsType<RestStep>(exploration.CurrentEventStep);
        presenter.Update(TimeSpan.FromSeconds(2), default);

        presenter.Update(StartMapScenarios.OneUpdate, _confirmPressed);

        Assert.False(exploration.IsEventRunning);
    }

    [Fact]
    public void Update_ConfirmOnLastPage_ContinuesEvent()
    {
        var exploration = StartMapScenarios.StartExploring(StartMapScenarios.BelowTalkingChest);
        exploration.TryInteract();
        var presenter = EventPresenters.Over(exploration);
        var typedOut = TimeSpan.FromSeconds(2);
        presenter.Update(StartMapScenarios.OneUpdate, default);
        presenter.Update(typedOut, default);
        presenter.Update(StartMapScenarios.OneUpdate, _confirmPressed);
        presenter.Update(typedOut, default);
        Assert.IsType<DialogueStep>(exploration.CurrentEventStep);

        presenter.Update(StartMapScenarios.OneUpdate, _confirmPressed);

        Assert.False(exploration.IsEventRunning);
    }

    [Fact]
    public void Update_TextSpeedInstant_ShowsWholePage()
    {
        var exploration = StartMapScenarios.StartExploring(StartMapScenarios.BelowTalkingChest);
        exploration.TryInteract();
        var presenter = EventPresenters.Over(exploration, settings: new PlayerSettings { TextSpeed = TextSpeed.Instant });
        presenter.Update(StartMapScenarios.OneUpdate, default);
        presenter.Update(StartMapScenarios.OneUpdate, _confirmPressed);

        // Each page shows whole, so one Confirm per page reads the dialogue with no time to type.
        presenter.Update(StartMapScenarios.OneUpdate, _confirmPressed);

        Assert.False(exploration.IsEventRunning);
    }

    [Fact]
    public void Update_FirstUpdateOnFoundItem_WaitsEvenWithConfirmPressed()
    {
        var exploration = StartMapScenarios.StartExploring(StartMapScenarios.BelowChest);
        exploration.TryInteract();
        var presenter = EventPresenters.Over(exploration);

        presenter.Update(StartMapScenarios.OneUpdate, _confirmPressed);

        Assert.IsType<GiveItemStep>(exploration.CurrentEventStep);
    }

    [Fact]
    public void Update_VoiceStepStarted_PlaysItsFile()
    {
        var (_, presenter, voice) = StartIntro();

        presenter.Update(StartMapScenarios.OneUpdate, default);

        Assert.Equal("Placeholder.wav", voice.VoiceFile);
    }

    [Fact]
    public void Update_HeldSkipDuringTheCinematic_StopsTheVoice()
    {
        var (_, presenter, voice) = StartIntro();
        presenter.Update(StartMapScenarios.OneUpdate, default);

        HoldConfirmToSkip(presenter);

        Assert.Null(voice.VoiceFile);
    }

    [Fact]
    public void Update_EventEnded_StopsTheVoice()
    {
        var (exploration, presenter, voice) = StartIntro();
        presenter.Update(StartMapScenarios.OneUpdate, default);
        presenter.MarkStillsLoaded();
        Assert.NotNull(voice.VoiceFile);

        // Bounded: the stills' seconds, then the warp's fade, at the game's 60 updates a second.
        for (var update = 0; update < 60 * 20 && exploration.IsEventRunning; update++)
        {
            presenter.Update(StartMapScenarios.OneUpdate, default);
        }

        Assert.False(exploration.IsEventRunning);
        Assert.Null(voice.VoiceFile);
    }

    [Fact]
    public void Update_AfterTheCinematicsLeadOut_ContinuesToTheWarp()
    {
        var (exploration, presenter, _) = StartIntro();
        presenter.Update(StartMapScenarios.OneUpdate, default);
        presenter.MarkStillsLoaded();
        // Bounded: the lead-in and the three stills' 12 s, at the game's 60 updates a second.
        for (var update = 0; update < 60 * 15 && exploration.CurrentEventStep is CinematicStep; update++)
        {
            presenter.Update(StartMapScenarios.OneUpdate, default);
        }

        // Black from the lead-out's end until the warp has moved the player: the intro map never shows again.
        for (var update = 0; update < 120 && exploration.CurrentMap.Id == "intro"; update++)
        {
            Assert.Equal(1f, presenter.Blackness);
            presenter.Update(StartMapScenarios.OneUpdate, default);
        }

        Assert.Equal("start", exploration.CurrentMap.Id);
        Assert.Equal(1f, presenter.Blackness);
    }

    [Fact]
    public void Update_SkipBeforeTheWarp_StaysBlackIntoIt()
    {
        var (exploration, presenter, _) = StartIntro();
        presenter.Update(StartMapScenarios.OneUpdate, default);
        presenter.MarkStillsLoaded();

        HoldConfirmToSkip(presenter);

        Assert.IsType<WarpStep>(exploration.CurrentEventStep);
        Assert.Equal(1f, presenter.Blackness);
    }

    // A press, then Confirm held for the hold time at the game's fixed step (decision 623).
    private static void HoldConfirmToSkip(EventPresenter presenter)
    {
        presenter.Update(StartMapScenarios.OneUpdate, _confirmPressed);
        for (var held = TimeSpan.Zero; held < SkipHold.HoldTime; held += StartMapScenarios.OneUpdate)
        {
            presenter.Update(StartMapScenarios.OneUpdate, InputFrames.Holding(GameButton.Confirm));
        }
    }

    // A new game on the test world's content: Ragnir's intro opens with the voice, then its cinematic of three stills,
    // then a warp.
    private static (ExplorationState Exploration, EventPresenter Presenter, VoiceChannel Voice) StartIntro()
    {
        var exploration = GameSession.StartNew(StartMapScenarios.TestWorld, "ragnir", new SeededRandom(1)).Exploration;
        var voice = new VoiceChannel();
        return (exploration, EventPresenters.Over(exploration, voice: voice), voice);
    }
}
