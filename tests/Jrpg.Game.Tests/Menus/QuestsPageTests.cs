using Jrpg.Core.Quests;
using Jrpg.Core.Randomness;
using Jrpg.Core.Session;
using Jrpg.Game.Menus;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Ui;

namespace Jrpg.Game.Tests.Menus;

// The Quests page reads the journal when it opens (decision 393): the active quests' titles, or nothing.
[Collection(RepositoryScreenText.CollectionName)]
public sealed class QuestsPageTests
{
    [Fact]
    public void Open_ActiveQuest_ListsItsTitle()
    {
        var session = WithTheErrandStarted();
        var page = new QuestsPage(RepositoryScreenText.Text, session);

        page.Open();

        Assert.Equal([session.Catalog.Quests["placeholder_errand"].Title], page.ShownTitles);
    }

    [Fact]
    public void Open_NoQuest_ShowsNoTitle()
    {
        var page = new QuestsPage(RepositoryScreenText.Text, StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowQuestGiver));

        page.Open();

        Assert.Empty(page.ShownTitles);
    }

    // The giver's talk starts the errand; the page reads the journal again on its next opening.
    [Fact]
    public void Open_AfterAQuestStarted_ListsItTheNextTime()
    {
        var session = StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowQuestGiver);
        var page = new QuestsPage(RepositoryScreenText.Text, session);
        page.Open();
        Assert.Empty(page.ShownTitles);
        Assert.True(session.Exploration.TryInteract());
        session.Exploration.ContinueEvent();

        page.Open();

        Assert.Single(page.ShownTitles);
    }

    [Fact]
    public void Glyph_IsTheQuestsGlyph()
    {
        Assert.Equal(Jrpg.Game.Ui.TabGlyph.Quests, NewPage(StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowQuestGiver)).Glyph);
    }

    [Fact]
    public void LeavesOnCancel_Always_IsTrue()
    {
        Assert.True(NewPage(StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowQuestGiver)).LeavesOnCancel);
    }

    private static QuestsPage NewPage(GameSession session) => new(RepositoryScreenText.Text, session);

    private static GameSession WithTheErrandStarted()
    {
        var session = StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowQuestGiver);
        var save = session.Capture(DateTimeOffset.UnixEpoch) with { Quests = [new QuestProgress("placeholder_errand", "ask_innkeeper", IsCompleted: false)] };
        return GameSession.Restore(save, StartMapScenarios.BelowQuestGiver, new SeededRandom(1));
    }
}
