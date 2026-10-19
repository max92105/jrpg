using Jrpg.Game.Input;
using Jrpg.Game.Scenes;
using Jrpg.Game.Tests.Input;
using Jrpg.Game.Title;

namespace Jrpg.Game.Tests.Title;

public sealed class TitleMenuTests
{
    [Fact]
    public void Update_ConfirmNewGame_RequestsNewGame()
    {
        var menu = new TitleMenu();

        var request = menu.Update(InputFrames.Pressing(GameButton.Confirm));

        Assert.IsType<NewGameRequested>(request);
    }

    [Fact]
    public void Update_ConfirmQuit_RequestsQuit()
    {
        var menu = new TitleMenu();
        menu.Update(InputFrames.Pressing(GameButton.Down));

        var request = menu.Update(InputFrames.Pressing(GameButton.Confirm));

        Assert.IsType<QuitRequested>(request);
    }

    [Fact]
    public void Update_NothingPressed_RequestsNothing()
    {
        var menu = new TitleMenu();

        var request = menu.Update(default);

        Assert.Null(request);
        Assert.Equal(TitleEntry.NewGame, menu.Selected);
    }
}
