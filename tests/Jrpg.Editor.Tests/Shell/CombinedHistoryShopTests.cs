using Jrpg.Core.Events;
using Jrpg.Core.Geometry;
using Jrpg.Core.Trade;
using Jrpg.Editor.Maps.Editing;
using Jrpg.Editor.Shell;

namespace Jrpg.Editor.Tests.Shell;

// An undo that would take away a shop a saved map's event opens is refused naming the map (decision 581, as decision
// 394 refuses a used quest).
public sealed class CombinedHistoryShopTests : IDisposable
{
    private static readonly ShopDefinition _armory = new("armory", "Armory", ["bronze_sword"]);

    private readonly EditorTestContent _content = new();

    public void Dispose() => _content.Dispose();

    [Fact]
    public void Undo_RemovesAShopAnEventOpens_RefusedNamingTheMap()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        viewModel.Content!.SetShops([.. viewModel.Catalog!.Shops.Values, _armory]);
        var mapId = SaveWithCellEvent(viewModel, new OpenShopStep("armory"));

        viewModel.History!.UndoCommand.Execute(null);

        Assert.Equal($"Can't undo: shop armory is opened on map {mapId}.", viewModel.History.Refusal);
        Assert.True(viewModel.Catalog.Shops.ContainsKey("armory"));
    }

    [Fact]
    public void Undo_RemovesAShopNobodyOpens_Undoes()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        viewModel.Content!.SetShops([.. viewModel.Catalog!.Shops.Values, _armory]);
        SaveWithCellEvent(viewModel, new OpenShopStep("general_store"));

        viewModel.History!.UndoCommand.Execute(null);

        Assert.Null(viewModel.History.Refusal);
        Assert.False(viewModel.Catalog.Shops.ContainsKey("armory"));
    }

    // The open map's unsaved step counts too: placed before the shop, so the shop's step is the next undo.
    [Fact]
    public void Undo_RemovesAShopAnUnsavedEventOpens_RefusedNamingTheMap()
    {
        var viewModel = _content.Open(_content.TestWorld);
        var map = viewModel.CurrentMap!;
        var opensArmory = new MapEvent
        {
            Id = "shop_check",
            Activation = EventActivation.Touch,
            Steps = [new OpenShopStep("armory")],
        };
        map.Do(SetCellEventEdit.For(map.Map.Layout.CellEvents, new GridCell(0, 0), opensArmory));
        viewModel.Content!.SetShops([.. viewModel.Catalog!.Shops.Values, _armory]);

        viewModel.History!.UndoCommand.Execute(null);

        Assert.Equal($"Can't undo: shop armory is opened on map {map.Map.Id}.", viewModel.History.Refusal);
    }

    // Puts an event with the step on the open map's top-left cell, saves, and opens another map so only the catalog
    // step is left to undo.
    private static string SaveWithCellEvent(MainViewModel viewModel, EventStep step)
    {
        var map = viewModel.CurrentMap!;
        var mapEvent = new MapEvent { Id = "shop_check", Activation = EventActivation.Touch, Steps = [step] };
        map.Do(SetCellEventEdit.For(map.Map.Layout.CellEvents, new GridCell(0, 0), mapEvent));
        viewModel.SaveCommand.Execute(null);
        viewModel.TryOpenMap(viewModel.MapIds.First(id => id != map.Map.Id));
        return map.Map.Id;
    }
}
