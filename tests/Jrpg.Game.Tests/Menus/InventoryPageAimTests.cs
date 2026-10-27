using Jrpg.Core.Party;
using Jrpg.Core.Session;
using Jrpg.Game.Input;
using Jrpg.Game.Menus;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Menus;

// Using an item aims at the party sidebar's cards (decision 267). The list starts on the potion, which anyone standing
// can take, and the whole-party start has more recruits than cards in view.
[Collection(RepositoryScreenText.CollectionName)]
public sealed class InventoryPageAimTests
{
    [Fact]
    public void Update_ConfirmPotion_PointsTheSidebarAtTheFirstCandidate()
    {
        var session = NewSession();
        var page = NewOpenPage(session);
        Assert.Null(page.Sidebar.PointedIndex);

        page.Update(Pressing(GameButton.Confirm));

        Assert.True(page.IsChoosingTarget);
        Assert.Same(MemberOrder.ActiveThenReserve(session.Party)[0], PointedMember(page));
    }

    [Fact]
    public void Update_DownWhileAiming_PointsTheSidebarAtTheNextCandidate()
    {
        var session = NewSession();
        var page = NewOpenPage(session);
        page.Update(Pressing(GameButton.Confirm));

        page.Update(Pressing(GameButton.Down));

        Assert.Same(MemberOrder.ActiveThenReserve(session.Party)[1], PointedMember(page));
    }

    [Fact]
    public void Update_DownToTheLastReserveMember_ScrollsTheSidebarToIt()
    {
        var page = NewOpenPage(NewSession());
        var cardCount = page.Sidebar.Cards.Count;
        Assert.True(cardCount > PartySidebar.VisibleCards, "The test needs more recruits than cards in view.");
        page.Update(Pressing(GameButton.Confirm));
        Assert.Equal(0, page.Sidebar.FirstVisible);

        for (var press = 1; press < cardCount; press++)
        {
            page.Update(Pressing(GameButton.Down));
        }

        Assert.Equal(cardCount - 1, page.Sidebar.PointedIndex);
        Assert.Equal(cardCount - PartySidebar.VisibleCards, page.Sidebar.FirstVisible);
    }

    [Fact]
    public void Hint_WhileAiming_NamesTheItem()
    {
        var page = NewOpenPage(NewSession());

        page.Update(Pressing(GameButton.Confirm));

        var potion = StartMapScenarios.BelowClosedDoor.Items["potion"].Name;
        Assert.Equal(RepositoryScreenText.Text.Messages.Format(MessageIds.MenuHintInventoryTarget, new MessageArgument("item", potion)), page.Hint);
    }

    [Fact]
    public void Hint_AfterCancel_IsTheListHint()
    {
        var page = NewOpenPage(NewSession());
        page.Update(Pressing(GameButton.Confirm));
        Assert.True(page.IsChoosingTarget);

        page.Update(Pressing(GameButton.Cancel));

        Assert.Equal(RepositoryScreenText.Text.Messages.Text(MessageIds.MenuHintInventory), page.Hint);
        Assert.Null(page.Sidebar.PointedIndex);
    }

    // Decision 288: copies are left and the healed member can still take a potion, so the pointer stays on them.
    [Fact]
    public void Update_UsePotionWithCopiesLeft_KeepsAimingAtTheSameMember()
    {
        var session = NewSession();
        var page = NewOpenPage(session);
        page.Update(Pressing(GameButton.Confirm));
        page.Update(Pressing(GameButton.Down));
        var potions = session.Inventory.QuantityOf("potion");
        var target = PointedMember(page);
        Assert.Same(MemberOrder.ActiveThenReserve(session.Party)[1], target);

        page.Update(Pressing(GameButton.Confirm));

        Assert.True(page.IsChoosingTarget);
        Assert.Same(target, PointedMember(page));
        Assert.Equal(potions - 1, session.Inventory.QuantityOf("potion"));
        Assert.NotNull(page.Notice);
    }

    [Fact]
    public void Update_UseLastPotion_ReturnsToTheList()
    {
        var session = NewSession();
        Assert.True(session.Inventory.TryRemove("potion", session.Inventory.QuantityOf("potion") - 1));
        var page = NewOpenPage(session);
        page.Update(Pressing(GameButton.Confirm));

        page.Update(Pressing(GameButton.Confirm));

        Assert.False(page.IsChoosingTarget);
        Assert.Null(page.Sidebar.PointedIndex);
        Assert.DoesNotContain("potion", page.ShownItemIds);
    }

