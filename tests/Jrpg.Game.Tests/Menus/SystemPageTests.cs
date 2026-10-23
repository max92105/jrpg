using Jrpg.Core.Randomness;
using Jrpg.Core.Session;
using Jrpg.Data.Saving;
using Jrpg.Game.Input;
using Jrpg.Game.Menus;
using Jrpg.Game.Saving;
using Jrpg.Game.Scenes;
using Jrpg.Game.Tests.Exploration;
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
    public void Hint_SaveFilesOpen_IsTheSaveHintUntilCancel()
    {
        var page = NewPage();
        var messages = RepositoryScreenText.Text.Messages;

        page.Update(Pressing(GameButton.Confirm));
        var whileSaving = page.Hint;
        page.Update(Pressing(GameButton.Cancel));

        Assert.Equal(messages.Text(MessageIds.MenuHintSave), whileSaving);
        Assert.Equal(messages.Text(MessageIds.MenuHintSystem), page.Hint);
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
        page.Update(Pressing(GameButton.Down));
        page.Update(Pressing(GameButton.Confirm));

        var request = page.Update(Pressing(GameButton.Confirm));

        Assert.Null(request);
        Assert.True(page.LeavesOnCancel);
    }

    [Fact]
    public void Update_CancelWhileAskingToOverwrite_AnswersNoAndKeepsTheFilesOpen()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"jrpg-system-{Guid.NewGuid():N}");
        try
        {
            var catalog = StartMapScenarios.BelowClosedDoor;
            var slots = new SaveSlots(new FileSaveStore(directory, catalog), catalog, () => DateTimeOffset.UnixEpoch);
            var session = GameSession.StartNew(catalog, "sarxian", new SeededRandom(1));
            slots.Write(1, session);
            var ui = new UiTextures();
            var text = RepositoryScreenText.Text;
            var page = new SystemPage(text, ui, new SavePage(slots, session, text, ui), new LoadPage(slots, text, ui));
            page.Update(Pressing(GameButton.Confirm));
            page.Update(Pressing(GameButton.Confirm));

            page.Update(Pressing(GameButton.Cancel));
            var filesStillOpen = !page.LeavesOnCancel;
            page.Update(Pressing(GameButton.Cancel));

            Assert.True(filesStillOpen);
            Assert.True(page.LeavesOnCancel);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private static SystemPage NewPage()
    {
        var ui = new UiTextures();
        var session = GameSession.StartNew(StartMapScenarios.BelowClosedDoor, "sarxian", new SeededRandom(1));
        return TestSystemPages.Create(session, ui);
    }
}
