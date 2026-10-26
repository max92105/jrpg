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
    private static readonly TimeSpan _closingFade = TimeSpan.FromSeconds(0.12);

    [Fact]
    public void Update_CancelOnTabs_RequestsMenuClosed()
    {
        var menu = NewMenu();

        var request = CloseAndWait(menu, Pressing(GameButton.Cancel));

        Assert.IsType<MenuClosed>(request);
    }

    [Fact]
    public void Update_MenuOnTabs_RequestsMenuClosed()
    {
        var menu = NewMenu();

        var request = CloseAndWait(menu, Pressing(GameButton.Menu));

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
        var onTabs = CloseAndWait(menu, Pressing(GameButton.Cancel));

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
    public void Update_RightOnStatus_MemberColumnShowsTheNextMember()
    {
        var session = NewSession();
        var menu = NewMenu(session, new StatusPage(RepositoryScreenText.Text, new UiTextures(), session, StartMapScenarios.BelowClosedDoor));
        menu.Update(_oneUpdate, Pressing(GameButton.Confirm));

        menu.Update(_oneUpdate, Pressing(GameButton.Right));

        Assert.Equal(1, menu.MemberColumn.ShownIndex);
    }

    [Fact]
    public void Update_CancelOnTabs_RequestsMenuClosedOnceFaded()
    {
        var menu = NewMenu();

        var onPress = menu.Update(_oneUpdate, Pressing(GameButton.Cancel));
        var faded = menu.Update(_closingFade, default);
        var after = menu.Update(_oneUpdate, default);

        Assert.Null(onPress);
        Assert.IsType<MenuClosed>(faded);
        Assert.Null(after);
    }

    [Fact]
    public void Update_ConfirmWhileClosing_DoesNothing()
    {
        var page = new CountingMenuPage();
        var menu = NewMenu(page);
        menu.Update(_oneUpdate, Pressing(GameButton.Cancel));

        menu.Update(_oneUpdate, Pressing(GameButton.Confirm));
        var faded = menu.Update(_closingFade, default);

        Assert.Equal(0, page.OpenCount);
        Assert.IsType<MenuClosed>(faded);
    }

    [Fact]
    public void Update_ConfirmDuringSlideIn_OpensThePage()
    {
        var page = new CountingMenuPage();
        var menu = NewMenu(page);

        menu.Update(_oneUpdate, Pressing(GameButton.Confirm));

        Assert.Equal(1, page.OpenCount);
    }

    [Fact]
    public void Update_PageOpen_AdvancesThePageBeforeItsInput()
    {
        var page = new CountingMenuPage();
        var menu = NewMenu(page);
        menu.Update(_oneUpdate, Pressing(GameButton.Confirm));

        menu.Update(_oneUpdate, default);

        Assert.Equal(["Advance", "Update"], page.Calls);
        Assert.Equal(_oneUpdate, page.LastAdvance);
    }

    // The press starts the closing fade; MenuClosed comes once it has run.
    private static SceneRequest? CloseAndWait(GameMenuScene menu, InputFrame press)
    {
        menu.Update(_oneUpdate, press);
        return menu.Update(_closingFade, default);
    }

    private static GameMenuScene NewMenu(GameSession session, params IMenuPage[] pages) =>
        new(RepositoryScreenText.Text, new UiTextures(), HeaderOf(session), [.. pages]);

    private static GameMenuScene NewMenu()
    {
        var session = NewSession();
        var ui = new UiTextures();
        return new(RepositoryScreenText.Text, ui, HeaderOf(session), [TestSystemPages.Create(session, ui)]);
    }

    private static GameMenuScene NewMenu(IMenuPage page) =>
        new(RepositoryScreenText.Text, new UiTextures(), HeaderOf(NewSession()), [page]);

    private static MenuHeader HeaderOf(GameSession session) => new(RepositoryScreenText.Text, session);

    private static GameSession NewSession() => StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowClosedDoor);
}
