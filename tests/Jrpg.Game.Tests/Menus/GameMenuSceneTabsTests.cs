using Jrpg.Game.Input;
using Jrpg.Game.Menus;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Menus;

// The tabs fold into their icon rail while a page is open, and the page's area follows their edge (decision 286).
[Collection(RepositoryScreenText.CollectionName)]
public sealed class GameMenuSceneTabsTests
{
    private static readonly TimeSpan _oneUpdate = TimeSpan.FromSeconds(1.0 / 60);
    private static readonly TimeSpan _longAfter = TimeSpan.FromSeconds(1);

    [Fact]
    public void Update_HalfwayIntoAPage_PageAreaLeftIsBetweenTabsAndRail()
    {
        var menu = NewMenu();
        menu.Update(_oneUpdate, Pressing(GameButton.Confirm));

        menu.Update(TabFold.Duration / 2, default);

        Assert.InRange(menu.PageArea.X, MenuLayout.PageArea(TabFold.RailWidth).X + 1, MenuLayout.PageArea(NavTabShape.Width).X - 1);
    }

    [Fact]
    public void Update_LongAfterEnteringAPage_PageAreaStartsBesideTheRail()
    {
        var menu = NewMenu();
        menu.Update(_oneUpdate, Pressing(GameButton.Confirm));

        menu.Update(_longAfter, default);

        Assert.Equal(MenuLayout.PageArea(TabFold.RailWidth), menu.PageArea);
    }

    [Fact]
    public void Update_LongAfterLeavingAPage_PageAreaStartsBesideTheFullTabs()
    {
        var menu = NewMenu();
        menu.Update(_oneUpdate, Pressing(GameButton.Confirm));
        menu.Update(_longAfter, default);
        Assert.Equal(MenuLayout.PageArea(TabFold.RailWidth), menu.PageArea);
        menu.Update(_oneUpdate, Pressing(GameButton.Cancel));

        menu.Update(_longAfter, default);

        Assert.Equal(MenuLayout.PageArea(NavTabShape.Width), menu.PageArea);
    }

    private static GameMenuScene NewMenu()
    {
        var session = StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowClosedDoor);
        return new(RepositoryScreenText.Text, new UiTextures(), new MenuHeader(RepositoryScreenText.Text, session), [new CountingMenuPage()]);
    }
}
