using Jrpg.Core.Maps;

namespace Jrpg.ContentConverter.Objects;

/// <summary>
/// Turns the legacy object list into the object catalog: snake_case ids from the names, and the size from the
/// tile group, which is what legacy collision and drawing actually used. The tile group moves to start at (0, 0);
/// the collision polygon and above-player zones are already fractions of the group's bounds, so they carry over.
/// </summary>
internal static class ObjectConversion
{
    public const string SourceFile = "Objects/Objects.json";

    public static ConvertedObjects Convert(IReadOnlyList<LegacyObjectJson> legacyObjects, ConversionReport report)
    {
        var namesById = new Dictionary<string, string>();
        var idsByLegacyId = new Dictionary<int, string>();
        var objects = new List<ObjectDefinition>();
        foreach (var legacyObject in legacyObjects)
        {
            var converted = ConvertObject(legacyObject, report);
            if (!namesById.TryAdd(converted.Id, legacyObject.Name))
            {
                throw new ConversionException(
                    $"{SourceFile}: objects '{namesById[converted.Id]}' and '{legacyObject.Name}' both become id '{converted.Id}'.");
            }

            if (!idsByLegacyId.TryAdd(legacyObject.Id, converted.Id))
            {
                throw new ConversionException($"{SourceFile}: legacy id {legacyObject.Id} is used by more than one object.");
            }

            objects.Add(converted);
        }

        return new ConvertedObjects([.. objects.OrderBy(definition => definition.Id, StringComparer.Ordinal)], idsByLegacyId);
    }

    private static ObjectDefinition ConvertObject(LegacyObjectJson? legacyObject, ConversionReport report)
    {
        // The serializer doesn't apply nullable annotations to collection elements, so a stray null gets here.
        if (legacyObject is null)
        {
            throw new ConversionException($"{SourceFile}: the list holds a null object.");
        }

        RejectUnsupportedFields(legacyObject);
        var id = LegacyIds.FromName(SourceFile, legacyObject.Name);
        var bounds = TileGroupBounds(legacyObject);
        if (bounds.Width != legacyObject.Width || bounds.Height != legacyObject.Height)
        {
            report.AddChanged(
                SourceFile,
                $"object '{legacyObject.Name}': stale size {legacyObject.Width}x{legacyObject.Height} replaced by its tile group's {bounds.Width}x{bounds.Height}");
        }

        return new ObjectDefinition
        {
            Id = id,
            Width = bounds.Width,
            Height = bounds.Height,
            CollisionPolygon = legacyObject.CollisionPolygon,
            Category = legacyObject.Category,
            Tiles = [.. legacyObject.Tiles.Select(tile => new ObjectTile(tile.X - bounds.Left, tile.Y - bounds.Top, tile.TileId))],
            AbovePlayerZones = legacyObject.AbovePlayerZones,
        };
    }

    private static void RejectUnsupportedFields(LegacyObjectJson legacyObject)
    {
        // No object uses these today, so the schema has no place for them yet.
        if (legacyObject.IsNpc)
        {
            throw new ConversionException($"{SourceFile}: object '{legacyObject.Name}' is an NPC, which the converter doesn't support yet.");
        }

        if (legacyObject.Meta.Count > 0)
        {
            throw new ConversionException($"{SourceFile}: object '{legacyObject.Name}' has meta data, which the converter doesn't support yet.");
        }
    }

    /// <summary>The tile group's bounds, as legacy <c>ObjectDefinition.GetTileBounds</c> computed them.</summary>
    private static (int Left, int Top, int Width, int Height) TileGroupBounds(LegacyObjectJson legacyObject)
    {
        if (legacyObject.Tiles.Count == 0)
        {
            throw new ConversionException($"{SourceFile}: object '{legacyObject.Name}' has no tiles, so it has no size.");
        }

        // The serializer doesn't apply nullable annotations to collection elements, so a stray null gets here.
        if (legacyObject.Tiles.Any(tile => tile is null))
        {
            throw new ConversionException($"{SourceFile}: object '{legacyObject.Name}' has a null tile.");
        }

        // Every object draws on one layer, which lets the game bake its canopy (slice 3b); a per-tile Z would break that.
        var offsetTile = legacyObject.Tiles.FirstOrDefault(tile => tile.Z != 0);
        if (offsetTile is not null)
        {
            throw new ConversionException(
                $"{SourceFile}: object '{legacyObject.Name}' has tile ({offsetTile.X}, {offsetTile.Y}) with Z offset {offsetTile.Z}, which the converter doesn't support.");
        }

        var columns = legacyObject.Tiles.Select(tile => tile.X).ToList();
        var rows = legacyObject.Tiles.Select(tile => tile.Y).ToList();
        return (columns.Min(), rows.Min(), columns.Max() - columns.Min() + 1, rows.Max() - rows.Min() + 1);
    }
}
