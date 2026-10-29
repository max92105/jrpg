using Jrpg.Game.Crafting;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Crafting;

// Every material is listed, 0 included, since the costs name them (decision 370).
[Collection(RepositoryScreenText.CollectionName)]
public sealed class MaterialStockTests
{
    [Fact]
    public void Rows_NoneOwned_ListsEveryMaterialAtZero()
    {
        var session = ForgeSessions.WithoutLoosePieces();
        var items = StartMapScenarios.BelowClosedDoor.Items;
        Assert.Equal(0, session.Inventory.QuantityOf("scrap"));

        var stock = new MaterialStock(session.Inventory, items, new SheetRowLook(RepositoryScreenText.Text));

        Assert.Contains(stock.Rows, row => row.Name == items["scrap"].Name && row.Count.Text == "0");
        Assert.Contains(stock.Rows, row => row.Name == items["essence"].Name && row.Count.Text == "0");
    }

    [Fact]
    public void Refresh_AfterScrapAdded_ShowsTheNewCount()
    {
        var session = ForgeSessions.WithoutLoosePieces();
        var items = StartMapScenarios.BelowClosedDoor.Items;
        var stock = new MaterialStock(session.Inventory, items, new SheetRowLook(RepositoryScreenText.Text));
        Assert.Contains(stock.Rows, row => row.Name == items["scrap"].Name && row.Count.Text == "0");
        session.Inventory.Add(items["scrap"], 7);

        stock.Refresh();

        Assert.Contains(stock.Rows, row => row.Name == items["scrap"].Name && row.Count.Text == "7");
    }
}
