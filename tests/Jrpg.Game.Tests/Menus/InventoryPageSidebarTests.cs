using Jrpg.Core.Session;
using Jrpg.Game.Input;
using Jrpg.Game.Menus;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Menus;

// The party sidebar belongs to the Inventory alone (decision 266), so the page keeps it current itself.
[Collection(RepositoryScreenText.CollectionName)]
public sealed class InventoryPageSidebarTests
{
    // The list starts on the potion; Confirm opens the target choice on the leader, and Confirm again uses it there.
    [Fact]
    public void Update_AfterUsingAPotionOnTheLeader_SidebarCardShowsTheNewHp()
    {
        var session = StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowClosedDoor);
        var leader = session.Party.Leader;
        leader.Vitals.TakeDamage(50);
        var page = NewOpenPage(session);
        var hurtHp = page.Sidebar.Cards.Single(card => card.Member == leader).Hp;
        page.Update(Pressing(GameButton.Confirm));

        page.Update(Pressing(GameButton.Confirm));

        var healedHp = page.Sidebar.Cards.Single(card => card.Member == leader).Hp;
        Assert.True(healedHp > hurtHp, $"The card still shows {healedHp} HP, as before the potion.");
        Assert.Equal(leader.Vitals.Hp, healedHp);
    }

    // Reordering happens on the Party page, while the Inventory is closed and gets no update. The recruits start in the
    // reserve, so two join the leader first.
    [Fact]
    public void Open_AfterPartyReordered_SidebarListsTheNewOrder()
    {
        var session = StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowClosedDoor);
        Assert.True(session.Party.TryAddToActive(session.Party.ReserveMembers[0]));
        Assert.True(session.Party.TryAddToActive(session.Party.ReserveMembers[0]));
        var active = session.Party.ActiveMembers.ToList();
        var page = NewOpenPage(session);
        session.Party.Reorder([active[0], active[2], active[1], .. active.Skip(3)]);
        Assert.Same(active[1], page.Sidebar.Cards[1].Member);

        page.Open();

        Assert.Same(active[2], page.Sidebar.Cards[1].Member);
        Assert.Same(active[1], page.Sidebar.Cards[2].Member);
    }

    private static InventoryPage NewOpenPage(GameSession session)
    {
        var page = new InventoryPage(RepositoryScreenText.Text, new UiTextures(), session, StartMapScenarios.BelowClosedDoor);
        page.Open();
        return page;
    }
}
