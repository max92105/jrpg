using Jrpg.Core.Randomness;
using Jrpg.Core.Session;
using Jrpg.Game.Input;
using Jrpg.Game.Menus;
using Jrpg.Game.Scenes;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Saving;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Menus;

[Collection(RepositoryScreenText.CollectionName)]
public sealed class SystemPageTests
{
    [Fact]
    public void Update_ConfirmSave_OpensSaveFilesUntilCancel()
    {
        var page = NewPage();

        page.Update(Pressing(GameButton.Confirm));
        var whileSaving = page.LeavesOnCancel;
        page.Update(Pressing(GameButton.Cancel));

        Assert.False(whileSaving);
        Assert.True(page.LeavesOnCancel);
    }

    [Fact]
    public void Update_ConfirmQuit_AsksBeforeRequesting()
    {
        var page = NewPage();
        page.Update(Pressing(GameButton.Up));

        var request = page.Update(Pressing(GameButton.Confirm));

        Assert.Null(request);
        Assert.False(page.LeavesOnCancel);
    }

    [Fact]
    public void Update_YesToQuit_RequestsQuit()
    {
        var page = NewPage();
        page.Update(Pressing(GameButton.Up));
        page.Update(Pressing(GameButton.Confirm));
        page.Update(Pressing(GameButton.Left));

        var request = page.Update(Pressing(GameButton.Confirm));

        Assert.IsType<QuitRequested>(request);
    }

    [Fact]
    public void Update_YesToTitleScreen_RequestsTitle()
    {
        var page = NewPage();
        page.Update(Pressing(GameButton.Down));
        page.Update(Pressing(GameButton.Confirm));
        page.Update(Pressing(GameButton.Left));

        var request = page.Update(Pressing(GameButton.Confirm));

        Assert.IsType<TitleRequested>(request);
    }

    [Fact]
    public void Update_AnswerNo_StaysInPage()
    {
        var page = NewPage();
        page.Update(Pressing(GameButton.Down));
        page.Update(Pressing(GameButton.Confirm));

        var request = page.Update(Pressing(GameButton.Confirm));

        Assert.Null(request);
        Assert.True(page.LeavesOnCancel);
    }

    private static SystemPage NewPage()
    {
        var ui = new UiTextures();
        var session = GameSession.StartNew(StartMapScenarios.BelowClosedDoor, new SeededRandom(1));
        return new(RepositoryScreenText.Text, ui, new SavePage(UnwrittenSaveSlots.Create(), session, RepositoryScreenText.Text, ui));
    }
}
