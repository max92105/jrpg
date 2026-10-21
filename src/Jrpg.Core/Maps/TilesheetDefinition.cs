namespace Jrpg.Core.Maps;

/// <summary>
/// How one tilesheet image is cut into a grid of cells, in the image's own pixels. Tiles are drawn at the map's cell
/// size whatever their size here.
/// </summary>
public sealed record TilesheetDefinition
{
    public required string Id { get; init; }

    /// <summary>The image's file name within the tilesheet art folder (ADR 0009).</summary>
    public required string File { get; init; }

    public required int TileWidth { get; init; }

    public required int TileHeight { get; init; }

    /// <summary>Empty pixels around the whole grid.</summary>
    public required int Margin { get; init; }

    /// <summary>Empty pixels between neighbouring cells.</summary>
    public required int Spacing { get; init; }

    /// <summary>The left edge of a cell column, in image pixels.</summary>
    public int CellLeft(int column) => Margin + (column * (TileWidth + Spacing));

    /// <summary>The top edge of a cell row, in image pixels.</summary>
    public int CellTop(int row) => Margin + (row * (TileHeight + Spacing));

    /// <summary>How many whole cell columns an image of this width holds; a cut-off last column doesn't count.</summary>
    public int ColumnsIn(int imageWidth) => WholeCellsIn(imageWidth, TileWidth);

    /// <summary>How many whole cell rows an image of this height holds; a cut-off last row doesn't count.</summary>
    public int RowsIn(int imageHeight) => WholeCellsIn(imageHeight, TileHeight);

    // Cell n ends at Margin + n * (cell + Spacing) + cell, which must fit in the image; the last cell has no spacing after it.
    private int WholeCellsIn(int imageLength, int cellLength) => Math.Max(0, (imageLength - Margin + Spacing) / (cellLength + Spacing));
}
