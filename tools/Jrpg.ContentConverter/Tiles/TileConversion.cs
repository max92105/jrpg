using System.Globalization;
using Jrpg.Core.Maps;

namespace Jrpg.ContentConverter.Tiles;

/// <summary>Turns the legacy tileset, keyed by id, into the tile catalog.</summary>
internal static class TileConversion
{
    public const string SourceFile = "Tiles/Tileset.json";

    /// <returns>The tiles ordered by id. Legacy names are dropped: the editor shows the sheet cell (question 69).</returns>
    public static IReadOnlyList<TileDefinition> Convert(IReadOnlyDictionary<string, LegacyTileJson> legacyTiles) =>
        [.. legacyTiles
            .Select(entry => ConvertTile(entry.Key, entry.Value))
            .OrderBy(tile => tile.Id)];

    private static TileDefinition ConvertTile(string key, LegacyTileJson? legacyTile)
    {
        // The serializer doesn't apply nullable annotations to dictionary values, so a stray null gets here.
        if (legacyTile is null)
        {
            throw new ConversionException($"{SourceFile}: key '{key}' holds null.");
        }

        if (!int.TryParse(key, NumberStyles.None, CultureInfo.InvariantCulture, out var id))
        {
            throw new ConversionException($"{SourceFile}: key '{key}' isn't a tile id.");
        }

        // The legacy loader silently overwrote the inner id with the key; a disagreement means the file was edited by hand.
        if (id != legacyTile.Id)
        {
            throw new ConversionException($"{SourceFile}: key '{key}' holds tile id {legacyTile.Id}; they must be equal.");
        }

        // No tile has a collision polygon or its own sprite today, so the schema has no place for them yet.
        if (legacyTile.CollisionPolygon is not null || legacyTile.Sprite is not null || legacyTile.Frames is not null)
        {
            throw new ConversionException($"{SourceFile}: tile {id} has a sprite, frames or a collision polygon, which the converter doesn't support yet.");
        }

        // Every tile today comes from a sheet cell; a tile without one would have nothing to draw.
        if (legacyTile.Tilesheet is null || legacyTile.SheetX is not { } column || legacyTile.SheetY is not { } row)
        {
            throw new ConversionException($"{SourceFile}: tile {id} has no tilesheet cell.");
        }

        return new TileDefinition
        {
            Id = id,
            IsWalkable = legacyTile.IsWalkable,
            SheetId = TilesheetConversion.IdOf(legacyTile.Tilesheet),
            Column = column,
            Row = row,
        };
    }
}
