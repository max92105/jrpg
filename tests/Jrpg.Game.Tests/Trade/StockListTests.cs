using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Session;
using Jrpg.Core.Trade;
using Jrpg.Game.Menus;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Trade;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Trade;

// The Buy list is the stock in the shop's order, each row quoted by Core for one copy (decisions 370, 385).
[Collection(RepositoryScreenText.CollectionName)]
public sealed class StockListTests
{
    [Fact]
    public void Rows_GeneralStore_InTheListsOrderWithPrices()
    {
        var session = NewSession();
        var shop = session.ShopFor("general_store");

        var list = new StockList(shop, session.Inventory, Font());

        Assert.Equal(shop.Stock.Definition.ItemIds, list.Rows.Select(row => row.Item.Id).Distinct());
        Assert.All(list.Rows, row => Assert.Equal(row.Piece is null ? NumberText.Of(row.Item.Price!.Value) : NumberText.Of(shop.QuoteBuy(row.Piece).Gold), row.Price.Text));
        Assert.Equal(list.Rows[0], list.SelectedRow);
    }

    [Fact]
    public void Rows_GearEntry_IsItsRolledPieceAtItsQuote()
    {
        var session = NewSession();
        var shop = session.ShopFor("general_store");

        var list = new StockList(shop, session.Inventory, Font());

        var sword = Assert.Single(list.Rows, row => row.Item.Id == "bronze_sword");
        Assert.Equal(shop.Stock.Pieces.Single(piece => piece.Base.Id == "bronze_sword"), sword.Piece);
        Assert.Equal(NumberText.Of(shop.QuoteBuy(sword.Piece!).Gold), sword.Price.Text);
    }

    [Fact]
    public void Rows_OwnedStack_ShowsItsCount()
    {
        var session = NewSession();
        var potion = StartMapScenarios.BelowClosedDoor.Items["potion"];
        var before = session.Inventory.QuantityOf("potion");
        session.Inventory.Add(potion, 3);

        var list = new StockList(session.ShopFor("general_store"), session.Inventory, Font());

        Assert.Equal(NumberText.Of(before + 3), Assert.Single(list.Rows, row => row.Item.Id == "potion").Owned.Text);
    }

    [Fact]
    public void Rows_OwnedPieces_CountsThePiecesOfThatBase()
    {
        var session = NewSession();
        var sword = StartMapScenarios.BelowClosedDoor.Items["bronze_sword"];
        var before = session.Inventory.GearCountOf("bronze_sword");
        session.Inventory.AddGear(GearPiece.Plain(sword));
        session.Inventory.AddGear(GearPiece.Plain(sword));

        var list = new StockList(session.ShopFor("general_store"), session.Inventory, Font());

        Assert.Equal(NumberText.Of(before + 2), Assert.Single(list.Rows, row => row.Item.Id == "bronze_sword").Owned.Text);
    }

    [Fact]
    public void Rows_ShortOfGold_RefusedShortOfGold()
    {
        var session = NewSession();
        Assert.True(session.Party.Purse.TrySpend(session.Party.Purse.Gold));

        var list = new StockList(session.ShopFor("general_store"), session.Inventory, Font());

        Assert.All(list.Rows, row => Assert.Equal(TradeRefusal.ShortOfGold, row.Refusal));
    }

    [Fact]
    public void Refresh_AfterGoldChanged_RowsBecomeAvailable()
    {
        var session = NewSession();
        Assert.True(session.Party.Purse.TrySpend(session.Party.Purse.Gold));
        var list = new StockList(session.ShopFor("general_store"), session.Inventory, Font());
        Assert.False(list.Rows[0].IsAvailable);
        session.Party.Purse.Add(Purse.MaxGold);

        list.Refresh();

        Assert.All(list.Rows, row => Assert.True(row.IsAvailable));
    }

    [Fact]
    public void Refresh_WeaponsCategory_ShowsOnlyWeapons()
    {
        var session = NewSession();
        var list = new StockList(session.ShopFor("general_store"), session.Inventory, Font());
        list.Selection.MoveDown();

        list.Refresh(InventoryCategory.Weapons);

        Assert.NotEmpty(list.Rows);
        Assert.All(list.Rows, row => Assert.Equal(EquipmentKind.Weapon, row.Item.Equipment?.Kind));
        Assert.Equal(0, list.Selection.SelectedIndex);
    }

    private static GameSession NewSession() => StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowClosedDoor);

    private static FontStashSharp.SpriteFontBase Font() => RepositoryScreenText.Text.Fonts.FontAt(ShopView.BodyPixels);
}
