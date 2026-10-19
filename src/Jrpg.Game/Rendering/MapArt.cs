using Jrpg.Core.Content;

namespace Jrpg.Game.Rendering;

/// <summary>What every map is drawn with, built once per game so entering a map rebuilds nothing but its view.</summary>
internal sealed class MapArt
{
    /// <param name="catalog">The tiles, tilesheets and objects to draw.</param>
    /// <param name="cellPixels">Screen pixels per map cell, the size canopies are baked at.</param>
    public MapArt(ContentCatalog catalog, int cellPixels)
    {
        Atlas = new TileAtlas(catalog);
        Canopies = new CanopyTextures(catalog.Objects, Atlas, cellPixels);
    }

    public TileAtlas Atlas { get; }

    public CanopyTextures Canopies { get; }
}
