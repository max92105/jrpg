using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Core.Battle;
using Jrpg.Core.Sprites;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Sprites;
using Jrpg.Editor.Tiles;

namespace Jrpg.Editor.Animation;

/// <summary>
/// Strip from sheet cells as typed (decision 990): a sheet's size, its cut as columns and rows (the sprite setup's
/// rule, <see cref="GridCut.Plain"/>), the cells picked on it in order (a cell may repeat), the strip's file name and
/// pace, and the problem line that greys OK. It holds no catalog session: the window reads the sheet's image and writes
/// the strip <see cref="ToStrip"/> gives.
/// </summary>
internal sealed partial class StripFromCellsViewModel : ObservableObject
{
    private const string StripExtension = ".png";

    // Data refuses a strip under 1 ms a frame.
    private const int LowestFrameMilliseconds = 1;

    private static readonly char[] _pathCharacters =
        [.. Path.GetInvalidFileNameChars(), Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar];

    private (int Width, int Height)? _sheetSize;

    /// <summary>
    /// Raised when the picks or the cut change, for the window's preview and marks; a new sheet may raise it more than
    /// once, as it clears the picks and guesses the cut.
    /// </summary>
    public event EventHandler? PicksChanged;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CellSize), nameof(CutLine), nameof(Problem), nameof(CanWrite))]
    public partial string ColumnsText { get; set; } = TypedNumber.Text(1);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CellSize), nameof(CutLine), nameof(Problem), nameof(CanWrite))]
    public partial string RowsText { get; set; } = TypedNumber.Text(1);

    /// <summary>Whether a click on the sheet adds the cell under it; on once a sheet is taken.</summary>
    [ObservableProperty]
    public partial bool IsPicking { get; set; }

    /// <summary>
    /// The picked cells in play order, column and row from 0; replaced whole on a change, so it notifies.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PicksLine), nameof(Problem), nameof(CanWrite))]
    [NotifyCanExecuteChangedFor(nameof(RemoveLastCommand), nameof(ClearCommand))]
    public partial IReadOnlyList<SpriteCell> Picks { get; private set; } = [];

    /// <summary>The strip's file name in the effects folder; ".png" is added when it is typed without one.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem), nameof(CanWrite))]
    public partial string StripNameText { get; set; } = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FrameMilliseconds), nameof(Problem), nameof(CanWrite))]
    public partial string FrameMillisecondsText { get; set; } =
        TypedNumber.Text((int)EffectStripDefinition.UnlistedFrameTime.TotalMilliseconds);

    /// <summary>A cell's size in the sheet's pixels; null without a sheet or while the cut is a problem.</summary>
    public (int Width, int Height)? CellSize =>
        _sheetSize is { } size && WholeCut() is { } cut
            ? (GridCut.Plain.LengthAlong(size.Width, cut.Columns), GridCut.Plain.LengthAlong(size.Height, cut.Rows))
            : null;

    /// <summary>"6 x 2 cells of 192 x 192 px"; empty without a sheet or while the cut is a problem.</summary>
    public string CutLine =>
        WholeCut() is { } cut && CellSize is { } cell
            ? $"{cut.Columns} x {cut.Rows} cells of {cell.Width} x {cell.Height} px"
            : "";

    /// <summary>"4 cells: (1, 1) (2, 1) (3, 1) (2, 1)", numbered from 1 as the sheet's marks show them.</summary>
    public string PicksLine => CellPicking.Line(Picks);

    /// <summary>The Frame time box as a number from 1, which the preview plays at; null while it isn't one.</summary>
    public int? FrameMilliseconds =>
        TypedNumber.Parse(FrameMillisecondsText) is { } frameTime and >= LowestFrameMilliseconds ? frameTime : null;

    /// <summary>
    /// Why OK can't write the strip, or null: the sheet, the cut, the picks, the name, then the pace.
    /// </summary>
    public string? Problem => _sheetSize is not { } size
        ? "Open a sheet."
        : CutProblem(size)
            ?? (Picks.Count == 0 ? "Pick the cells the strip plays, in order." : null)
            ?? NameProblem()
            ?? (FrameMilliseconds is null ? $"Frame time must be a whole number from {LowestFrameMilliseconds}." : null);

    public bool CanWrite => Problem is null;

    /// <summary>
    /// Starts on a newly opened sheet: the picks cleared and picking on, the cut guessed as one row of square cells
    /// when the width splits into squares of the height (an effects strip), else as 1 by 1, and the strip named as the
    /// sheet while it has no name. The window reads the image; only its size comes here.
    /// </summary>
    /// <param name="fileName">The sheet's file name, without its folder.</param>
    /// <param name="size">The sheet's size in its own pixels.</param>
    public void TakeSheet(string fileName, (int Width, int Height) size)
    {
        _sheetSize = size;
        Picks = [];
        var isRowOfSquares = size.Height > 0 && size.Width % size.Height == 0;
        ColumnsText = TypedNumber.Text(isRowOfSquares ? size.Width / size.Height : 1);
        RowsText = TypedNumber.Text(1);
        if (StripNameText.Trim().Length == 0)
        {
            StripNameText = fileName;
        }

        IsPicking = true;
        OnPropertyChanged(nameof(CellSize));
        OnPropertyChanged(nameof(CutLine));
        OnPropertyChanged(nameof(Problem));
        OnPropertyChanged(nameof(CanWrite));
        PicksChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Adds the cell under a point of the sheet to the picks: nothing while not picking, outside the whole cells or
    /// while the cut is a problem. A command, never a throw.
    /// </summary>
    /// <param name="x">The point's distance from the sheet's left edge, in its own pixels.</param>
    /// <param name="y">The point's distance from the sheet's top edge, in its own pixels.</param>
    public void PickCellAt(double x, double y)
    {
        if (IsPicking && CellSize is { } cell && WholeCut() is { } cut && CellPicking.CellAt((x, y), cell, cut) is { } picked)
        {
            Picks = [.. Picks, picked];
        }
    }

    /// <summary>
    /// Adds every cell of a row, left to right, for a click on its number: nothing while not picking, while the cut is
    /// a problem or for a row past it.
    /// </summary>
    /// <param name="row">The row from 0.</param>
    public void PickRow(int row)
    {
        if (IsPicking && WholeCut() is { } cut && row >= 0 && row < cut.Rows)
        {
            Picks = [.. Picks, .. Enumerable.Range(0, cut.Columns).Select(column => new SpriteCell(column, row))];
        }
    }

    /// <exception cref="InvalidOperationException">There is a <see cref="Problem"/>; OK is greyed then.</exception>
    public CellStrip ToStrip() =>
        Problem is null && CellSize is { } cell && FrameMilliseconds is { } frameTime
            ? new CellStrip(StripFileName(), cell, Picks, frameTime)
            : throw new InvalidOperationException(Problem);

    // Columns and rows below 1 are the same message as text that isn't a number: both are a box to retype.
    private static int? TypedCount(string text) => TypedNumber.Parse(text) is { } count and >= 1 ? count : null;

    private static string? SideProblem(int length, string text, string cellsName) => TypedCount(text) is { } cells
        ? GridCut.Plain.ProblemAlong(length, cells, cellsName)
        : $"The {cellsName} must be a whole number from 1.";

    partial void OnColumnsTextChanged(string value) => KeepPicksWithinTheCut();

    partial void OnRowsTextChanged(string value) => KeepPicksWithinTheCut();

    partial void OnPicksChanged(IReadOnlyList<SpriteCell> value) => PicksChanged?.Invoke(this, EventArgs.Empty);

    [RelayCommand(CanExecute = nameof(HasPicks))]
    private void RemoveLast() => Picks = [.. Picks.Take(Picks.Count - 1)];

    [RelayCommand(CanExecute = nameof(HasPicks))]
    private void Clear() => Picks = [];

    private bool HasPicks() => Picks.Count > 0;

    private string? CutProblem((int Width, int Height) size) =>
        SideProblem(size.Width, ColumnsText, "columns") ?? SideProblem(size.Height, RowsText, "rows");

    private (int Columns, int Rows)? WholeCut() =>
        _sheetSize is { } size && CutProblem(size) is null && TypedCount(ColumnsText) is { } columns
            && TypedCount(RowsText) is { } rows
            ? (columns, rows)
            : null;

    // A pick past a smaller cut would name a cell the sheet no longer has, so the picks start over; while a box isn't a
    // count yet (being retyped) they are kept, and the cut's problem greys OK.
    private void KeepPicksWithinTheCut()
    {
        if (TypedCount(ColumnsText) is { } columns && TypedCount(RowsText) is { } rows
            && Picks.Any(pick => pick.Column >= columns || pick.Row >= rows))
        {
            Picks = [];
            return;
        }

        PicksChanged?.Invoke(this, EventArgs.Empty);
    }

    private string? NameProblem()
    {
        var name = StripNameText.Trim();
        if (name.Length == 0 || name.Equals(StripExtension, StringComparison.OrdinalIgnoreCase))
        {
            return "Name the strip.";
        }

        return name.IndexOfAny(_pathCharacters) is var at and >= 0 ? $"The strip's name can't hold '{name[at]}'." : null;
    }

    private string StripFileName()
    {
        var name = StripNameText.Trim();
        return name.EndsWith(StripExtension, StringComparison.OrdinalIgnoreCase) ? name : name + StripExtension;
    }
}
