using Jrpg.Core.Randomness;
using Jrpg.Core.Session;
using Jrpg.Data.Saving;
using Jrpg.Game.Input;
using Jrpg.Game.Saving;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Title;
using Jrpg.Game.Ui;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Title;

// Move, confirm and cancel on the title and its file list (decision 421); a refused file is silent, with no buzzer.
[Collection(RepositoryScreenText.CollectionName)]
public sealed class TitleSceneSoundTests : IDisposable
{
    private static readonly TimeSpan _oneUpdate = TimeSpan.FromSeconds(1.0 / 60);

    private readonly string _directory = Path.Combine(Path.GetTempPath(), $"jrpg-title-sound-{Guid.NewGuid():N}");
    private readonly SaveSlots _slots;

    public TitleSceneSoundTests()
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
    public void Update_Down_AsksTheMoveSound()
    {
        var scene = NewScene();

        scene.Update(_oneUpdate, Pressing(GameButton.Down));

        Assert.Equal([UiSoundFiles.Effects.MenuMove], scene.Sounds);
    }

    [Fact]
    public void Update_NothingPressed_AsksNothing()
    {
        var scene = NewScene();
        scene.Update(_oneUpdate, Pressing(GameButton.Down));

        scene.Update(_oneUpdate, default);

        Assert.Empty(scene.Sounds);
    }

    [Fact]
    public void Update_Confirm_AsksTheConfirmSound()
    {
        var scene = NewScene();

        scene.Update(_oneUpdate, Pressing(GameButton.Confirm));

        Assert.Equal([UiSoundFiles.Effects.MenuConfirm], scene.Sounds);
    }

    [Fact]
    public void Update_DownInTheFileList_AsksTheMoveSound()
    {
        var scene = SceneOnTheFileList();

        scene.Update(_oneUpdate, Pressing(GameButton.Down));

        Assert.Equal([UiSoundFiles.Effects.MenuMove], scene.Sounds);
    }

    [Fact]
    public void Update_ConfirmOnALoadableFile_AsksTheConfirmSound()
    {
        var scene = SceneOnTheFileList();

        scene.Update(_oneUpdate, Pressing(GameButton.Confirm));

        Assert.Equal([UiSoundFiles.Effects.MenuConfirm], scene.Sounds);
    }

    [Fact]
    public void Update_CancelInTheFileList_AsksTheCancelSound()
    {
        var scene = SceneOnTheFileList();

        scene.Update(_oneUpdate, Pressing(GameButton.Cancel));

        Assert.Equal([UiSoundFiles.Effects.MenuCancel], scene.Sounds);
    }

    // The save is in slot 2, so the list opens on the empty slot 1.
    [Fact]
    public void Update_ConfirmOnAnEmptySlot_AsksNothing()
    {
        var scene = SceneOnTheFileList(saveSlot: 2);

        scene.Update(_oneUpdate, Pressing(GameButton.Confirm));

        Assert.Empty(scene.Sounds);
        Assert.True(scene.IsChoosingFile);
    }

    [Fact]
    public void Update_ConfirmOnADamagedSlot_AsksNothing()
    {
        var scene = SceneOnTheFileList();
        File.WriteAllText(Path.Combine(_directory, "slot_01.json"), "{ not json");

        scene.Update(_oneUpdate, Pressing(GameButton.Confirm));

        Assert.Empty(scene.Sounds);
        Assert.True(scene.IsChoosingFile);
    }

    [Fact]
    public void Update_ConfirmContinueAfterTheFileBroke_AsksNothing()
    {
        _slots.Write(1, GameSession.StartNew(StartMapScenarios.BelowClosedDoor, "sarxian", new SeededRandom(1)));
        var scene = NewScene();
        File.WriteAllText(Path.Combine(_directory, "slot_01.json"), "{ not json");

        scene.Update(_oneUpdate, Pressing(GameButton.Confirm));

        Assert.Equal(TitleEntry.Continue, scene.SelectedEntry);
        Assert.Empty(scene.Sounds);
    }

    private TitleScene NewScene() => new(RepositoryScreenText.Text, _slots, new UiTextures());

    // A save makes Load an entry; the entries are picked by what they are.
    private TitleScene SceneOnTheFileList(int saveSlot = 1)
    {
        _slots.Write(saveSlot, GameSession.StartNew(StartMapScenarios.BelowClosedDoor, "sarxian", new SeededRandom(1)));
        var scene = NewScene();
        for (var presses = 0; presses < Enum.GetValues<TitleEntry>().Length && scene.SelectedEntry != TitleEntry.Load; presses++)
        {
            scene.Update(_oneUpdate, Pressing(GameButton.Down));
        }

        scene.Update(_oneUpdate, Pressing(GameButton.Confirm));
        Assert.True(scene.IsChoosingFile);
        return scene;
    }
}
