using System.Globalization;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;

namespace Jrpg.ContentConverter.Maps;

/// <summary>
/// Turns a legacy map into a layout: its tile list becomes one row grid per distinct z, bottom first, and its
/// objects keep their uid and get the new string object id. Draw order and z overrides are converted in part 3,
/// triggers and events in slice 3c.
/// </summary>
internal static class MapConversion
{
    public const string SourceFolder = "Maps";

    /// <param name="sourceFile">The map's path under the assets folder, such as <c>Maps/start.json</c>.</param>
    /// <param name="legacyMap">The map as the legacy editor wrote it.</param>
    /// <param name="objectIdsByLegacyId">The new object ids, by the legacy int ids placed objects use.</param>
    public static MapLayout Convert(string sourceFile, LegacyMapJson legacyMap, IReadOnlyDictionary<int, string> objectIdsByLegacyId)
    {
        // The serializer doesn't apply nullable annotations to collection elements, so a stray null gets here.
        if (legacyMap.Tiles.Contains(null))
        {
            throw new ConversionException($"{sourceFile}: the tile list holds a null entry.");
        }

        if (legacyMap.Objects.Contains(null))
        {
            throw new ConversionException($"{sourceFile}: the object list holds a null entry.");
        }

        return new MapLayout
        {
            Id = LegacyIds.FromName(sourceFile, Path.GetFileNameWithoutExtension(sourceFile)),
            Width = legacyMap.Width,
            Height = legacyMap.Height,
            Layers = ConvertLayers(sourceFile, legacyMap),
            Objects = [.. legacyMap.Objects.Select(placed => ConvertObject(sourceFile, placed, objectIdsByLegacyId))],
        };
    }

    private static IReadOnlyList<TileLayer> ConvertLayers(string sourceFile, LegacyMapJson legacyMap) =>
        [.. legacyMap.Tiles
            .Where(tile => tile.Id != TileLayer.EmptyTileId)
            .GroupBy(tile => tile.Z)
            .OrderBy(layerTiles => layerTiles.Key)
            .Select(layerTiles => ConvertLayer(sourceFile, legacyMap, layerTiles))];

    private static TileLayer ConvertLayer(string sourceFile, LegacyMapJson legacyMap, IEnumerable<LegacyMapTileJson> layerTiles)
    {
        var rows = Enumerable.Range(0, legacyMap.Height).Select(_ => new int[legacyMap.Width]).ToArray();
        foreach (var tile in layerTiles)
        {
            if (tile.Y < 0 || tile.Y >= legacyMap.Height || tile.X < 0 || tile.X >= legacyMap.Width)
            {
                throw new ConversionException($"{sourceFile}: tile {tile.Id} at ({tile.X}, {tile.Y}) is outside the map.");
            }

            if (rows[tile.Y][tile.X] != TileLayer.EmptyTileId)
            {
                throw new ConversionException($"{sourceFile}: cell ({tile.X}, {tile.Y}) holds two tiles on layer z {tile.Z}.");
            }

            rows[tile.Y][tile.X] = tile.Id;
        }

        return new TileLayer { Rows = rows };
    }

    private static PlacedObject ConvertObject(
        string sourceFile, LegacyPlacedObjectJson placed, IReadOnlyDictionary<int, string> objectIdsByLegacyId)
    {
        var description = string.Create(CultureInfo.InvariantCulture, $"{sourceFile}: object {placed.Id} at ({placed.X:0.###}, {placed.Y:0.###})");
        if (placed.Uid is null)
        {
            throw new ConversionException($"{description} has no uid.");
        }

        if (placed.Meta is { Count: > 0 })
        {
            throw new ConversionException($"{description} has meta data, which the converter doesn't support yet.");
        }

        if (!objectIdsByLegacyId.TryGetValue(placed.Id, out var objectId))
        {
            throw new ConversionException($"{description} uses an object id that isn't in the object list.");
        }

        return new PlacedObject { Uid = placed.Uid, ObjectId = objectId, Position = new Point2D(placed.X, placed.Y) };
    }
}
