using Jrpg.Game.Input;
using Jrpg.Game.Menus;
using Jrpg.Game.Scenes;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Menus;

[Collection(RepositoryScreenText.CollectionName)]
public sealed class SystemPageTests
{
    [Fact]
    public void Update_ConfirmQuit_AsksBeforeRequesting()
    {
        var page = NewPage();
        page.Update(Pressing(GameButton.Down));

        var request = page.Update(Pressing(GameButton.Confirm));

        Assert.Null(request);
        Assert.False(page.LeavesOnCancel);
    }

    [Fact]
    public void Update_YesToQuit_RequestsQuit()
    {
        var page = NewPage();
        page.Update(Pressing(GameButton.Down));
        page.Update(Pressing(GameButton.Confirm));
        page.Update(Pressing(GameButton.Left));

        var request = page.Update(Pressing(GameButton.Confirm));

        Assert.IsType<QuitRequested>(request);
    }

    [Fact]
    public void Update_YesToTitleScreen_RequestsTitle()
    {
        var page = NewPage();
        page.Update(Pressing(GameButton.Confirm));
        page.Update(Pressing(GameButton.Left));

        var request = page.Update(Pressing(GameButton.Confirm));

        Assert.IsType<TitleRequested>(request);
    }

    [Fact]
    public void Update_AnswerNo_StaysInPage()
    {
        var page = NewPage();
        page.Update(Pressing(GameButton.Confirm));

        var request = page.Update(Pressing(GameButton.Confirm));

        Assert.Null(request);
        Assert.True(page.LeavesOnCancel);
    }

    private static SystemPage NewPage() => new(RepositoryScreenText.Text, new UiTextures());
}
