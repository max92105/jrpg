namespace Jrpg.Editor.Catalogs;

internal sealed partial class CatalogDraft
{
    /// <summary>
    /// Takes a sheet and every tile cut from it out as one undo step (decision 443); the caller has checked nothing
    /// uses the tiles.
    /// </summary>
    /// <exception cref="InvalidOperationException">No sheet has the id.</exception>
    public void RemoveTilesheet(string sheetId)
    {
        var sheet = Current.Tilesheets.GetValueOrDefault(sheetId) ?? throw new InvalidOperationException($"No sheet '{sheetId}' to remove.");
        var tiles = Current.Tiles.Values.Where(tile => tile.SheetId == sheetId).ToList();
        Do(new CatalogEdit(
            catalog => WithoutTilesheet(catalog, sheetId) with { Tiles = catalog.Tiles.Where(pair => pair.Value.SheetId != sheetId).ToDictionary() },
            catalog => WithTilesheet(catalog, sheet) with { Tiles = catalog.Tiles.Values.Concat(tiles).ToDictionary(tile => tile.Id) }));
    }

    /// <summary>
    /// Takes an object definition out as one undo step (decision 443); the caller has checked nothing uses it.
    /// </summary>
    /// <exception cref="InvalidOperationException">No object has the id.</exception>
    public void RemoveObject(string objectId)
    {
        var before = Current.Objects.GetValueOrDefault(objectId) ?? throw new InvalidOperationException($"No object '{objectId}' to remove.");
        Do(new CatalogEdit(catalog => WithoutObject(catalog, objectId), catalog => WithObject(catalog, before)));
    }
}
