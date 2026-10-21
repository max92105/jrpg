using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Maps;

namespace Jrpg.Editor.Tiles;

/// <summary>
/// The Cut panel: how one sheet image is cut, entered as columns and rows, with the tile size they give shown read-only
/// (decision 115). Nothing changes in the catalog until the cut is applied.
/// </summary>
internal sealed partial class SheetCutViewModel : ObservableObject
{
    private readonly TilesheetDefinition _sheet;
    private readonly (int Width, int Height) _imageSize;

    /// <param name="sheet">The cut it starts from; its id and file are kept.</param>
    /// <param name="imageSize">The image's size in its own pixels.</param>
    /// <param name="tiles">The tiles already cut from the sheet.</param>
    public SheetCutViewModel(TilesheetDefinition sheet, (int Width, int Height) imageSize, IReadOnlyList<TileDefinition> tiles)
    {
        _sheet = sheet;
        _imageSize = imageSize;
        Tiles = tiles;
        Columns = sheet.ColumnsIn(imageSize.Width);
        Rows = sheet.RowsIn(imageSize.Height);
        Margin = sheet.Margin;
        Spacing = sheet.Spacing;
    }

    public string SheetId => _sheet.Id;

    /// <summary>
    /// The tiles cut from the sheet, which keep their column and row (decision 116); kept current while the panel is open,
    /// as painting can make one.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem), nameof(Summary))]
    public partial IReadOnlyList<TileDefinition> Tiles { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TileWidth), nameof(Problem), nameof(Summary), nameof(PreviewSheet))]
    public partial int Columns { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TileHeight), nameof(Problem), nameof(Summary), nameof(PreviewSheet))]
    public partial int Rows { get; set; }

    /// <summary>Empty pixels around the whole grid.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TileWidth), nameof(TileHeight), nameof(Problem), nameof(Summary), nameof(PreviewSheet))]
    public partial int Margin { get; set; }

    /// <summary>Empty pixels between neighbouring cells.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TileWidth), nameof(TileHeight), nameof(Problem), nameof(Summary), nameof(PreviewSheet))]
    public partial int Spacing { get; set; }

    /// <summary>In image pixels; rounded down when the width doesn't split evenly.</summary>
    public int TileWidth => TileLengthAlong(_imageSize.Width, Columns);

    /// <summary>In image pixels; rounded down when the height doesn't split evenly.</summary>
    public int TileHeight => TileLengthAlong(_imageSize.Height, Rows);

    /// <summary>Why the cut can't be applied, or null when it splits the image into whole tiles.</summary>
    public string? Problem
    {
        get
        {
            if (Columns < 1 || Rows < 1)
            {
                return "Columns and rows must be at least 1.";
            }

            if (Margin < 0 || Spacing < 0)
            {
                return "Margin and spacing must not be negative.";
            }

            return ProblemAlong(_imageSize.Width, Columns, "columns") ?? ProblemAlong(_imageSize.Height, Rows, "rows") ?? MarginProblem() ?? TileOutsideProblem();
        }
    }

    /// <summary>"48 x 48 px; 8 x 16 = 128 cells"; empty while there is a problem.</summary>
    public string Summary => Problem is null ? $"{TileWidth} x {TileHeight} px; {Columns} x {Rows} = {Columns * Rows} cells" : "";

    /// <summary>The sheet as the fields stand, for the grid drawn over the image; null when they give no tile to draw.</summary>
    public TilesheetDefinition? PreviewSheet =>
        Columns >= 1 && Rows >= 1 && Margin >= 0 && Spacing >= 0 && TileWidth >= 1 && TileHeight >= 1
            ? _sheet with { TileWidth = TileWidth, TileHeight = TileHeight, Margin = Margin, Spacing = Spacing }
            : null;

    /// <exception cref="InvalidOperationException">The cut has a <see cref="Problem"/>; its text is the message.</exception>
    public TilesheetDefinition ToSheet() =>
        Problem is { } problem ? throw new InvalidOperationException(problem) : PreviewSheet!;

    // The image less the margin on both sides and the spacing between cells, shared by the cells.
    private int SpanAlong(int imageLength, int cells) => imageLength - (2 * Margin) - ((cells - 1) * Spacing);

    private int TileLengthAlong(int imageLength, int cells) => cells < 1 ? 0 : SpanAlong(imageLength, cells) / cells;

    private string? ProblemAlong(int imageLength, int cells, string cellsName)
    {
        if (TileLengthAlong(imageLength, cells) < 1)
        {
            return $"{imageLength} px is too small for {cells} {cellsName}.";
        }

        return SpanAlong(imageLength, cells) % cells != 0 ? $"{imageLength} px doesn't split into {cells} {cellsName}." : null;
    }

    // The sheet stores the margin once, before the cells, so the grid counts whole cells to the image's edge: a margin as
    // wide as a cell and its spacing would give the saved sheet one more column or row than asked for.
    private string? MarginProblem() =>
        PreviewSheet is { } cut && (cut.ColumnsIn(_imageSize.Width) != Columns || cut.RowsIn(_imageSize.Height) != Rows)
            ? $"A {Margin} px margin leaves room for another cell; make it smaller than a cell."
            : null;

    // A tile past the new grid would fail the art check on save, and the maps using it would no longer open.
    private string? TileOutsideProblem() =>
        Tiles.Where(tile => tile.Column >= Columns || tile.Row >= Rows).MinBy(tile => tile.Id) is { } outside
            ? $"Tile {outside.Id} at ({outside.Column}, {outside.Row}) would be outside the {Columns} x {Rows} grid."
            : null;
}
