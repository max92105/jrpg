using Jrpg.Core.Items;
using Jrpg.Core.Session;
using Jrpg.Core.Stats;
using Jrpg.Core.Trade;
using Jrpg.Game.Tests.Crafting;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Trade;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Trade;

// The Sell list is what the party owns, stacks then pieces in pickup order, each quoted by Core (decisions 375, 378).
[Collection(RepositoryScreenText.CollectionName)]
public sealed class SaleListTests
{
    private static readonly IReadOnlyDictionary<string, ItemDefinition> _items = StartMapScenarios.BelowClosedDoor.Items;

    [Fact]
    public void Rows_StacksThenPieces_InPickupOrder()
    {
        var session = ForgeSessions.WithoutLoosePieces();
        session.Inventory.AddGear(GearPiece.Plain(_items["bronze_sword"]));
        session.Inventory.Add(_items["bomb"], 2);

        var list = NewList(session);

        var ids = list.Rows.Select(row => row.Item.Id).ToList();
        Assert.True(ids.IndexOf("bomb") < ids.IndexOf("bronze_sword"));
        Assert.Equal([.. session.Inventory.Entries.Select(entry => entry.ItemId), .. session.Inventory.Gear.Select(piece => piece.Base.Id)], ids);
    }

    [Fact]
    public void Rows_UnpricedMaterial_RefusedNotForSale()
    {
        var session = ForgeSessions.WithoutLoosePieces();
        session.Inventory.Add(_items["scrap"], 3);

        var list = NewList(session);

        var scrap = Assert.Single(list.Rows, row => row.Item.Id == "scrap");
        Assert.Equal(TradeRefusal.NotForSale, scrap.Refusal);
        Assert.False(scrap.IsAvailable);
    }

    [Fact]
    public void Rows_RolledPiece_PricedAtTheShareOfItsValue()
    {
        var session = ForgeSessions.WithoutLoosePieces();
        var piece = GearPiece.Rolled(_items["bronze_sword"], Rarity.Magic, [new RolledAffix("power", StatKind.Power, 4)]);
        session.Inventory.AddGear(piece);

        var list = NewList(session);

        var row = Assert.Single(list.Rows, row => row.Piece is not null);
        Assert.Equal(NumberText.Of(session.Sales.QuoteSale(piece).Gold), row.Price.Text);
        Assert.Equal(string.Empty, row.Owned.Text);
    }

    [Fact]
    public void Rows_NothingOwned_HasNoCursor()
    {
        var session = ForgeSessions.WithoutLoosePieces();
        foreach (var entry in session.Inventory.Entries.ToList())
        {
            Assert.True(session.Inventory.TryRemove(entry.ItemId, entry.Quantity));
        }

        var list = NewList(session);

        Assert.Empty(list.Rows);
        Assert.Null(list.Selection.Cursor);
        Assert.Null(list.SelectedRow);
    }

    private static SaleList NewList(GameSession session) =>
        new(session.Sales, session.Inventory, _items, RepositoryScreenText.Text.Fonts.FontAt(ShopView.BodyPixels));
}
