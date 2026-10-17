using System.Globalization;
using Jrpg.Core.Maps;
using Jrpg.Data.Content;

namespace Jrpg.Data.Maps;

internal static class TileCatalogValidator
{
    public static void Validate(string fileName, IReadOnlyList<TileDefinition> tiles) =>
        new ContentListValidator<TileDefinition>(fileName, "tile", tile => tile.Id.ToString(CultureInfo.InvariantCulture))
            .Validate(tiles, FindProblem);

    private static string? FindProblem(TileDefinition tile) =>
        tile.Id <= TileLayer.EmptyTileId
            ? $"id must be at least 1, since {TileLayer.EmptyTileId} means an empty cell (was {tile.Id})."
            : null;
}
