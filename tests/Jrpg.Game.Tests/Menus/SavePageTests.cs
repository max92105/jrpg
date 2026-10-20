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

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
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

    private static GameSession NewSession() => GameSession.StartNew(StartMapScenarios.BelowClosedDoor, new SeededRandom(1));

    private SaveSlots NewSlots()
    {
        var catalog = StartMapScenarios.BelowClosedDoor;
        return new SaveSlots(new FileSaveStore(_directory, catalog), catalog, () => DateTimeOffset.UnixEpoch);
    }

    private SavePage NewPage(SaveSlots? slots = null) =>
        new(slots ?? NewSlots(), NewSession(), RepositoryScreenText.Text, new UiTextures());
}
