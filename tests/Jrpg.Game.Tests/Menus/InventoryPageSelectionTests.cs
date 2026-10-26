using Jrpg.Game.Input;
using Jrpg.Game.Menus;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Menus;

// InventoryPageTests is over 300 lines, so the list's motion is tested here.
[Collection(RepositoryScreenText.CollectionName)]
public sealed class InventoryPageSelectionTests
{
    [Fact]
    public void Advance_AfterMovingDown_SelectionReachesTheNextRow()
    {
        var session = StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowClosedDoor);
        var page = new InventoryPage(RepositoryScreenText.Text, new UiTextures(), session, StartMapScenarios.BelowClosedDoor);
        page.Update(Pressing(GameButton.Down));

        page.Advance(TimeSpan.FromSeconds(1));

        Assert.Equal(1, page.SelectionPosition);
    }
}
