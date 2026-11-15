using Jrpg.Core.Content;
using Jrpg.Core.Events;
using Jrpg.Core.Exploration;
using Jrpg.Game.Exploration;
using Jrpg.Game.Input;
using Jrpg.Game.Tests.Input;
using Jrpg.Game.Tests.Ui;

namespace Jrpg.Game.Tests.Exploration;

/// <summary>
/// A cinematic step over the map (ADR 0046): black while its pictures load and a moment more, each still for its
/// seconds, black after the last before what follows; at the game's 1/60 s step.
/// </summary>
[Collection(RepositoryScreenText.CollectionName)]
public sealed class EventPresenterCinematicTests
{
    private const int UpdatesASecond = 60;

    [Fact]
    public void Update_StillsNotLoaded_LeadInDoesNotCount()
    {
        var (_, presenter) = Start(StartMapScenarios.BelowChestPlayingCinematic);

        Run(presenter, CinematicRun.LeadIn * 4);

        Assert.Null(presenter.ShownCinematicImage);
        Assert.Equal(1f, presenter.Blackness);
    }

    [Fact]
    public void Update_PastLoadingAndTheLeadIn_ShowsTheFirstStill()
    {
        var (exploration, presenter) = Start(StartMapScenarios.BelowChestPlayingCinematic);
        presenter.MarkStillsLoaded();

        Run(presenter, CinematicRun.LeadIn + StartMapScenarios.OneUpdate);

        Assert.Equal("a.png", presenter.ShownCinematicImage);
        Assert.IsType<CinematicStep>(exploration.CurrentEventStep);
    }

    [Fact]
    public void Update_PastTheFirstStillsSeconds_ShowsTheSecond()
    {
        var (_, presenter) = StartPastTheLeadIn(StartMapScenarios.BelowChestPlayingCinematic);

        Run(presenter, TimeSpan.FromSeconds(4));

        Assert.Equal("b.png", presenter.ShownCinematicImage);
        Assert.True(presenter.Blackness < 1f);
    }

    // Decision 623: only a hold skips a cinematic.
    [Fact]
    public void Update_ConfirmPressedDuringAStill_DoesNothing()
    {
        var (exploration, presenter) = StartPastTheLeadIn(StartMapScenarios.BelowChestPlayingCinematic);

        presenter.Update(StartMapScenarios.OneUpdate, InputFrames.Pressing(GameButton.Confirm));

        Assert.Equal("a.png", presenter.ShownCinematicImage);
        Assert.IsType<CinematicStep>(exploration.CurrentEventStep);
    }

    [Fact]
    public void Update_LastStillOver_HoldsBlackForTheLeadOutBeforeTheDialogue()
    {
        var (exploration, presenter) = StartPastTheLeadIn(StartMapScenarios.BelowChestPlayingCinematic);
        Run(presenter, TimeSpan.FromSeconds(8) + StartMapScenarios.OneUpdate);
        Assert.IsType<DialogueStep>(exploration.CurrentEventStep);
        Assert.Equal(1f, presenter.Blackness);

        Run(presenter, CinematicRun.LeadOut + StartMapScenarios.OneUpdate);

        Assert.Equal(0f, presenter.Blackness);
    }

    // Black from the last still's end until the warp has moved the player: the start map never shows again.
    [Fact]
    public void Update_CinematicThenWarp_WarpStartsAlreadyBlack()
    {
        var (exploration, presenter) = StartPastTheLeadIn(StartMapScenarios.BelowChestPlayingCinematicIntoAWarp);
        Run(presenter, TimeSpan.FromSeconds(8) + StartMapScenarios.OneUpdate);

        for (var update = 0; update < 2 * UpdatesASecond && exploration.CurrentMap.Id == "start"; update++)
        {
            Assert.Equal(1f, presenter.Blackness);
            presenter.Update(StartMapScenarios.OneUpdate, default);
        }

        Assert.Equal("forest", exploration.CurrentMap.Id);
    }

    // The chest's event started, its first update taken: the cinematic is loading its pictures.
    private static (ExplorationState Exploration, EventPresenter Presenter) Start(ContentCatalog catalog)
    {
        var exploration = StartMapScenarios.StartExploring(catalog);
        exploration.TryInteract();
        var presenter = EventPresenters.Over(exploration);
        presenter.Update(StartMapScenarios.OneUpdate, default);
        return (exploration, presenter);
    }

    // Loaded and past the black before the first still, which has just begun.
    private static (ExplorationState Exploration, EventPresenter Presenter) StartPastTheLeadIn(ContentCatalog catalog)
    {
        var (exploration, presenter) = Start(catalog);
        presenter.MarkStillsLoaded();
        Run(presenter, CinematicRun.LeadIn + StartMapScenarios.OneUpdate);
        return (exploration, presenter);
    }

    // At the game's fixed step, so no phase is jumped over.
    private static void Run(EventPresenter presenter, TimeSpan time)
    {
        for (var elapsed = TimeSpan.Zero; elapsed < time; elapsed += StartMapScenarios.OneUpdate)
        {
            presenter.Update(StartMapScenarios.OneUpdate, default);
        }
    }
}
