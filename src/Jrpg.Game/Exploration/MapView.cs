using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Data.Art;
using Jrpg.Game.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Exploration;

/// <summary>
/// Draws one map's tile layers, only the cells on screen, and its placed objects as they are now (an opened door draws
/// open).
/// </summary>
internal sealed class MapView(MapLayout layout, Func<PlacedObject, ObjectDefinition> currentDefinitionOf, TileAtlas atlas)
{
    /// <summary>Screen pixels per map cell at the 1920x1080 virtual screen (decision 48).</summary>
    public const int CellPixels = 48;

    /// <summary>
    /// The cells the screen shows from <paramref name="origin"/>, with one more column and row for the partly shown
    /// ones, clamped to the map.
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

                DrawTile(batch, textures, tileId, new Point((x * CellPixels) - (int)origin.X, (y * CellPixels) - (int)origin.Y));
            }
        }
    }

    /// <summary>Draws a placed object's tiles at its position, as seen from the camera's <paramref name="origin"/>.</summary>
    public void DrawObject(SpriteBatch batch, TextureLibrary textures, PlacedObject placed, Vector2 origin)
    {
        var definition = currentDefinitionOf(placed);
        var area = ScreenAreaOf(placed, definition, origin);
        if (!IsOnScreen(area))
        {
            return;
        }

        // An NPC stands still facing down (decisions 127, 128).
        if (definition.FieldSpriteFile is { } spriteFile)
        {
            batch.Draw(textures.Get(ArtFolders.FieldSprites, spriteFile), area, FieldSpriteFrame.SourceOf(Facing.Down, 0), Color.White);
            return;
        }

        var tiles = definition.Tiles;
        for (var index = 0; index < tiles.Count; index++)
        {
            var tile = tiles[index];
            DrawTile(batch, textures, tile.TileId, new Point(area.X + (tile.X * CellPixels), area.Y + (tile.Y * CellPixels)));
        }
    }

    /// <summary>Draws a placed object's baked canopy over its body, as seen from the camera's <paramref name="origin"/>.</summary>
    public void DrawCanopy(SpriteBatch batch, Texture2D canopy, PlacedObject placed, Vector2 origin)
    {
        var area = ScreenAreaOf(placed, currentDefinitionOf(placed), origin);
        if (IsOnScreen(area))
        {
            batch.Draw(canopy, area, Color.White);
        }
    }

    // Objects can sit between cells (positions such as 2.5), so the top-left is rounded to a whole pixel once.
    private static Rectangle ScreenAreaOf(PlacedObject placed, ObjectDefinition definition, Vector2 origin) =>
        new(
            (int)Math.Round((placed.Position.X * CellPixels) - origin.X),
            (int)Math.Round((placed.Position.Y * CellPixels) - origin.Y),
            definition.Width * CellPixels,
            definition.Height * CellPixels);

    private static bool IsOnScreen(Rectangle area) =>
        area.Right > 0 && area.Bottom > 0 && area.Left < VirtualScreen.Width && area.Top < VirtualScreen.Height;

    private void DrawTile(SpriteBatch batch, TextureLibrary textures, int tileId, Point screenTopLeft)
    {
        var image = atlas.ImageOf(tileId);
        var destination = new Rectangle(screenTopLeft.X, screenTopLeft.Y, CellPixels, CellPixels);
        batch.Draw(textures.Get(ArtFolders.Tilesheets, image.SheetFile), destination, image.Source, Color.White);
    }
}
