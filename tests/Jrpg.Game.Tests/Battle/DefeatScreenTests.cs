using Jrpg.Core.Randomness;
using Jrpg.Core.Session;
using Jrpg.Data.Saving;
using Jrpg.Game.Battle;
using Jrpg.Game.Input;
using Jrpg.Game.Saving;
using Jrpg.Game.Scenes;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Battle;

[Collection(RepositoryScreenText.CollectionName)]
public sealed class DefeatScreenTests : IDisposable
{
    private static readonly TimeSpan _oneUpdate = TimeSpan.FromSeconds(1.0 / 60);

    private readonly string _directory = Path.Combine(Path.GetTempPath(), $"jrpg-defeat-{Guid.NewGuid():N}");
    private readonly SaveSlots _slots;
    private DateTimeOffset _now = new(2026, 10, 21, 20, 0, 0, TimeSpan.Zero);

    public DefeatScreenTests()
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
    public void Update_ConfirmLoadLastSave_RequestsMostRecent()
    {
        SaveWithPlayTime(3, TimeSpan.FromMinutes(30));
        _now = _now.AddHours(1);
        SaveWithPlayTime(1, TimeSpan.FromMinutes(10));
        var screen = NewScreen();

        var request = screen.Update(_oneUpdate, Pressing(GameButton.Confirm));

        Assert.Equal(TimeSpan.FromMinutes(10), Assert.IsType<LoadRequested>(request).Save.PlayTime);
    }

    [Fact]
    public void Update_ChooseFileFromLoad_RequestsLoadingIt()
    {
        SaveWithPlayTime(1, TimeSpan.FromMinutes(5));
        var screen = NewScreen();
        screen.Update(_oneUpdate, Pressing(GameButton.Down));
        screen.Update(_oneUpdate, Pressing(GameButton.Confirm));

        var request = screen.Update(_oneUpdate, Pressing(GameButton.Confirm));

        Assert.Equal(TimeSpan.FromMinutes(5), Assert.IsType<LoadRequested>(request).Save.PlayTime);
    }

    [Fact]
    public void Update_CancelOnSaveList_ReturnsToChoices()
    {
        SaveWithPlayTime(1, TimeSpan.Zero);
        var screen = NewScreen();
        screen.Update(_oneUpdate, Pressing(GameButton.Down));
        screen.Update(_oneUpdate, Pressing(GameButton.Confirm));

        screen.Update(_oneUpdate, Pressing(GameButton.Cancel));

        Assert.False(screen.IsChoosingFile);
    }

    [Fact]
    public void Update_ConfirmBeforeChoicesShow_DoesNothing()
    {
        SaveWithPlayTime(1, TimeSpan.Zero);
        var screen = new DefeatScreen(_slots, RepositoryScreenText.Text, new UiTextures());

        var request = screen.Update(TimeSpan.FromSeconds(0.5), Pressing(GameButton.Confirm));

        Assert.Null(request);
    }

    [Fact]
    public void Update_ConfirmMainMenu_RequestsTitleOnce()
    {
        SaveWithPlayTime(1, TimeSpan.Zero);
        var screen = NewScreen();
        screen.Update(_oneUpdate, Pressing(GameButton.Up));

        var first = screen.Update(_oneUpdate, Pressing(GameButton.Confirm));
        var second = screen.Update(_oneUpdate, Pressing(GameButton.Confirm));

        Assert.IsType<TitleRequested>(first);
        Assert.Null(second);
    }

    private void SaveWithPlayTime(int slot, TimeSpan playTime)
    {
        var session = GameSession.StartNew(StartMapScenarios.BelowClosedDoor, new SeededRandom(1));
        session.AddPlayTime(playTime);
        _slots.Write(slot, session);
    }

    // Past the entrance, so the choices take input.
    private DefeatScreen NewScreen()
    {
        var screen = new DefeatScreen(_slots, RepositoryScreenText.Text, new UiTextures());
        screen.Update(TimeSpan.FromSeconds(1), default);
        return screen;
    }
}
