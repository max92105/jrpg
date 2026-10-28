using Jrpg.Core.Session;
using Jrpg.Game.Input;
using Jrpg.Game.Menus;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Menus;

[Collection(RepositoryScreenText.CollectionName)]
public sealed class InventoryPageUseTests
{
    [Fact]
    public void Open_ClearsTheLastUseNotice()
    {
        var session = NewSession();
        session.Party.Leader.Vitals.TakeDamage(50);
        var page = NewPage(session);
        UseThirdItemOnLeader(page);

        page.Open();

        Assert.Null(page.Notice);
    }

    [Fact]
    public void Update_ConfirmUnusableItem_OpensNothing()
    {
        // Starting items, in pickup order: potion, hi_potion, elixir, then the bomb, which can't be used from the menu.
        var page = NewPage();
        for (var press = 0; press < 3; press++)
        {
            page.Update(Pressing(GameButton.Down));
        }

        page.Update(Pressing(GameButton.Confirm));

        Assert.Equal("bomb", page.ShownItemIds[page.SelectedRow ?? -1]);
        Assert.False(page.IsChoosingTarget);
    }

    [Fact]
    public void Update_UseLastElixir_RemovesRowAndSaysWhatHappened()
    {
        var session = NewSession();
        var leader = session.Party.Leader.Vitals;
        leader.TakeDamage(50);
        // The elixir restores 100 HP, but never past the maximum.
        var expected = Math.Min(100, leader.MaxHp - leader.Hp);
        var page = NewPage(session);
        page.Update(Pressing(GameButton.Down));
        page.Update(Pressing(GameButton.Down));
        page.Update(Pressing(GameButton.Confirm));

        page.Update(Pressing(GameButton.Confirm));

        Assert.DoesNotContain("elixir", page.ShownItemIds);
        Assert.Equal(2, page.SelectedRow);
        Assert.Equal($"Sarxian recovers {expected} HP.", page.Notice);
        Assert.False(page.IsChoosingTarget);
    }

    [Fact]
    public void Update_CancelWhileChoosingTarget_ClosesItAndUsesNothing()
    {
        var session = NewSession();
        var page = NewPage(session);
        page.Update(Pressing(GameButton.Confirm));
        var whileChoosing = page.LeavesOnCancel;

        page.Update(Pressing(GameButton.Cancel));

        Assert.False(whileChoosing);
        Assert.False(page.IsChoosingTarget);
        Assert.Equal(5, session.Inventory.QuantityOf("potion"));
    }

    [Fact]
    public void Update_UseReviveFeatherWithTwoFallen_RevivesBothAndSaysTheItemWasUsed()
    {
        // The feather revives every fallen member at once; with more than one, the result names the item, not a member.
        var session = NewSession();
        var jazog = session.Party.MemberById("jazog");
        var teelle = session.Party.MemberById("teelle");
        jazog.Vitals.TakeDamage(10_000);
        teelle.Vitals.TakeDamage(10_000);
        var page = NewPage(session);
        var featherRow = page.ShownItemIds.ToList().IndexOf("revive_feather");
        for (var press = 0; press < featherRow; press++)
        {
            page.Update(Pressing(GameButton.Down));
        }

        page.Update(Pressing(GameButton.Confirm));
        page.Update(Pressing(GameButton.Confirm));

        Assert.True(jazog.Vitals.IsAlive);
        Assert.True(teelle.Vitals.IsAlive);
        Assert.Equal($"{StartMapScenarios.BelowClosedDoor.Items["revive_feather"].Name} was used.", page.Notice);
    }

    [Fact]
    public void Update_LeftWhileChoosingTarget_LeavesTheRailAlone()
    {
        var page = NewPage();
        page.Update(Pressing(GameButton.Confirm));
        Assert.True(page.IsChoosingTarget);

        page.Update(Pressing(GameButton.Left));

        Assert.False(page.IsChoosingCategory);
    }

    // The third starting item is the elixir; the target picker starts on the leader.
    private static void UseThirdItemOnLeader(InventoryPage page)
    {
        page.Update(Pressing(GameButton.Down));
        page.Update(Pressing(GameButton.Down));
        page.Update(Pressing(GameButton.Confirm));
        page.Update(Pressing(GameButton.Confirm));
    }

    private static GameSession NewSession() => StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowClosedDoor);

    private static InventoryPage NewPage(GameSession? session = null) =>
        new(RepositoryScreenText.Text, new UiTextures(), session ?? NewSession(), StartMapScenarios.BelowClosedDoor);
}
