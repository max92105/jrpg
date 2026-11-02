using Jrpg.Core.Randomness;
using Jrpg.Core.Session;
using Jrpg.Data.Saving;
using Jrpg.Game.Battle;
using Jrpg.Game.Input;
using Jrpg.Game.Saving;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Battle;

// The defeat choices' sounds (decision 421); a load whose file broke is silent, with no buzzer.
[Collection(RepositoryScreenText.CollectionName)]
public sealed class DefeatScreenSoundTests : IDisposable
{
    private static readonly TimeSpan _oneUpdate = TimeSpan.FromSeconds(1.0 / 60);

    private readonly string _directory = Path.Combine(Path.GetTempPath(), $"jrpg-defeat-sound-{Guid.NewGuid():N}");
    private readonly SaveSlots _slots;

    public DefeatScreenSoundTests()
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
    public void Update_ConfirmLoadLastSave_AsksTheConfirmSound()
    {
        Save();
        var screen = NewScreen();

        screen.Update(_oneUpdate, Pressing(GameButton.Confirm));

        Assert.Equal([UiSoundFiles.Effects.MenuConfirm], screen.Sounds);
    }

    [Fact]
    public void Update_ConfirmLoadLastSaveAfterTheFileBroke_AsksNothing()
    {
        Save();
        var screen = NewScreen();
        File.WriteAllText(Path.Combine(_directory, "slot_01.json"), "{ not json");

        screen.Update(_oneUpdate, Pressing(GameButton.Confirm));

        Assert.Empty(screen.Sounds);
    }

    [Fact]
    public void Update_Down_AsksTheMoveSound()
    {
        Save();
        var screen = NewScreen();

        screen.Update(_oneUpdate, Pressing(GameButton.Down));

        Assert.Equal([UiSoundFiles.Effects.MenuMove], screen.Sounds);
    }

    [Fact]
    public void Update_CancelInTheFileList_AsksTheCancelSound()
    {
        Save();
        var screen = NewScreen();
        for (var presses = 0; presses < screen.Menu.Entries.Count && screen.Menu.Selected != DefeatEntry.Load; presses++)
        {
            screen.Update(_oneUpdate, Pressing(GameButton.Down));
        }

        screen.Update(_oneUpdate, Pressing(GameButton.Confirm));
        Assert.True(screen.IsChoosingFile);

        screen.Update(_oneUpdate, Pressing(GameButton.Cancel));

        Assert.Equal([UiSoundFiles.Effects.MenuCancel], screen.Sounds);
    }

    private void Save() => _slots.Write(1, GameSession.StartNew(StartMapScenarios.BelowClosedDoor, "sarxian", new SeededRandom(1)));

    // Past the entrance, so the choices take input.
    private DefeatScreen NewScreen()
    {
        var screen = new DefeatScreen(_slots, RepositoryScreenText.Text, new UiTextures());
        screen.Update(TimeSpan.FromSeconds(1), default);
        return screen;
    }
}
