using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Data.Art;
using Jrpg.Game.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Exploration;

/// <summary>Draws one map's tile layers, only the cells on screen.</summary>
internal sealed class MapView(MapLayout layout, TileAtlas atlas)
{
    /// <summary>Screen pixels per map cell at the 1920x1080 virtual screen (decision 48, legacy <c>TileSize</c>).</summary>
    public const int CellPixels = 48;

    /// <summary>
    /// The cells the screen shows from <paramref name="origin"/>, with one more column and row for the partly shown ones,
    /// clamped to the map.
    /// </summary>
    public static CellRange VisibleCells(Vector2 origin, MapLayout layout) =>
        new(
            Math.Max(0, (int)MathF.Floor(origin.X / CellPixels)),
            Math.Max(0, (int)MathF.Floor(origin.Y / CellPixels)),
            Math.Min(layout.Width, (int)MathF.Floor((origin.X + VirtualScreen.Width) / CellPixels) + 1),
            Math.Min(layout.Height, (int)MathF.Floor((origin.Y + VirtualScreen.Height) / CellPixels) + 1));

    /// <summary>Draws the layer's cells on screen as seen from the camera's <paramref name="origin"/>, in map pixels.</summary>
    public void DrawLayer(SpriteBatch batch, TextureLibrary textures, TileLayer layer, Vector2 origin)
    {
        var cells = VisibleCells(origin, layout);
        for (var y = cells.Top; y < cells.Bottom; y++)
        {
            for (var x = cells.Left; x < cells.Right; x++)
            {
                var tileId = layer.TileIdAt(new GridCell(x, y));
                if (tileId == TileLayer.EmptyTileId)
                {
                    continue;
                }

                var image = atlas.ImageOf(tileId);
                var destination = new Rectangle((x * CellPixels) - (int)origin.X, (y * CellPixels) - (int)origin.Y, CellPixels, CellPixels);
                batch.Draw(textures.Get(ArtFolders.Tilesheets, image.SheetFile), destination, image.Source, Color.White);
            }
        }
    }
}
