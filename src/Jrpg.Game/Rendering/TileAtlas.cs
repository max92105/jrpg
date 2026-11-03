using Jrpg.Core.Content;
using Jrpg.Core.Maps;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Rendering;

/// <summary>
/// Each tile's picture, worked out once from the catalog so drawing only looks it up: one source rectangle for a still
/// tile, one per frame for an animated one, its own cell then the cells to its right (ADR 0039).
/// </summary>
internal sealed class TileAtlas
{
    private readonly Dictionary<int, TileFrames> _tiles = [];

    public TileAtlas(ContentCatalog catalog)
    {
        foreach (var tile in catalog.Tiles.Values)
        {
            var sheet = catalog.Tilesheets[tile.SheetId];
            var sources = Enumerable.Range(tile.Column, tile.Animation?.FrameCount ?? 1)
                .Select(column => new Rectangle(sheet.CellLeft(column), sheet.CellTop(tile.Row), sheet.TileWidth, sheet.TileHeight))
                .ToArray();
            _tiles.Add(tile.Id, new TileFrames(sheet.File, sources, tile.Animation));
        }
    }

    /// <summary>The first frame, the tile's own cell: what a canopy is baked from (decision 465).</summary>
    public TileImage ImageOf(int tileId) => ImageAt(tileId, TimeSpan.Zero);

    /// <summary>
    /// The frame shown after <paramref name="elapsed"/> of the map's animation time; a still tile's own cell.
    /// </summary>
    public TileImage ImageAt(int tileId, TimeSpan elapsed)
    {
        var tile = _tiles[tileId];
        return new TileImage(tile.SheetFile, tile.Sources[tile.Animation?.FrameAt(elapsed) ?? 0]);
    }

    private sealed record TileFrames(string SheetFile, Rectangle[] Sources, TileAnimation? Animation);
}
