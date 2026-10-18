using Jrpg.Core.Content;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Rendering;

/// <summary>Each tile's picture, worked out once from the catalog so drawing only looks it up.</summary>
internal sealed class TileAtlas
{
    private readonly Dictionary<int, TileImage> _images = [];

    public TileAtlas(ContentCatalog catalog)
    {
        foreach (var tile in catalog.Tiles.Values)
        {
            var sheet = catalog.Tilesheets[tile.SheetId];
            var source = new Rectangle(sheet.CellLeft(tile.Column), sheet.CellTop(tile.Row), sheet.TileWidth, sheet.TileHeight);
            _images.Add(tile.Id, new TileImage(sheet.File, source));
        }
    }

    public TileImage ImageOf(int tileId) => _images[tileId];
}
