using Jrpg.Core.Randomness;
using Jrpg.Core.Session;
using Jrpg.Data.Saving;
using Jrpg.Game.Input;
using Jrpg.Game.Menus;
using Jrpg.Game.Saving;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Menus;

[Collection(RepositoryScreenText.CollectionName)]
public sealed class SavePageTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), $"jrpg-save-page-{Guid.NewGuid():N}");
    private DateTimeOffset _now = DateTimeOffset.UnixEpoch;

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
        else if (File.Exists(_directory))
        {
            File.Delete(_directory);
        }
    }

    [Fact]
    public void Update_ConfirmSlot_WritesSave()
    {
        var page = NewPage();
        page.Open();
        page.Update(Pressing(GameButton.Down));

        page.Update(Pressing(GameButton.Confirm));

        Assert.True(File.Exists(Path.Combine(_directory, "slot_02.json")));
        Assert.NotNull(page.Entries[1].Summary);
        Assert.True(page.Entries[0].IsEmpty);
    }

    [Fact]
    public void Open_AfterSavingElsewhere_ShowsTheNewSave()
    {
        var slots = NewSlots();
        var page = NewPage(slots);
        slots.Write(5, NewSession());

        page.Open();

        Assert.NotNull(page.Entries[4].Summary);
    }

    [Fact]
    public void Update_ConfirmFilledSlot_AsksBeforeWriting()
    {
        var slots = NewSlots();
        slots.Write(1, NewSession());
        var page = NewPage(slots);
        page.Open();
        _now = _now.AddDays(1);

        page.Update(Pressing(GameButton.Confirm));

        Assert.True(page.IsAsking);
        Assert.False(page.LeavesOnCancel);
        Assert.Equal(DateTimeOffset.UnixEpoch, page.Entries[0].Summary?.SavedAt);
    }

    [Fact]
    public void Update_AnswerYes_Overwrites()
    {
        var slots = NewSlots();
        slots.Write(1, NewSession());
        var page = NewPage(slots);
        page.Open();
        _now = _now.AddDays(1);
        page.Update(Pressing(GameButton.Confirm));
        page.Update(Pressing(GameButton.Left));

        page.Update(Pressing(GameButton.Confirm));

        Assert.False(page.IsAsking);
        Assert.Equal(_now, page.Entries[0].Summary?.SavedAt);
    }

    [Fact]
    public void Update_AnswerNo_KeepsTheOldSave()
    {
        var slots = NewSlots();
        slots.Write(1, NewSession());
        var page = NewPage(slots);
        page.Open();
        _now = _now.AddDays(1);
        page.Update(Pressing(GameButton.Confirm));

        page.Update(Pressing(GameButton.Cancel));

        Assert.False(page.IsAsking);
        Assert.Equal(DateTimeOffset.UnixEpoch, page.Entries[0].Summary?.SavedAt);
    }

    [Fact]
    public void Update_WriteFails_KeepsListOpenAndSaysSo()
    {
        // A file where the save folder should be: the folder can't be created, so writing throws.
        File.WriteAllText(_directory, "not a folder");
        var page = NewPage();
        page.Open();

        page.Update(Pressing(GameButton.Confirm));

        Assert.Equal("Couldn't save to File 1.", page.Notice);
        Assert.True(page.LeavesOnCancel);
        Assert.True(page.Entries[0].IsEmpty);
    }

    [Fact]
    public void Update_SaveAfterAFailedOne_ClearsTheNotice()
    {
        File.WriteAllText(_directory, "not a folder");
        var page = NewPage();
        page.Open();
        page.Update(Pressing(GameButton.Confirm));
        File.Delete(_directory);

        page.Update(Pressing(GameButton.Confirm));

        Assert.Null(page.Notice);
        Assert.NotNull(page.Entries[0].Summary);
    }

    private static GameSession NewSession() => GameSession.StartNew(StartMapScenarios.BelowClosedDoor, "sarxian", new SeededRandom(1));

    private SaveSlots NewSlots()
    {
        var catalog = StartMapScenarios.BelowClosedDoor;
        return new SaveSlots(new FileSaveStore(_directory, catalog), catalog, () => _now);
    }

    private SavePage NewPage(SaveSlots? slots = null) =>
        new(slots ?? NewSlots(), NewSession(), RepositoryScreenText.Text, new UiTextures());
}
