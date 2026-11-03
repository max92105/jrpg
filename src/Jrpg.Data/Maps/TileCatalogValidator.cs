using System.Globalization;
using Jrpg.Core.Maps;
using Jrpg.Data.Content;

namespace Jrpg.Data.Maps;

internal static class TileCatalogValidator
{
    public static void Validate(string fileName, IReadOnlyList<TileDefinition> tiles, IReadOnlySet<string> sheetIds) =>
        new ContentListValidator<TileDefinition>(fileName, "tile", tile => tile.Id.ToString(CultureInfo.InvariantCulture))
            .Validate(tiles, tile => FindProblem(tile, sheetIds));

    private static string? FindProblem(TileDefinition tile, IReadOnlySet<string> sheetIds)
    {
        if (tile.Id <= TileLayer.EmptyTileId)
        {
            return $"id must be at least 1, since {TileLayer.EmptyTileId} means an empty cell (was {tile.Id}).";
        }

        if (!sheetIds.Contains(tile.SheetId))
        {
            return $"sheetId '{tile.SheetId}' isn't a tilesheet.";
        }

        if (tile.Column < 0 || tile.Row < 0)
        {
            return $"column and row must not be negative (was {tile.Column}, {tile.Row}).";
        }

        return tile.Animation is { } animation ? FindAnimationProblem(animation) : null;
    }

    // ADR 0039: whether its last frame lies inside the image is the startup art check's (ArtReferenceCheck).
    private static string? FindAnimationProblem(TileAnimation animation)
    {
        if (animation.FrameCount < TileAnimation.MinFrames)
        {
            return $"animation.frameCount must be at least {TileAnimation.MinFrames} (was {animation.FrameCount}).";
        }

        return animation.FrameMilliseconds < 1 ? $"animation.frameMilliseconds must be at least 1 (was {animation.FrameMilliseconds})." : null;
    }
}
