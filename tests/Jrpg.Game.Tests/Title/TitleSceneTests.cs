using Jrpg.Core.Randomness;
using Jrpg.Core.Session;
using Jrpg.Data.Saving;
using Jrpg.Game.Input;
using Jrpg.Game.Saving;
using Jrpg.Game.Scenes;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Title;
using Jrpg.Game.Ui;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Title;

[Collection(RepositoryScreenText.CollectionName)]
public sealed class TitleSceneTests : IDisposable
{
    private static readonly TimeSpan _oneUpdate = TimeSpan.FromSeconds(1.0 / 60);

    private readonly string _directory = Path.Combine(Path.GetTempPath(), $"jrpg-title-{Guid.NewGuid():N}");
    private readonly SaveSlots _slots;
    private DateTimeOffset _now = new(2026, 10, 19, 20, 0, 0, TimeSpan.Zero);

    public TitleSceneTests()
    {
        var catalog = StartMapScenarios.BelowClosedDoor;
        _slots = new SaveSlots(new FileSaveStore(_directory, catalog), catalog, () => _now);
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }

    [Fact]
    public void Update_ConfirmWithReadableSave_RequestsMostRecentSlot()
    {
        SaveWithPlayTime(3, TimeSpan.FromMinutes(30));
        _now = _now.AddHours(1);
        SaveWithPlayTime(1, TimeSpan.FromMinutes(10));
        var scene = NewScene();

        var request = scene.Update(_oneUpdate, Pressing(GameButton.Confirm));

        var load = Assert.IsType<LoadRequested>(request);
        Assert.Equal(TimeSpan.FromMinutes(10), load.Save.PlayTime);
    }

    [Fact]
    public void Update_ConfirmLoad_OpensSlotList()
    {
        SaveWithPlayTime(1, TimeSpan.Zero);
        var scene = NewScene();
        Select(scene, TitleEntry.Load);

        scene.Update(_oneUpdate, Pressing(GameButton.Confirm));

        Assert.True(scene.IsChoosingFile);
    }

    [Fact]
    public void Update_CancelOnSlotList_ReturnsToEntries()
    {
        SaveWithPlayTime(1, TimeSpan.Zero);
        var scene = NewScene();
        Select(scene, TitleEntry.Load);
        scene.Update(_oneUpdate, Pressing(GameButton.Confirm));

        scene.Update(_oneUpdate, Pressing(GameButton.Cancel));

        Assert.False(scene.IsChoosingFile);
    }

    [Fact]
    public void Update_ConfirmFileOnSlotList_RequestsLoadingIt()
    {
        SaveWithPlayTime(1, TimeSpan.FromMinutes(5));
        var scene = NewScene();
        Select(scene, TitleEntry.Load);
        scene.Update(_oneUpdate, Pressing(GameButton.Confirm));

        var request = scene.Update(_oneUpdate, Pressing(GameButton.Confirm));

        Assert.Equal(TimeSpan.FromMinutes(5), Assert.IsType<LoadRequested>(request).Save.PlayTime);
    }

    [Fact]
    public void Update_ConfirmContinueAfterTheFileBroke_RequestsNothing()
    {
        SaveWithPlayTime(1, TimeSpan.Zero);
        var scene = NewScene();
        File.WriteAllText(Path.Combine(_directory, "slot_01.json"), "{ not json");

        var request = scene.Update(_oneUpdate, Pressing(GameButton.Confirm));

        Assert.Null(request);
    }

    [Fact]
    public void Update_ConfirmDuringEntrance_ChoosesTheEntry()
    {
        var scene = NewScene();
        Select(scene, TitleEntry.NewGame);

        var request = scene.Update(_oneUpdate, Pressing(GameButton.Confirm));

        Assert.IsType<NewGameRequested>(request);
    }

    private void SaveWithPlayTime(int slot, TimeSpan playTime)
    {
        var session = GameSession.StartNew(StartMapScenarios.BelowClosedDoor, "sarxian", new SeededRandom(1));
        session.AddPlayTime(playTime);
        _slots.Write(slot, session);
    }

    private TitleScene NewScene() => new(RepositoryScreenText.Text, _slots, new UiTextures());

    // Bounded by the number of entries, so an entry that isn't listed fails the test instead of looping forever.
    private static void Select(TitleScene scene, TitleEntry entry)
    {
        for (var presses = 0; presses < Enum.GetValues<TitleEntry>().Length && scene.SelectedEntry != entry; presses++)
        {
            scene.Update(_oneUpdate, Pressing(GameButton.Down));
        }

        Assert.Equal(entry, scene.SelectedEntry);
    }
}
