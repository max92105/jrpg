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
        menu.Update(_oneUpdate, Pressing(GameButton.Down));
        menu.Update(_oneUpdate, Pressing(GameButton.Down));
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
        menu.Update(_oneUpdate, Pressing(GameButton.Down));
        menu.Update(_oneUpdate, Pressing(GameButton.Down));
        menu.Update(_oneUpdate, Pressing(GameButton.Confirm));

        var answering = menu.Update(_oneUpdate, PressingEscape);
        var leavingPage = menu.Update(_oneUpdate, PressingEscape);

        Assert.Null(answering);
        Assert.Null(leavingPage);
    }

    [Fact]
    public void Update_ConfirmOnTab_OpensThatPage()
    {
        var page = new CountingMenuPage();
        var menu = NewMenu(page);

        menu.Update(_oneUpdate, Pressing(GameButton.Confirm));

        Assert.Equal(1, page.OpenCount);
    }

    [Fact]
    public void Update_ConfirmOnTabAgainAfterCancel_OpensThePageAgain()
    {
        var page = new CountingMenuPage();
        var menu = NewMenu(page);
        menu.Update(_oneUpdate, Pressing(GameButton.Confirm));
        menu.Update(_oneUpdate, Pressing(GameButton.Cancel));

        menu.Update(_oneUpdate, Pressing(GameButton.Confirm));

        Assert.Equal(2, page.OpenCount);
    }

    [Fact]
    public void Hint_NoPageOpen_IsTheMainMenuHint()
    {
        var menu = NewMenu(new CountingMenuPage());

        Assert.Equal(RepositoryScreenText.Text.Messages.Text(MessageIds.MenuHintMain), menu.FooterHint);
    }

    [Fact]
    public void Hint_PageOpen_IsThePagesHint()
    {
        var page = new CountingMenuPage();
        var menu = NewMenu(page);

        menu.Update(_oneUpdate, Pressing(GameButton.Confirm));

        Assert.Equal(page.Hint, menu.FooterHint);
    }

    [Fact]
    public void Hint_PageClosed_GoesBackToTheMainMenuHint()
    {
        var menu = NewMenu(new CountingMenuPage());
        menu.Update(_oneUpdate, Pressing(GameButton.Confirm));

        menu.Update(_oneUpdate, Pressing(GameButton.Cancel));

        Assert.Equal(RepositoryScreenText.Text.Messages.Text(MessageIds.MenuHintMain), menu.FooterHint);
    }

    [Fact]
    public void ShowsSidebar_PageThatWantsIt_IsTrue()
    {
        var menu = NewMenu(new CountingMenuPage());

        menu.Update(_oneUpdate, Pressing(GameButton.Confirm));

        Assert.True(menu.ShowsSidebar);
    }

    [Fact]
    public void ShowsSidebar_PageThatShowsThePartyItself_IsFalse()
    {
        var menu = NewMenu(new CountingMenuPage { ShowsPartySidebar = false });

        menu.Update(_oneUpdate, Pressing(GameButton.Confirm));

        Assert.False(menu.ShowsSidebar);
    }

    [Fact]
    public void ShowsSidebar_MainMenu_IsFalse()
    {
        var menu = NewMenu(new CountingMenuPage());

        Assert.False(menu.ShowsSidebar);
    }

    private static GameMenuScene NewMenu()
    {
        var session = NewSession();
        var ui = new UiTextures();
        return new(RepositoryScreenText.Text, ui, FrameOf(session), [TestSystemPages.Create(session, ui)]);
    }

    private static GameMenuScene NewMenu(IMenuPage page) =>
        new(RepositoryScreenText.Text, new UiTextures(), FrameOf(NewSession()), [page]);

    private static MenuFrame FrameOf(GameSession session) =>
        new(new MenuHeader(RepositoryScreenText.Text, session), new PartySidebar(session.Party, RepositoryScreenText.Text.Messages));

    private static GameSession NewSession() => GameSession.StartNew(StartMapScenarios.BelowClosedDoor, new SeededRandom(1));
}
