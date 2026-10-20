using Jrpg.Game.Input;
using Jrpg.Game.Title;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Title;

public sealed class TitleMenuTests
{
    [Fact]
    public void Update_ConfirmFirstEntry_ReturnsNewGame()
    {
        var menu = new TitleMenu(hasReadableSave: true);

        var entry = menu.Update(Pressing(GameButton.Confirm));

        Assert.Equal(TitleEntry.NewGame, entry);
    }

    [Fact]
    public void Constructor_NoReadableSave_ContinueAndLoadDisabled()
    {
        var menu = new TitleMenu(hasReadableSave: false);

        Assert.False(menu.IsEnabled(TitleEntry.Continue));
        Assert.False(menu.IsEnabled(TitleEntry.Load));
        Assert.True(menu.IsEnabled(TitleEntry.Quit));
    }

    [Fact]
    public void Update_DownWithoutReadableSave_SkipsToQuit()
    {
        var menu = new TitleMenu(hasReadableSave: false);

        menu.Update(Pressing(GameButton.Down));

        Assert.Equal(TitleEntry.Quit, menu.Selected);
    }

    [Fact]
    public void Update_DownWithReadableSave_SelectsContinue()
    {
        var menu = new TitleMenu(hasReadableSave: true);

        menu.Update(Pressing(GameButton.Down));

        Assert.Equal(TitleEntry.Continue, menu.Selected);
    }

    [Fact]
    public void Update_NothingPressed_ReturnsNothing()
    {
        var menu = new TitleMenu(hasReadableSave: true);

        Assert.Null(menu.Update(default));
        Assert.Equal(TitleEntry.NewGame, menu.Selected);
    }
}
