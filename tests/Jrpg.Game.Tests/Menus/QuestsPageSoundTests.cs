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

// A read-only page: only a selection that really changed ticks (decision 421).
[Collection(RepositoryScreenText.CollectionName)]
public sealed class QuestsPageSoundTests
{
    private readonly QuestsPage _page;

    public QuestsPageSoundTests()
    {
        var session = StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowQuestGiver);
        var save = session.Capture(DateTimeOffset.UnixEpoch) with { Quests = [new QuestProgress("placeholder_errand", "ask_innkeeper", false)] };
        _page = new QuestsPage(RepositoryScreenText.Text, new UiTextures(), GameSession.Restore(save, StartMapScenarios.BelowQuestGiver, new SeededRandom(1)));
        _page.Open();
    }

    [Fact]
    public void Update_RightToTheNextCategory_AsksTheMoveSound()
    {
        _page.Update(Pressing(GameButton.Right));

        Assert.Equal([UiSoundFiles.Effects.MenuMove], _page.Sounds);
    }

    [Fact]
    public void Update_NothingPressedAfterAMove_AsksNothing()
    {
        _page.Update(Pressing(GameButton.Right));

        _page.Update(default);

        Assert.Empty(_page.Sounds);
    }

    // One quest in the list, so Down stays on it.
    [Fact]
    public void Update_DownOnTheOnlyQuest_AsksNothing()
    {
        _page.Update(Pressing(GameButton.Down));

        Assert.Empty(_page.Sounds);
    }
}
