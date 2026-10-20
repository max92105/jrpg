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
public sealed class LoadPageTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), $"jrpg-load-page-{Guid.NewGuid():N}");
    private readonly SaveSlots _slots;

    public LoadPageTests()
    {
        var catalog = StartMapScenarios.BelowClosedDoor;
        _slots = new SaveSlots(new FileSaveStore(_directory, catalog), catalog, () => DateTimeOffset.UnixEpoch);
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }

    [Fact]
    public void Update_ConfirmEmptySlot_RequestsNothing()
    {
        var page = NewPage();
        page.Open();

        var request = page.Update(Pressing(GameButton.Confirm));

        Assert.Null(request);
    }

    [Fact]
    public void Update_ConfirmSavedSlot_RequestsLoadingIt()
    {
        var session = GameSession.StartNew(StartMapScenarios.BelowClosedDoor, new SeededRandom(1));
        session.AddPlayTime(TimeSpan.FromMinutes(12));
        _slots.Write(2, session);
        var page = NewPage();
        page.Open();
        page.Update(Pressing(GameButton.Down));

        var request = page.Update(Pressing(GameButton.Confirm));

        var load = Assert.IsType<LoadRequested>(request);
        Assert.Equal(TimeSpan.FromMinutes(12), load.Save.PlayTime);
    }

    [Fact]
    public void Update_FileDamagedSinceTheListOpened_RequestsNothingAndShowsItDamaged()
    {
        _slots.Write(1, GameSession.StartNew(StartMapScenarios.BelowClosedDoor, new SeededRandom(1)));
        var page = NewPage();
        page.Open();
        File.WriteAllText(Path.Combine(_directory, "slot_01.json"), "{ not json");

        var request = page.Update(Pressing(GameButton.Confirm));

        Assert.Null(request);
        Assert.True(page.Entries[0].IsDamaged);
    }

    private LoadPage NewPage() => new(_slots, RepositoryScreenText.Text, new UiTextures());
}
