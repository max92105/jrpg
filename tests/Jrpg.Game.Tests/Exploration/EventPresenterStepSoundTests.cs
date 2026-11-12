using Jrpg.Core.Content;
using Jrpg.Core.Events;
using Jrpg.Core.Exploration;
using Jrpg.Game.Exploration;
using Jrpg.Game.Hosting;
using Jrpg.Game.Input;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Exploration;

// A found item, a recruit and a quest's change sound on the update their line shows; a found item only when something
// was carried (decision 421), a completed quest its own sound (decision 433), a dialogue nothing but its typing.
[Collection(RepositoryScreenText.CollectionName)]
public sealed class EventPresenterStepSoundTests
{
    private const string Quest = "placeholder_errand";

    [Fact]
    public void Update_GiveItemStepShows_AsksTheItemSound()
    {
        var presenter = PresenterAtTheChest(new GiveItemStep("potion", 1));

        presenter.Update(StartMapScenarios.OneUpdate, default);

        Assert.Equal([UiSoundFiles.Field.ItemGot], presenter.Sounds);
    }

    // The first gift fills the stack, so the second carries nothing: its "couldn't carry it" line is silent.
    [Fact]
    public void Update_GiveItemNothingCarried_AsksNothing()
    {
        var (presenter, exploration) = PresenterAtTheChestWith(new GiveItemStep("potion", 999), new GiveItemStep("potion", 1));
        presenter.Update(StartMapScenarios.OneUpdate, default);
        ReadThePage(presenter);

        presenter.Update(StartMapScenarios.OneUpdate, default);

        Assert.Equal(0, exploration.CurrentItemReceipt!.Added);
        Assert.True(presenter.IsShowingMessage);
        Assert.Empty(presenter.Sounds);
    }

    [Fact]
    public void Update_GiveItemLineStillShowing_AsksNothing()
    {
        var presenter = PresenterAtTheChest(new GiveItemStep("potion", 1));
        presenter.Update(StartMapScenarios.OneUpdate, default);

        presenter.Update(StartMapScenarios.OneUpdate, default);

        Assert.True(presenter.IsShowingMessage);
        Assert.Empty(presenter.Sounds);
    }

    [Fact]
    public void Update_RecruitStepShows_AsksTheRecruitSound()
    {
        var presenter = PresenterAtTheChest(new RecruitStep("jazog"));

        presenter.Update(StartMapScenarios.OneUpdate, default);

        Assert.Equal([UiSoundFiles.Field.Recruit], presenter.Sounds);
    }

    // The second recruit finds the member already in the party: its line shows, but nobody joined.
    [Fact]
    public void Update_RecruitAlreadyInParty_AsksNothing()
    {
        var (presenter, exploration) = PresenterAtTheChestWith(new RecruitStep("jazog"), new RecruitStep("jazog"));
        presenter.Update(StartMapScenarios.OneUpdate, default);
        ReadThePage(presenter);

        presenter.Update(StartMapScenarios.OneUpdate, default);

        Assert.False(exploration.CurrentRecruitment!.HasJoined);
        Assert.True(presenter.IsShowingMessage);
        Assert.Empty(presenter.Sounds);
    }

    [Fact]
    public void Update_StartQuestStepShows_AsksTheQuestUpdatedSound()
    {
        var presenter = PresenterAtTheChest(new StartQuestStep(Quest));

        presenter.Update(StartMapScenarios.OneUpdate, default);

        Assert.Equal([UiSoundFiles.Field.QuestUpdated], presenter.Sounds);
    }

    [Fact]
    public void Update_AdvanceQuestStepShows_AsksTheQuestUpdatedSound()
    {
        var (presenter, exploration) = PresenterAtTheChestWith(new StartQuestStep(Quest), new AdvanceQuestStep(Quest, "return_to_giver"));
        presenter.Update(StartMapScenarios.OneUpdate, default);
        ReadThePage(presenter);

        presenter.Update(StartMapScenarios.OneUpdate, default);

        Assert.IsType<AdvanceQuestStep>(exploration.CurrentEventStep);
        Assert.Equal([UiSoundFiles.Field.QuestUpdated], presenter.Sounds);
    }

    [Fact]
    public void Update_CompleteQuestStepShows_AsksTheQuestCompletedSound()
    {
        var (presenter, exploration) = PresenterAtTheChestWith(new StartQuestStep(Quest), new CompleteQuestStep(Quest));
        presenter.Update(StartMapScenarios.OneUpdate, default);
        ReadThePage(presenter);

        presenter.Update(StartMapScenarios.OneUpdate, default);

        Assert.IsType<CompleteQuestStep>(exploration.CurrentEventStep);
        Assert.Equal([UiSoundFiles.Field.QuestCompleted], presenter.Sounds);
    }

    // Shown at once, so no tick either: only the page steps that report a change sound as they show.
    [Fact]
    public void Update_DialogueStepShows_AsksNothing()
    {
        var presenter = PresenterAtTheChest(new DialogueStep(new(["A line."])));

        presenter.Update(StartMapScenarios.OneUpdate, default);

        Assert.True(presenter.IsShowingMessage);
        Assert.Empty(presenter.Sounds);
    }

    private static EventPresenter PresenterAtTheChest(EventStep step) => PresenterAtTheChestWith(step).Presenter;

    // Instant text, so one Confirm turns a page.
    private static (EventPresenter Presenter, ExplorationState Exploration) PresenterAtTheChestWith(params EventStep[] steps)
    {
        ContentCatalog catalog = StartMapScenarios.WithChestSaying(StartMapScenarios.BelowChest, steps);
        var exploration = StartMapScenarios.StartExploring(catalog);
        Assert.True(exploration.TryInteract());
        var presenter = EventPresenters.Over(exploration, settings: new PlayerSettings { TextSpeed = TextSpeed.Instant });
        return (presenter, exploration);
    }

    // The page is shown on one update and turned on the next Confirm (the press that opened it can't also turn it).
    private static void ReadThePage(EventPresenter presenter)
    {
        presenter.Update(StartMapScenarios.OneUpdate, default);
        presenter.Update(StartMapScenarios.OneUpdate, Pressing(GameButton.Confirm));
    }
}
