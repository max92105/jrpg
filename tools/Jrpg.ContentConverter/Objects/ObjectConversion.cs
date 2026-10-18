using Jrpg.Core.Maps;

namespace Jrpg.ContentConverter.Objects;

/// <summary>
/// Turns the legacy object list into the object catalog: snake_case ids from the names, and the size from the
/// tile group, which is what legacy collision and drawing actually used. Tiles, above-player zones and the
/// category are render or editor data, converted in parts 3 and 4.
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
        var (width, height) = TileGroupSize(legacyObject);
        if (width != legacyObject.Width || height != legacyObject.Height)
        {
            report.AddChanged(
                SourceFile,
                $"object '{legacyObject.Name}': stale size {legacyObject.Width}x{legacyObject.Height} replaced by its tile group's {width}x{height}");
        }

        return new ObjectDefinition
        {
            Id = id,
            Width = width,
            Height = height,
            CollisionPolygon = legacyObject.CollisionPolygon,
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
    private static (int Width, int Height) TileGroupSize(LegacyObjectJson legacyObject)
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

        var columns = legacyObject.Tiles.Select(tile => tile.X).ToList();
        var rows = legacyObject.Tiles.Select(tile => tile.Y).ToList();
        return (columns.Max() - columns.Min() + 1, rows.Max() - rows.Min() + 1);
    }
}
