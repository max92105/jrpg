using Jrpg.Core.Session;
using Jrpg.Game.Input;
using Jrpg.Game.Menus;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Menus;

// The new game's one Camp Kit rests the whole party from the item list, with no member to pick.
[Collection(RepositoryScreenText.CollectionName)]
public sealed class InventoryPageRestItemTests
{
    [Fact]
    public void Confirm_CampKit_UsesWithoutPickingMember()
    {
        var session = StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowClosedDoor);
        var leader = session.Party.Leader.Vitals;
        leader.TakeDamage(50);
        var page = PageOnCampKit(session);

        page.Update(Pressing(GameButton.Confirm));

        Assert.False(page.IsChoosingTarget);
        Assert.Equal(RepositoryScreenText.Text.Messages.Text(MessageIds.PartyRests), page.Notice);
        Assert.Equal(leader.MaxHp, leader.Hp);
        Assert.DoesNotContain("camp_kit", page.ShownItemIds);
    }

    // Nobody needs a rest in a new game, so the kit is greyed and Confirm uses nothing.
    [Fact]
    public void Confirm_CampKitNobodyToRest_UsesNothing()
    {
        var session = StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowClosedDoor);
        var page = PageOnCampKit(session);

        page.Update(Pressing(GameButton.Confirm));

        Assert.Null(page.Notice);
        Assert.Contains("camp_kit", page.ShownItemIds);
    }

    private static InventoryPage PageOnCampKit(GameSession session)
    {
        var page = new InventoryPage(RepositoryScreenText.Text, new UiTextures(), session, StartMapScenarios.BelowClosedDoor);
        while (page.ShownItemIds[page.SelectedRow!.Value] != "camp_kit")
        {
            page.Update(Pressing(GameButton.Down));
        }

        return page;
    }
}
