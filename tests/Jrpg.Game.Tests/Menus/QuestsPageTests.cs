using Jrpg.Core.Quests;
using Jrpg.Core.Randomness;
using Jrpg.Core.Session;
using Jrpg.Game.Input;
using Jrpg.Game.Menus;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Menus;

// The Quests page reads the journal when it opens (decision 393): the active quests' titles, or nothing, and the chosen
// quest's detail (decision 392).
[Collection(RepositoryScreenText.CollectionName)]
public sealed class QuestsPageTests
{
    [Fact]
    public void Open_ActiveQuest_ListsItsTitle()
    {
        var session = WithTheErrandAt("ask_innkeeper");
        var page = NewPage(session);

        page.Open();

        var errand = session.Catalog.Quests["placeholder_errand"];
        Assert.Equal([errand.Title], page.ShownTitles);
        Assert.Equal(errand.Stage("ask_innkeeper").Text, page.Detail!.StageText);
    }

    [Fact]
    public void Open_NoQuest_ShowsNoTitleAndNoDetail()
    {
        var page = NewPage(StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowQuestGiver));

        page.Open();

        Assert.Empty(page.ShownTitles);
        Assert.Null(page.Detail);
    }

    // The giver's talk starts the errand; the page reads the journal again on its next opening.
    [Fact]
    public void Open_AfterAQuestStarted_ListsItTheNextTime()
    {
        var session = StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowQuestGiver);
        var page = NewPage(session);
        page.Open();
        Assert.Empty(page.ShownTitles);
        Assert.True(session.Exploration.TryInteract());
        session.Exploration.ContinueEvent();

        page.Open();

        Assert.Single(page.ShownTitles);
    }

    [Fact]
    public void Open_QuestAtItsSecondStage_DetailSaysThatStage()
    {
        var session = WithTheErrandAt("return_to_giver");
        var errand = session.Catalog.Quests["placeholder_errand"];
        var page = NewPage(session);

        page.Open();

        var detail = page.Detail!;
        Assert.Equal(errand.Stage("return_to_giver").Text, detail.StageText);
        Assert.Equal(RepositoryScreenText.Text.Messages.Text(MessageIds.QuestKindSide), detail.KindName);
    }

    [Fact]
    public void Update_Right_ShowsTheCompletedErrandWithItsNote()
    {
        var session = WithTheErrandCompletedAt("return_to_giver");
        var page = NewPage(session);
        page.Open();
        Assert.Empty(page.ShownTitles);

        page.Update(Pressing(GameButton.Right));

        Assert.Equal(QuestCategory.Completed, page.Category);
        Assert.Single(page.ShownTitles);
        Assert.Equal(RepositoryScreenText.Text.Messages.Text(MessageIds.QuestCompletedNote), page.Detail!.StageText);
        page.Update(Pressing(GameButton.Left));
        Assert.Equal(QuestCategory.Active, page.Category);
        Assert.Null(page.Detail);
    }

    [Fact]
    public void Glyph_IsTheQuestsGlyph()
    {
        Assert.Equal(TabGlyph.Quests, NewPage(StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowQuestGiver)).Glyph);
    }

    [Fact]
    public void LeavesOnCancel_Always_IsTrue()
    {
        Assert.True(NewPage(StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowQuestGiver)).LeavesOnCancel);
    }

    private static QuestsPage NewPage(GameSession session) => new(RepositoryScreenText.Text, new UiTextures(), session);

    private static GameSession WithTheErrandAt(string stageId) => WithTheErrand(new QuestProgress("placeholder_errand", stageId, IsCompleted: false));

    private static GameSession WithTheErrandCompletedAt(string stageId) => WithTheErrand(new QuestProgress("placeholder_errand", stageId, IsCompleted: true));

    private static GameSession WithTheErrand(QuestProgress progress)
    {
        var session = StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowQuestGiver);
        var save = session.Capture(DateTimeOffset.UnixEpoch) with { Quests = [progress] };
        return GameSession.Restore(save, StartMapScenarios.BelowQuestGiver, new SeededRandom(1));
    }
}
