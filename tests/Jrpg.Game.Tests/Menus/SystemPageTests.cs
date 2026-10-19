using Jrpg.Game.Input;
using Jrpg.Game.Menus;
using Jrpg.Game.Scenes;
using Jrpg.Game.Tests.Ui;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Menus;

[Collection(RepositoryScreenText.CollectionName)]
public sealed class SystemPageTests
{
    [Fact]
    public void Update_ConfirmTitleScreen_RequestsTitle()
    {
        var page = new SystemPage(RepositoryScreenText.Text);

        var request = page.Update(Pressing(GameButton.Confirm));

        Assert.IsType<TitleRequested>(request);
    }

    [Fact]
    public void Update_ConfirmQuit_RequestsQuit()
    {
        var page = new SystemPage(RepositoryScreenText.Text);
        page.Update(Pressing(GameButton.Down));

        var request = page.Update(Pressing(GameButton.Confirm));

        Assert.IsType<QuitRequested>(request);
    }
}
