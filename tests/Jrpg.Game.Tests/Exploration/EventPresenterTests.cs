using Jrpg.Core.Events;
using Jrpg.Core.Exploration;
using Jrpg.Core.Randomness;
using Jrpg.Core.Session;
using Jrpg.Game.Audio;
using Jrpg.Game.Exploration;
using Jrpg.Game.Hosting;
using Jrpg.Game.Input;
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
        var presenter = new EventPresenter(exploration, RepositoryScreenText.Text, new PlayerSettings(), new VoiceChannel());
        presenter.Update(StartMapScenarios.OneUpdate, default);
        presenter.Update(TimeSpan.FromSeconds(0.2), default);
        Assert.Equal("start", exploration.CurrentMap.Id);

        presenter.Update(TimeSpan.FromSeconds(0.1), default);

        Assert.Equal("forest", exploration.CurrentMap.Id);
        Assert.Equal(1f, presenter.Blackness);
        Assert.True(presenter.IsBusy);
    }

    [Fact]
    public void Update_PastFirstStillsSeconds_ShowsTheSecond()
    {
        var (exploration, presenter) = StartStills();

        presenter.Update(TimeSpan.FromSeconds(4), default);

        Assert.Equal("Placeholder2.png", Assert.IsType<StillStep>(exploration.CurrentEventStep).ImageFile);
        Assert.True(presenter.IsShowingStill);
    }

    [Fact]
    public void Update_ConfirmDuringStill_SkipsToTheDialogue()
    {
        var (exploration, presenter) = StartStills();

        presenter.Update(StartMapScenarios.OneUpdate, _confirmPressed);

        Assert.IsType<DialogueStep>(exploration.CurrentEventStep);
    }

    [Fact]
    public void Update_ConfirmRightAfterTheSkip_DoesNotEndTheDialogue()
    {
        var (exploration, presenter) = StartStills();
        presenter.Update(StartMapScenarios.OneUpdate, _confirmPressed);

        presenter.Update(StartMapScenarios.OneUpdate, _confirmPressed);

        Assert.IsType<DialogueStep>(exploration.CurrentEventStep);
    }

    [Fact]
    public void Update_ConfirmOnFoundItem_ContinuesEvent()
    {
        var exploration = StartMapScenarios.StartExploring(StartMapScenarios.BelowChest);
        var chest = exploration.CurrentMap.Layout.Objects.Single(placed => placed.ObjectId == "chest");
        Assert.True(exploration.TryInteract());
        var presenter = new EventPresenter(exploration, RepositoryScreenText.Text, new PlayerSettings(), new VoiceChannel());
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
        var presenter = new EventPresenter(exploration, RepositoryScreenText.Text, new PlayerSettings(), new VoiceChannel());
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
        var presenter = new EventPresenter(exploration, RepositoryScreenText.Text, new PlayerSettings(), new VoiceChannel());
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
        var presenter = new EventPresenter(exploration, RepositoryScreenText.Text, new PlayerSettings { TextSpeed = TextSpeed.Instant }, new VoiceChannel());
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
        var presenter = new EventPresenter(exploration, RepositoryScreenText.Text, new PlayerSettings(), new VoiceChannel());

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
    public void Update_SkipDuringStills_StopsTheVoice()
    {
        var (_, presenter, voice) = StartIntro();
        presenter.Update(StartMapScenarios.OneUpdate, default);

        presenter.Update(StartMapScenarios.OneUpdate, _confirmPressed);

        Assert.Null(voice.VoiceFile);
    }

    [Fact]
    public void Update_EventEnded_StopsTheVoice()
    {
        var (exploration, presenter, voice) = StartIntro();
        presenter.Update(StartMapScenarios.OneUpdate, default);
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
    public void Update_ConfirmOnFirstStillStep_StartsTheLeadIn()
    {
        var exploration = StartMapScenarios.StartExploring(StartMapScenarios.BelowChestShowingStills);
        exploration.TryInteract();
        var presenter = new EventPresenter(exploration, RepositoryScreenText.Text, new PlayerSettings(), new VoiceChannel());

        presenter.Update(StartMapScenarios.OneUpdate, _confirmPressed);

        Assert.IsType<StillStep>(exploration.CurrentEventStep);
        Assert.Equal(1f, presenter.Blackness);
    }

    [Fact]
    public void Update_ConfirmDuringLeadIn_SkipsToTheDialogue()
    {
        var exploration = StartMapScenarios.StartExploring(StartMapScenarios.BelowChestShowingStills);
        exploration.TryInteract();
        var presenter = new EventPresenter(exploration, RepositoryScreenText.Text, new PlayerSettings(), new VoiceChannel());
        presenter.Update(StartMapScenarios.OneUpdate, default);

        presenter.Update(StartMapScenarios.OneUpdate, _confirmPressed);

        Assert.IsType<DialogueStep>(exploration.CurrentEventStep);
    }

    [Fact]
    public void Update_AfterLastStillsLeadOut_ContinuesToTheWarp()
    {
        var (exploration, presenter, _) = StartIntro();
        presenter.Update(StartMapScenarios.OneUpdate, default);
        presenter.Update(StillRun.LeadIn, default);
        for (var still = 0; still < 3; still++)
        {
            presenter.Update(TimeSpan.FromSeconds(4), default);
        }

        // Black from the last still's end until the warp has moved the player: the intro map never shows again.
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
        presenter.Update(StillRun.LeadIn, default);

        presenter.Update(StartMapScenarios.OneUpdate, _confirmPressed);

        Assert.IsType<WarpStep>(exploration.CurrentEventStep);
        Assert.Equal(1f, presenter.Blackness);
    }

    // A new game on the repository's content: Ragnir's intro opens with the voice, then three stills, then a warp.
    private static (ExplorationState Exploration, EventPresenter Presenter, VoiceChannel Voice) StartIntro()
    {
        var exploration = GameSession.StartNew(StartMapScenarios.RepositoryContent, "ragnir", new SeededRandom(1)).Exploration;
        var voice = new VoiceChannel();
        return (exploration, new EventPresenter(exploration, RepositoryScreenText.Text, new PlayerSettings(), voice), voice);
    }

    [Fact]
    public void Update_DuringLeadIn_ShowsNoStill()
    {
        var exploration = StartMapScenarios.StartExploring(StartMapScenarios.BelowChestShowingStills);
        exploration.TryInteract();
        var presenter = new EventPresenter(exploration, RepositoryScreenText.Text, new PlayerSettings(), new VoiceChannel());

        presenter.Update(StartMapScenarios.OneUpdate, default);
        presenter.Update(StillRun.LeadIn / 2, default);

        Assert.False(presenter.IsShowingStill);
        Assert.Equal(1f, presenter.Blackness);
    }

    [Fact]
    public void Update_FirstStillOver_SecondStartsAtOnceWithNoBlackHold()
    {
        var (_, presenter) = StartStills();

        presenter.Update(TimeSpan.FromSeconds(4), default);

        Assert.True(presenter.IsShowingStill);
        Assert.Equal(0f, presenter.Blackness);
    }

    [Fact]
    public void Update_LastStillOver_HoldsBlackForTheLeadOutBeforeTheDialogue()
    {
        var (exploration, presenter) = StartStills();
        presenter.Update(TimeSpan.FromSeconds(4), default);
        presenter.Update(TimeSpan.FromSeconds(4), default);
        Assert.IsType<DialogueStep>(exploration.CurrentEventStep);
        Assert.Equal(1f, presenter.Blackness);

        presenter.Update(StillRun.LeadOut, default);

        Assert.Equal(0f, presenter.Blackness);
    }

    // The chest's cutscene started, past its lead-in, with its first still just begun.
    private static (ExplorationState Exploration, EventPresenter Presenter) StartStills()
    {
        var exploration = StartMapScenarios.StartExploring(StartMapScenarios.BelowChestShowingStills);
        exploration.TryInteract();
        var presenter = new EventPresenter(exploration, RepositoryScreenText.Text, new PlayerSettings(), new VoiceChannel());
        presenter.Update(StartMapScenarios.OneUpdate, default);
        presenter.Update(StillRun.LeadIn, default);
        return (exploration, presenter);
    }
}
