using Jrpg.Game.Input;
using Jrpg.Game.Title;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Title;

public sealed class TitleMenuTests
{
    [Fact]
    public void Entries_ReadableSave_ListsContinueNewGameLoadQuit()
    {
        var menu = new TitleMenu(hasReadableSave: true);

        Assert.Equal([TitleEntry.Continue, TitleEntry.NewGame, TitleEntry.Load, TitleEntry.Quit], menu.Entries);
    }

    [Fact]
    public void Entries_NoReadableSave_ListsNewGameAndQuit()
    {
        var menu = new TitleMenu(hasReadableSave: false);

        Assert.Equal([TitleEntry.NewGame, TitleEntry.Quit], menu.Entries);
    }

    [Fact]
    public void Selected_ReadableSave_IsContinue()
    {
        var menu = new TitleMenu(hasReadableSave: true);

        Assert.Equal(TitleEntry.Continue, menu.Selected);
    }

    [Fact]
    public void Selected_NoReadableSave_IsNewGame()
    {
        var menu = new TitleMenu(hasReadableSave: false);

        Assert.Equal(TitleEntry.NewGame, menu.Selected);
    }

    [Fact]
    public void Update_DownFromContinue_SelectsNewGame()
    {
        var menu = new TitleMenu(hasReadableSave: true);

        menu.Update(Pressing(GameButton.Down));

        Assert.Equal(TitleEntry.NewGame, menu.Selected);
    }

    [Fact]
    public void Update_DownWithoutReadableSave_SelectsQuit()
    {
        var menu = new TitleMenu(hasReadableSave: false);

        menu.Update(Pressing(GameButton.Down));

        Assert.Equal(TitleEntry.Quit, menu.Selected);
    }

    [Fact]
    public void Update_UpFromContinue_WrapsToQuit()
    {
        var menu = new TitleMenu(hasReadableSave: true);

        menu.Update(Pressing(GameButton.Up));

        Assert.Equal(TitleEntry.Quit, menu.Selected);
    }

    [Fact]
    public void Update_NothingPressed_ReturnsNothing()
    {
        var menu = new TitleMenu(hasReadableSave: true);

        Assert.Null(menu.Update(default));
        Assert.Equal(TitleEntry.Continue, menu.Selected);
    }

    [Fact]
    public void Update_ConfirmWithReadableSave_ReturnsContinue()
    {
        var menu = new TitleMenu(hasReadableSave: true);

        var entry = menu.Update(Pressing(GameButton.Confirm));

        Assert.Equal(TitleEntry.Continue, entry);
    }

    [Fact]
    public void Update_ConfirmWithoutReadableSave_ReturnsNewGame()
    {
        var menu = new TitleMenu(hasReadableSave: false);

        var entry = menu.Update(Pressing(GameButton.Confirm));

        Assert.Equal(TitleEntry.NewGame, entry);
    }
}
