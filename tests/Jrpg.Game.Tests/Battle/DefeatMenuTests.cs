using Jrpg.Game.Battle;

namespace Jrpg.Game.Tests.Battle;

public sealed class DefeatMenuTests
{
    [Fact]
    public void Start_ReadableSave_SelectsLoadLastSave()
    {
        var menu = new DefeatMenu(hasReadableSave: true);

        Assert.Equal(DefeatEntry.LoadLastSave, menu.Selected);
    }

    [Fact]
    public void Start_NoReadableSave_BothLoadsGreyedAndMainMenuSelected()
    {
        var menu = new DefeatMenu(hasReadableSave: false);

        Assert.False(menu.IsEnabled(DefeatEntry.LoadLastSave));
        Assert.False(menu.IsEnabled(DefeatEntry.Load));
        Assert.Equal(DefeatEntry.MainMenu, menu.Selected);
    }
}
