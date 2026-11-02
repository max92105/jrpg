using Jrpg.Core.Randomness;
using Jrpg.Core.Session;
using Jrpg.Game.Input;
using Jrpg.Game.Menus;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Menus;

// A read-only page: only a member that really changed ticks (decision 421).
[Collection(RepositoryScreenText.CollectionName)]
public sealed class StatusPageSoundTests
{
    [Fact]
    public void Update_RightToTheNextMember_AsksTheMoveSound()
    {
        var page = PageOf(StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowClosedDoor));

        page.Update(Pressing(GameButton.Right));

        Assert.Equal([UiSoundFiles.Effects.MenuMove], page.Sounds);
    }

    [Fact]
    public void Update_NothingPressedAfterAMove_AsksNothing()
    {
        var page = PageOf(StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowClosedDoor));
        page.Update(Pressing(GameButton.Right));

        page.Update(default);

        Assert.Empty(page.Sounds);
    }

    [Fact]
    public void Update_RightWithOneMember_AsksNothing()
    {
        var page = PageOf(GameSession.StartNew(StartMapScenarios.BelowClosedDoor, "sarxian", new SeededRandom(1)));

        page.Update(Pressing(GameButton.Right));

        Assert.Empty(page.Sounds);
    }

    private static StatusPage PageOf(GameSession session)
    {
        var page = new StatusPage(RepositoryScreenText.Text, new UiTextures(), session, StartMapScenarios.BelowClosedDoor);
        page.Open();
        return page;
    }
}
