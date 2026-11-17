using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Core.Sprites;

namespace Jrpg.Game.Rendering;

/// <summary>
/// What every map is drawn with, and the encounters whose battle backgrounds its entry decodes, built once per game so
/// entering a map rebuilds nothing but its view.
/// </summary>
internal sealed class MapArt
{
    /// <param name="catalog">
    /// The tiles, tilesheets, objects and field sprites to draw, and the encounters whose backgrounds a map's battles
    /// can be fought on.
    /// </param>
    /// <param name="cellPixels">Screen pixels per map cell, the size canopies are baked at.</param>
    public MapArt(ContentCatalog catalog, int cellPixels)
    {
        Atlas = new TileAtlas(catalog);
        Canopies = new CanopyTextures(catalog.Objects, Atlas, cellPixels);
        PlayerSprite = catalog.FieldSprites[catalog.NewGame.FieldSpriteFile];
        FieldSprites = catalog.FieldSprites;
        Encounters = catalog.Encounters;
    }

    public TileAtlas Atlas { get; }

    public CanopyTextures Canopies { get; }

    /// <summary>The sheet the player walks with (ADR 0043): the new game's sprite file's record.</summary>
    public FieldSpriteSheet PlayerSprite { get; }

    /// <summary>Every field sprite's record by file name, for the NPCs.</summary>
    public IReadOnlyDictionary<string, FieldSpriteSheet> FieldSprites { get; }

    /// <summary>Every encounter by id, for the backgrounds a map's battles can show (decision 900).</summary>
    public IReadOnlyDictionary<string, EncounterDefinition> Encounters { get; }
}
