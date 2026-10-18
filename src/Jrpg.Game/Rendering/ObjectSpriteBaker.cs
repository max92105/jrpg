using Jrpg.Core.Maps;
using Jrpg.Data.Art;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Rendering;

/// <summary>
/// Bakes each object's canopy: its tiles composed at screen size, keeping only the pixels inside its above-player zones,
/// so drawing the canopy over the player needs no clipping or shader (plan 3a, key decision 4). It works because every
/// object draws on one layer: an object tile has no Z of its own.
/// </summary>
internal static class ObjectSpriteBaker
{
    /// <param name="objects">Every object definition; only those with above-player zones get a canopy.</param>
    /// <param name="atlas">Where each tile's picture is.</param>
    /// <param name="textures">Supplies the tilesheets and keeps the baked textures.</param>
    /// <param name="cellPixels">Screen pixels per map cell, the size each tile is drawn at.</param>
    /// <returns>The canopy texture of each object id that has one.</returns>
    public static IReadOnlyDictionary<string, Texture2D> BakeCanopies(
        IReadOnlyDictionary<string, ObjectDefinition> objects, TileAtlas atlas, TextureLibrary textures, int cellPixels)
    {
        var sheets = new Dictionary<string, SheetPixels>();
        SheetPixels SheetOf(string file)
        {
            if (!sheets.TryGetValue(file, out var sheet))
            {
                var texture = textures.Get(ArtFolders.Tilesheets, file);
                var pixels = new Color[texture.Width * texture.Height];
                texture.GetData(pixels);
                sheet = new SheetPixels(pixels, texture.Width);
                sheets.Add(file, sheet);
            }

            return sheet;
        }

        var canopies = new Dictionary<string, Texture2D>();
        foreach (var definition in objects.Values.Where(definition => definition.AbovePlayerZones.Count > 0))
        {
            var pixels = CanopyPixels(definition, atlas, SheetOf, cellPixels);
            canopies.Add(definition.Id, textures.Create(definition.Width * cellPixels, definition.Height * cellPixels, pixels));
        }

        return canopies;
    }

    /// <summary>
    /// The object's tiles at <paramref name="cellPixels"/> per cell, scaled by nearest pixel like the map, with every pixel
    /// outside the zones left transparent.
    /// </summary>
    internal static Color[] CanopyPixels(ObjectDefinition definition, TileAtlas atlas, Func<string, SheetPixels> sheetOf, int cellPixels)
    {
        var width = definition.Width * cellPixels;
        var mask = CanopyMask.Build(definition.AbovePlayerZones, width, definition.Height * cellPixels);
        var pixels = new Color[mask.Length];
        foreach (var tile in definition.Tiles)
        {
            var image = atlas.ImageOf(tile.TileId);
            var target = new Point(tile.X * cellPixels, tile.Y * cellPixels);
            CopyTile(sheetOf(image.SheetFile), image.Source, new CanopyTarget(pixels, mask, width, cellPixels), target);
        }

        return pixels;
    }

    private static void CopyTile(SheetPixels sheet, Rectangle source, CanopyTarget canopy, Point topLeft)
    {
        for (var dy = 0; dy < canopy.CellPixels; dy++)
        {
            var sourceRow = source.Y + (dy * source.Height / canopy.CellPixels);
            for (var dx = 0; dx < canopy.CellPixels; dx++)
            {
                var index = ((topLeft.Y + dy) * canopy.Width) + topLeft.X + dx;
                if (canopy.Mask[index])
                {
                    canopy.Pixels[index] = sheet.Pixels[(sourceRow * sheet.Width) + source.X + (dx * source.Width / canopy.CellPixels)];
                }
            }
        }
    }

    private readonly record struct CanopyTarget(Color[] Pixels, bool[] Mask, int Width, int CellPixels);
}
