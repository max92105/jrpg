using Jrpg.Core.Randomness;
using Jrpg.Core.Session;
using Jrpg.Game.Input;
using Jrpg.Game.Menus;
using Jrpg.Game.Scenes;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Menus;

[Collection(RepositoryScreenText.CollectionName)]
public sealed class GameMenuSceneTests
{
    private static readonly TimeSpan _oneUpdate = TimeSpan.FromSeconds(1.0 / 60);

    [Fact]
    public void Update_CancelOnTabs_RequestsMenuClosed()
    {
        var menu = NewMenu();

        var request = menu.Update(_oneUpdate, Pressing(GameButton.Cancel));

        Assert.IsType<MenuClosed>(request);
    }

    [Fact]
    public void Update_MenuOnTabs_RequestsMenuClosed()
    {
        var menu = NewMenu();

        var request = menu.Update(_oneUpdate, Pressing(GameButton.Menu));

        Assert.IsType<MenuClosed>(request);
    }

    [Fact]
    public void Update_YesInSystemPage_RequestsTitle()
    {
        var menu = NewMenu();

        var entering = menu.Update(_oneUpdate, Pressing(GameButton.Confirm));
        menu.Update(_oneUpdate, Pressing(GameButton.Confirm));
        menu.Update(_oneUpdate, Pressing(GameButton.Left));
        var inPage = menu.Update(_oneUpdate, Pressing(GameButton.Confirm));

        Assert.Null(entering);
        Assert.IsType<TitleRequested>(inPage);
    }

    [Fact]
    public void Update_EscapeInPage_ReturnsToTabsWithoutClosing()
    {
        var menu = NewMenu();
        menu.Update(_oneUpdate, Pressing(GameButton.Confirm));

        var leaving = menu.Update(_oneUpdate, PressingEscape);
        var onTabs = menu.Update(_oneUpdate, Pressing(GameButton.Cancel));

        Assert.Null(leaving);
        Assert.IsType<MenuClosed>(onTabs);
    }

    [Fact]
    public void Update_EscapeWhileAsking_AnswersNoAndStaysInPage()
    {
        var menu = NewMenu();
        menu.Update(_oneUpdate, Pressing(GameButton.Confirm));
        menu.Update(_oneUpdate, Pressing(GameButton.Confirm));

        var answering = menu.Update(_oneUpdate, PressingEscape);
        var leavingPage = menu.Update(_oneUpdate, PressingEscape);

        Assert.Null(answering);
        Assert.Null(leavingPage);
    }

    private static GameMenuScene NewMenu()
    {
        var session = GameSession.StartNew(StartMapScenarios.BelowClosedDoor, new SeededRandom(1));
        var header = new MenuHeader(RepositoryScreenText.Text, session);
        return new(RepositoryScreenText.Text, new UiTextures(), header, [new SystemPage(RepositoryScreenText.Text, new UiTextures())]);
    }
}
