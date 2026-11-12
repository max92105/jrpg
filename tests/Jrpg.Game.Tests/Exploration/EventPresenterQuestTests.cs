using Jrpg.Core.Events;
using Jrpg.Core.Quests;
using Jrpg.Game.Dialogue;
using Jrpg.Game.Exploration;
using Jrpg.Game.Hosting;
using Jrpg.Game.Input;
using Jrpg.Game.Tests.Ui;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Exploration;

// A quest step that changed the log is read as one page, its line naming the quest (decision 387).
[Collection(RepositoryScreenText.CollectionName)]
public sealed class EventPresenterQuestTests
{
    [Fact]
    public void Update_StartQuestStep_ShowsTheStartedLine()
    {
        var exploration = StartMapScenarios.StartExploring(StartMapScenarios.BelowQuestGiver);
        Assert.True(exploration.TryInteract());
        var presenter = EventPresenters.Over(exploration, settings: new PlayerSettings { TextSpeed = TextSpeed.Instant });
        presenter.Update(StartMapScenarios.OneUpdate, default);
        ReadThePage(presenter);
        Assert.IsType<StartQuestStep>(exploration.CurrentEventStep);

        presenter.Update(StartMapScenarios.OneUpdate, default);

        Assert.True(presenter.IsShowingMessage);
        var change = exploration.CurrentQuestChange!;
        Assert.Equal(QuestChangeKind.Started, change.Kind);
        Assert.Equal(QuestNoticeText.Of(change, RepositoryScreenText.Text.Messages), presenter.ShownLine);
        ReadThePage(presenter);
        Assert.False(exploration.IsEventRunning);
    }

    // The page is shown on one update and turned on the next Confirm (the press that opened it can't also turn it).
    private static void ReadThePage(EventPresenter presenter)
    {
        presenter.Update(StartMapScenarios.OneUpdate, default);
        presenter.Update(StartMapScenarios.OneUpdate, Pressing(GameButton.Confirm));
    }
}
