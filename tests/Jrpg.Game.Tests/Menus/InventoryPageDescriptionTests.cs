using Jrpg.Game.Input;
using Jrpg.Game.Menus;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Menus;

// Info shows or hides the selected item's description (decision 291). The list starts on the potion.
[Collection(RepositoryScreenText.CollectionName)]
public sealed class InventoryPageDescriptionTests
{
    [Fact]
    public void Update_Info_HidesTheDescription()
    {
        var page = NewOpenPage();

        page.Update(Pressing(GameButton.Info));

        Assert.False(page.IsDescriptionShown);
    }

    [Fact]
    public void Update_InfoTwice_ShowsTheDescriptionAgain()
    {
        var page = NewOpenPage();
        page.Update(Pressing(GameButton.Info));

        page.Update(Pressing(GameButton.Info));

        Assert.True(page.IsDescriptionShown);
    }

    [Fact]
    public void Update_InfoWhileAiming_KeepsTheDescription()
    {
        var page = NewOpenPage();
        page.Update(Pressing(GameButton.Confirm));
        Assert.True(page.IsChoosingTarget);

        page.Update(Pressing(GameButton.Info));

        Assert.True(page.IsDescriptionShown);
    }

    private static InventoryPage NewOpenPage()
    {
        var session = StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowClosedDoor);
        var page = new InventoryPage(RepositoryScreenText.Text, new UiTextures(), session, StartMapScenarios.BelowClosedDoor);
        page.Open();
        return page;
    }
}
