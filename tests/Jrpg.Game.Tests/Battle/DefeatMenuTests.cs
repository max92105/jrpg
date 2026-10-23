using Jrpg.Game.Battle;
using Jrpg.Game.Input;
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
}
