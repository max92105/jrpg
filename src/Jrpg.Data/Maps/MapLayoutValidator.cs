using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Data.Content;

namespace Jrpg.Data.Maps;

internal static class MapLayoutValidator
{
    /// <summary>Checks one layout; <paramref name="fileName"/> is its path under the content folder, and the map id must equal its file name.</summary>
    public static void Validate(string fileName, MapLayout layout, MapReferences references)
    {
        var problem = FindShapeProblem(fileName, layout)
            ?? FindLayerProblem(layout, references)
            ?? FindObjectProblem(layout, references);
        if (problem is not null)
        {
            throw new ContentValidationException($"{fileName}: {problem}");
        }
    }

    private static string? FindShapeProblem(string fileName, MapLayout layout)
    {
        var expectedId = Path.GetFileNameWithoutExtension(fileName);
        if (layout.Id != expectedId)
        {
            return $"id must equal the file name '{expectedId}' (was '{layout.Id}').";
        }

        return layout.Width < 1 || layout.Height < 1
            ? $"width and height must be at least 1 (was {layout.Width}x{layout.Height})."
            : null;
    }

    private static string? FindLayerProblem(MapLayout layout, MapReferences references)
    {
        for (var layerIndex = 0; layerIndex < layout.Layers.Count; layerIndex++)
        {
            var problem = FindRowProblem(layout, layout.Layers[layerIndex]) ?? FindUnknownTile(layout, layout.Layers[layerIndex], references);
            if (problem is not null)
            {
                return $"layer {layerIndex}: {problem}";
            }
        }

        return null;
    }

    private static string? FindRowProblem(MapLayout layout, TileLayer? layer)
    {
        // The serializer doesn't apply nullable annotations to collection elements, so a stray null gets here.
        if (layer is null)
        {
            return "is null.";
        }

        if (layer.Rows.Count != layout.Height)
        {
            return $"must have {layout.Height} rows, one per map row (had {layer.Rows.Count}).";
        }

        for (var y = 0; y < layer.Rows.Count; y++)
        {
            var columns = layer.Rows[y]?.Count ?? 0;
            if (columns != layout.Width)
            {
                return $"row {y} must have {layout.Width} tile ids, one per map column (had {columns}).";
            }
        }

        return null;
    }

    private static string? FindUnknownTile(MapLayout layout, TileLayer layer, MapReferences references)
    {
        for (var y = 0; y < layout.Height; y++)
        {
            for (var x = 0; x < layout.Width; x++)
            {
                var tileId = layer.TileIdAt(new GridCell(x, y));
                if (tileId != TileLayer.EmptyTileId && !references.TileIds.Contains(tileId))
                {
                    return $"cell ({x}, {y}) has unknown tile id {tileId}.";
                }
            }
        }

        return null;
    }

    private static string? FindObjectProblem(MapLayout layout, MapReferences references)
    {
        var seenUids = new HashSet<string>();
        foreach (var placed in layout.Objects)
        {
            // The serializer doesn't apply nullable annotations to collection elements, so a stray null gets here.
            if (placed is null)
            {
                return "objects holds a null entry.";
            }

            if (string.IsNullOrWhiteSpace(placed.Uid))
            {
                return $"an object '{placed.ObjectId}' has an empty uid.";
            }

            if (!seenUids.Add(placed.Uid))
            {
                return $"object uid '{placed.Uid}' is used more than once.";
            }

            if (!references.Objects.ContainsKey(placed.ObjectId))
            {
                return $"object '{placed.Uid}' has unknown object id '{placed.ObjectId}'.";
            }
        }

        return null;
    }
}
