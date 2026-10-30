using Jrpg.Core.Party;
using Jrpg.Core.Session;
using Jrpg.Game.Input;
using Jrpg.Game.Menus;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Trade;
using Jrpg.Game.Ui;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Trade;

// A gear row previews one member's changes, the leader first; Left and Right switch through the active party, wrapping
// (decision 383). A stack has no preview.
[Collection(RepositoryScreenText.CollectionName)]
public sealed class ShopScenePreviewTests
{
    [Fact]
    public void Update_GearRow_PreviewsTheLeader()
    {
        var (shop, session) = ShopOnAGearRow();

        Assert.Same(session.Party.Leader, shop.PreviewedMember);
        Assert.Equal(ChangesFor(session.Party.Leader, shop).Lines, shop.Preview!.Lines);
    }

    [Fact]
    public void Right_OnGearRow_PreviewsTheNextActiveMember()
    {
        var (shop, session) = ShopOnAGearRow();
        var leaderLines = shop.Preview!.Lines;

        shop.Update(TimeSpan.Zero, Pressing(GameButton.Right));

        var member = session.Party.ActiveMembers[1];
        Assert.Same(member, shop.PreviewedMember);
        Assert.NotEqual(leaderLines, shop.Preview!.Lines);
        Assert.Equal(ChangesFor(member, shop).Lines, shop.Preview.Lines);
    }

    [Fact]
    public void Left_OnTheLeader_WrapsToTheLastActiveMember()
    {
        var (shop, session) = ShopOnAGearRow();

        shop.Update(TimeSpan.Zero, Pressing(GameButton.Left));

        Assert.Same(session.Party.ActiveMembers[^1], shop.PreviewedMember);
    }

    [Fact]
    public void Update_ConsumableRow_HasNoPreview()
    {
        var (shop, _) = NewShopInTheList();
        Assert.Null(shop.SelectedRow!.Piece);

        Assert.Null(shop.Preview);
    }

    // Leaving a gear row clears the preview, which the stack's row would otherwise keep showing.
    [Fact]
    public void Up_FromGearToConsumable_ClearsThePreview()
    {
        var (shop, _) = ShopOnAGearRow();
        Assert.NotNull(shop.Preview);

        shop.Update(TimeSpan.Zero, Pressing(GameButton.Up));

        Assert.Null(shop.SelectedRow!.Piece);
        Assert.Null(shop.Preview);
    }

    // The member stays chosen while the cursor moves to another gear row.
    [Fact]
    public void Down_AfterSwitchingMember_KeepsTheMember()
    {
        var (shop, session) = ShopOnAGearRow();
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Right));

        shop.Update(TimeSpan.Zero, Pressing(GameButton.Down));

        Assert.Same(session.Party.ActiveMembers[1], shop.PreviewedMember);
    }

    // What the Equipment page would show for the highlighted piece on that member.
    private static StatComparisonText ChangesFor(PartyMember member, ShopScene shop)
    {
        var comparison = new EquipmentComparison(RepositoryScreenText.Text.Messages);
        comparison.ShowPiece(member, shop.SelectedRow!.Piece!);
        return comparison.Text!;
    }

    // The stock lists its stacks first, so the first gear row is reached going down; the cursor wraps, so coming back
    // to the start fails the loop rather than running it forever when the stock has no gear.
    private static (ShopScene Shop, GameSession Session) ShopOnAGearRow()
    {
        var (shop, session) = NewShopInTheList();
        var start = shop.SelectedRow;
        while (shop.SelectedRow?.Piece is null)
        {
            shop.Update(TimeSpan.Zero, Pressing(GameButton.Down));
            Assert.NotSame(start, shop.SelectedRow);
        }

        return (shop, session);
    }

    // The leader and one recruit are active: the two the Left and Right presses switch between.
    private static (ShopScene Shop, GameSession Session) NewShopInTheList()
    {
        var session = StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowClosedDoor);
        Assert.True(session.Party.TryAddToActive(session.Party.ReserveMembers[0]));
        var shop = new ShopScene(RepositoryScreenText.Text, new UiTextures(), session, session.ShopFor("general_store"));
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));
        return (shop, session);
    }
}
