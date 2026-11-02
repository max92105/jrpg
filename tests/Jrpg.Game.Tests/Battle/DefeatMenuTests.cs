using Jrpg.Game.Battle;
using Jrpg.Game.Input;
using Jrpg.Game.Ui;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Battle;

public sealed class DefeatMenuTests
{
    [Fact]
    public void Entries_ReadableSave_ListsBothLoadsAndMainMenu()
    {
        var menu = new DefeatMenu(hasReadableSave: true);

        Assert.Equal([DefeatEntry.LoadLastSave, DefeatEntry.Load, DefeatEntry.MainMenu], menu.Entries);
    }

    [Fact]
    public void Entries_NoReadableSave_ListsMainMenuOnly()
    {
        var menu = new DefeatMenu(hasReadableSave: false);

        Assert.Equal([DefeatEntry.MainMenu], menu.Entries);
    }

    [Fact]
    public void Start_ReadableSave_SelectsLoadLastSave()
    {
        var menu = new DefeatMenu(hasReadableSave: true);

        Assert.Equal(DefeatEntry.LoadLastSave, menu.Selected);
    }

    [Fact]
    public void Update_DownWithoutReadableSave_StaysOnMainMenu()
    {
        var menu = new DefeatMenu(hasReadableSave: false);

        menu.Update(Pressing(GameButton.Down));

        Assert.Equal(DefeatEntry.MainMenu, menu.Selected);
    }

    [Fact]
    public void Update_Down_AsksTheMoveSound()
    {
        var menu = new DefeatMenu(hasReadableSave: true);

        menu.Update(Pressing(GameButton.Down));

        Assert.Equal([UiSoundFiles.Effects.MenuMove], menu.Sounds);
    }

    [Fact]
    public void Update_NothingPressedAfterASound_AsksNothing()
    {
        var menu = new DefeatMenu(hasReadableSave: true);
        menu.Update(Pressing(GameButton.Down));

        menu.Update(default);

        Assert.Empty(menu.Sounds);
    }
}
