using Jrpg.Core.Maps;

namespace Jrpg.Data.Art;

/// <summary>A Tiled tileset (<c>.tsx</c>) as the editor's animation import needs it (decision 469).</summary>
public sealed record TiledTileset
{
    /// <summary>The <c>&lt;image source&gt;</c>'s file name, without its folder.</summary>
    public required string ImageFile { get; init; }

    public required int TileWidth { get; init; }

    public required int TileHeight { get; init; }

    /// <summary>Pixels around the grid; 0 when the file doesn't say.</summary>
    public required int Margin { get; init; }

    /// <summary>Pixels between cells; 0 when the file doesn't say.</summary>
    public required int Spacing { get; init; }

    public required int Columns { get; init; }

    /// <summary>Each animated tile's frame count and time, by its cell.</summary>
    public required IReadOnlyDictionary<(int Column, int Row), TileAnimation> Animations { get; init; }

    /// <summary>
    /// Why the first animation of another shape than ADR 0039's (frames the tile then the next ones on its row, one
    /// time for all) can't be imported, naming its tile; null when every animation fits.
    /// </summary>
    public required string? Refusal { get; init; }
}
