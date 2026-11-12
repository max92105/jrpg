using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Data.Art;
using Jrpg.Game.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Exploration;

/// <summary>
/// Draws one map's tile layers, only the cells on screen, and its placed objects as they are now (an opened door draws
/// open). It holds the time its animated tiles show (decision 464): from zero on entering the map, moved on by the
/// scene.
/// </summary>
internal sealed class MapView(MapLayout layout, Func<PlacedObject, ObjectDefinition> currentDefinitionOf, MapArt art)
{
    /// <summary>Screen pixels per map cell at the 1920x1080 virtual screen (decision 48).</summary>
    public const int CellPixels = 48;

    /// <summary>How long the map has run, for its animated tiles; every copy of a tile shows the same frame.</summary>
    public TimeSpan AnimationTime { get; private set; }

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

    public void Advance(TimeSpan delta) => AnimationTime += delta;

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

    /// <summary>
    /// Draws a placed object at its position, as seen from the camera's <paramref name="origin"/>: its tiles, or an
    /// NPC's standing frame from its sheet's record, standing on its cell (ADR 0043).
    /// </summary>
    public void DrawObject(SpriteBatch batch, TextureLibrary textures, PlacedObject placed, Vector2 origin)
    {
        var definition = currentDefinitionOf(placed);
        var area = ObjectScreenArea.Of(placed, definition, art.FieldSprites, origin);
        if (!IsOnScreen(area))
        {
            return;
        }

        // An NPC stands still facing down (decisions 127, 128) on its sheet's standing frame.
        if (definition.FieldSpriteFile is { } spriteFile)
        {
            var sheet = art.FieldSprites[spriteFile];
            var source = FieldSpriteFrame.SourceOf(sheet, sheet.StandingCell(Facing.Down));
            batch.Draw(textures.Get(ArtFolders.FieldSprites, sheet.File), area, source, Color.White);
            return;
        }

        // Listed bottom level first (ADR 0040): drawing the list in order draws each stack.
        var tiles = definition.Tiles;
        for (var index = 0; index < tiles.Count; index++)
        {
            var tile = tiles[index];
            DrawTile(batch, textures, tile.TileId, new Point(area.X + (tile.X * CellPixels), area.Y + (tile.Y * CellPixels)));
        }
    }

    /// <summary>
    /// Draws a placed object's baked canopy over its body, as seen from the camera's <paramref name="origin"/>; a
    /// field-sprite object has no canopy, so the area is always the object's cells.
    /// </summary>
    public void DrawCanopy(SpriteBatch batch, Texture2D canopy, PlacedObject placed, Vector2 origin)
    {
        var area = ObjectScreenArea.Of(placed, currentDefinitionOf(placed), art.FieldSprites, origin);
        if (IsOnScreen(area))
        {
            batch.Draw(canopy, area, Color.White);
        }
    }

    private static bool IsOnScreen(Rectangle area) =>
        area.Right > 0 && area.Bottom > 0 && area.Left < VirtualScreen.Width && area.Top < VirtualScreen.Height;

    private void DrawTile(SpriteBatch batch, TextureLibrary textures, int tileId, Point screenTopLeft)
    {
        var image = art.Atlas.ImageAt(tileId, AnimationTime);
        var destination = new Rectangle(screenTopLeft.X, screenTopLeft.Y, CellPixels, CellPixels);
        batch.Draw(textures.Get(ArtFolders.Tilesheets, image.SheetFile), destination, image.Source, Color.White);
    }
}
