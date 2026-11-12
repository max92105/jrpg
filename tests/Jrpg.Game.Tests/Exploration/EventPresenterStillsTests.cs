using Jrpg.Core.Events;
using Jrpg.Core.Exploration;
using Jrpg.Game.Exploration;
using Jrpg.Game.Input;
using Jrpg.Game.Tests.Input;
using Jrpg.Game.Tests.Ui;

namespace Jrpg.Game.Tests.Exploration;

/// <summary>
/// Cutscene stills over the map: the lead-in from black, each still's time, Confirm's skip and the hold on black
/// before the dialogue begins.
/// </summary>
[Collection(RepositoryScreenText.CollectionName)]
public sealed class EventPresenterStillsTests
{
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

        presenter.Update(StartMapScenarios.OneUpdate, InputFrames.Pressing(GameButton.Confirm));

        Assert.IsType<DialogueStep>(exploration.CurrentEventStep);
    }

    [Fact]
    public void Update_ConfirmRightAfterTheSkip_DoesNotEndTheDialogue()
    {
        var (exploration, presenter) = StartStills();
        presenter.Update(StartMapScenarios.OneUpdate, InputFrames.Pressing(GameButton.Confirm));

        presenter.Update(StartMapScenarios.OneUpdate, InputFrames.Pressing(GameButton.Confirm));

        Assert.IsType<DialogueStep>(exploration.CurrentEventStep);
    }

    [Fact]
    public void Update_ConfirmOnFirstStillStep_StartsTheLeadIn()
    {
        var exploration = StartMapScenarios.StartExploring(StartMapScenarios.BelowChestShowingStills);
        exploration.TryInteract();
        var presenter = EventPresenters.Over(exploration);

        presenter.Update(StartMapScenarios.OneUpdate, InputFrames.Pressing(GameButton.Confirm));

        Assert.IsType<StillStep>(exploration.CurrentEventStep);
        Assert.Equal(1f, presenter.Blackness);
    }

    [Fact]
    public void Update_ConfirmDuringLeadIn_SkipsToTheDialogue()
    {
        var exploration = StartMapScenarios.StartExploring(StartMapScenarios.BelowChestShowingStills);
        exploration.TryInteract();
        var presenter = EventPresenters.Over(exploration);
        presenter.Update(StartMapScenarios.OneUpdate, default);

        presenter.Update(StartMapScenarios.OneUpdate, InputFrames.Pressing(GameButton.Confirm));

        Assert.IsType<DialogueStep>(exploration.CurrentEventStep);
    }

    [Fact]
    public void Update_DuringLeadIn_ShowsNoStill()
    {
        var exploration = StartMapScenarios.StartExploring(StartMapScenarios.BelowChestShowingStills);
        exploration.TryInteract();
        var presenter = EventPresenters.Over(exploration);

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
        var presenter = EventPresenters.Over(exploration);
        presenter.Update(StartMapScenarios.OneUpdate, default);
        presenter.Update(StillRun.LeadIn, default);
        return (exploration, presenter);
    }
}