    // The feather revives every fallen member at once, so with one fallen nobody is left for it, copies or not.
    [Fact]
    public void Update_ReviveTheOnlyFallenWithCopiesLeft_ReturnsToTheList()
    {
        var session = NewSession();
        var fallen = MemberOrder.ActiveThenReserve(session.Party)[1];
        fallen.Vitals.TakeDamage(fallen.Vitals.MaxHp);
        var page = NewOpenPage(session);
        SelectRow(page, "revive_feather");
        page.Update(Pressing(GameButton.Confirm));

        page.Update(Pressing(GameButton.Confirm));

        Assert.True(fallen.Vitals.IsAlive);
        Assert.True(session.Inventory.QuantityOf("revive_feather") > 0, "The test needs feathers left after the use.");
        Assert.False(page.IsChoosingTarget);
    }

    [Fact]
    public void Update_CancelAfterAUse_ReturnsToTheListAndUsesNothingMore()
    {
        var session = NewSession();
        var page = NewOpenPage(session);
        page.Update(Pressing(GameButton.Confirm));
        page.Update(Pressing(GameButton.Confirm));
        Assert.True(page.IsChoosingTarget);
        var potionsAfterOne = session.Inventory.QuantityOf("potion");

        page.Update(Pressing(GameButton.Cancel));

        Assert.False(page.IsChoosingTarget);
        Assert.Equal(potionsAfterOne, session.Inventory.QuantityOf("potion"));
        Assert.Null(page.Sidebar.PointedIndex);
    }

    [Fact]
    public void Update_ConfirmPotionWithTheLeaderHurt_AimsWithTheLeadersHpAfter()
    {
        var session = NewSession();
        var leader = session.Party.Leader;
        leader.Vitals.TakeDamage(leader.Vitals.MaxHp / 2);
        var page = NewOpenPage(session);

        page.Update(Pressing(GameButton.Confirm));

        var hpAfter = FieldItemUse.HpAfterUse(StartMapScenarios.BelowClosedDoor.Items["potion"], leader);
        Assert.True(hpAfter > leader.Vitals.Hp, "The test needs a potion that heals the hurt leader.");
        Assert.Equal(hpAfter, page.ItemUse.HpAfter(leader));
    }

    [Fact]
    public void Update_UsePotionWithCopiesLeft_HpAfterFollowsTheNewHp()
    {
        var session = NewSession();
        var leader = session.Party.Leader;
        leader.Vitals.TakeDamage(leader.Vitals.MaxHp - 1);
        var page = NewOpenPage(session);
        page.Update(Pressing(GameButton.Confirm));
        var hpAfterFirst = page.ItemUse.HpAfter(leader);

        page.Update(Pressing(GameButton.Confirm));

        Assert.Equal(hpAfterFirst, leader.Vitals.Hp);
        Assert.True(leader.Vitals.Hp < leader.Vitals.MaxHp, "The test needs the leader still hurt after one potion.");
        Assert.Equal(FieldItemUse.HpAfterUse(StartMapScenarios.BelowClosedDoor.Items["potion"], leader), page.ItemUse.HpAfter(leader));
        Assert.True(page.ItemUse.HpAfter(leader) > hpAfterFirst);
    }

    [Fact]
    public void HpAfter_NotAiming_Throws()
    {
        var session = NewSession();
        var page = NewOpenPage(session);

        Assert.Throws<InvalidOperationException>(() => page.ItemUse.HpAfter(session.Party.Leader));
    }

    private static void SelectRow(InventoryPage page, string itemId)
    {
        while (page.ShownItemIds[page.SelectedRow!.Value] != itemId)
        {
            page.Update(Pressing(GameButton.Down));
        }
    }

    private static PartyMember? PointedMember(InventoryPage page) =>
        page.Sidebar.PointedIndex is { } index ? page.Sidebar.Cards[index].Member : null;

    private static GameSession NewSession() => StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowClosedDoor);

    private static InventoryPage NewOpenPage(GameSession session)
    {
        var page = new InventoryPage(RepositoryScreenText.Text, new UiTextures(), session, StartMapScenarios.BelowClosedDoor);
        page.Open();
        return page;
    }
}
