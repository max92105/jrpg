using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Core.Geometry;
using Jrpg.Editor.Maps.Settings;
using Jrpg.Editor.Tiles;

namespace Jrpg.Editor.Objects.Composing;

/// <summary>
/// The composer's canvas as the developer paints it: strokes from the Tiles panel's chosen cells or erasing on the
/// chosen level, any from -7 to 7 (decision 490), the typed size, and the strokes' own undo (decision 461), which the
/// main editor's history never sees.
/// </summary>
internal sealed partial class CanvasPainting : ObservableObject
{
    private readonly Stack<ObjectCanvas> _undoable = new();
    private readonly Stack<ObjectCanvas> _redoable = new();
    private ObjectCanvas? _beforeStroke;
    private bool _isShowingCanvasSize;

    /// <param name="start">The canvas the composer opens on.</param>
    /// <param name="picker">
    /// The Tiles panel's sheet view model, whose chosen cells a stroke stamps (decision 457).
    /// </param>
    public CanvasPainting(ObjectCanvas start, TileBrowserViewModel picker)
    {
        Picker = picker;
        Canvas = start;
        ShowCanvasSize();
    }

    [ObservableProperty]
    public partial ObjectCanvas Canvas { get; private set; }

    public TileBrowserViewModel Picker { get; }

    /// <summary>
    /// The level strokes paint and erase, one of <see cref="LevelChoices"/>'s; the base when the composer opens.
    /// </summary>
    [ObservableProperty]
    public partial int Level { get; set; }

    /// <summary>Every level, the top one first; made once, so a box bound to it never loses its choice.</summary>
    public IReadOnlyList<LevelChoice> LevelChoices { get; } = LevelChoice.TopDown();

    /// <summary>Whether a stroke erases; the right button erases whatever this says (decision 458).</summary>
    [ObservableProperty]
    public partial bool IsErasing { get; set; }

    [ObservableProperty]
    public partial string ColumnsText { get; set; } = "";

    [ObservableProperty]
    public partial string RowsText { get; set; } = "";

    /// <summary>Why the typed size isn't applied; null while it is.</summary>
    [ObservableProperty]
    public partial string? SizeProblem { get; private set; }

    public bool CanUndo => _undoable.Count > 0;

    public bool CanRedo => _redoable.Count > 0;

    /// <summary>Starts a stroke: what it paints or erases until <see cref="EndStroke"/> is one undo step.</summary>
    public void BeginStroke() => _beforeStroke = Canvas;

    /// <summary>
    /// Stamps the picker's chosen cells with their top-left here, or erases the cell while <see cref="IsErasing"/>;
    /// nothing happens when no cell is chosen or the chosen cells' sheet has no image (decision 442).
    /// </summary>
    public void PaintAt(GridCell at)
    {
        if (IsErasing)
        {
            EraseAt(at);
        }
        else if (Picker.MarkedCells is { } cells && Picker.SheetChoices.Any(choice => choice.Sheet?.Id == cells.SheetId && !choice.IsImageMissing))
        {
            Canvas = Canvas.Stamp(Level, at, cells);
        }
    }

    public void EraseAt(GridCell at) => Canvas = Canvas.Erase(Level, at);

    /// <summary>Ends the stroke, recorded as one undo step when it changed the canvas.</summary>
    public void EndStroke()
    {
        if (_beforeStroke is { } before && !IsSamePicture(before, Canvas))
        {
            Record(before);
        }

        _beforeStroke = null;
    }

    [RelayCommand(CanExecute = nameof(CanUndo))]
    private void Undo()
    {
        _redoable.Push(Canvas);
        Canvas = _undoable.Pop();
        ShowRestoredCanvas();
    }

    [RelayCommand(CanExecute = nameof(CanRedo))]
    private void Redo()
    {
        _undoable.Push(Canvas);
        Canvas = _redoable.Pop();
        ShowRestoredCanvas();
    }

    // Runs for the start canvas too: the choices are made before the constructor's body sets it.
    partial void OnCanvasChanged(ObjectCanvas value)
    {
        foreach (var choice in LevelChoices)
        {
            choice.IsPainted = value.HasCellsOn(choice.Level);
        }
    }

    partial void OnColumnsTextChanged(string value) => ApplyTypedSize();

    partial void OnRowsTextChanged(string value) => ApplyTypedSize();

    private void ApplyTypedSize()
    {
        if (_isShowingCanvasSize)
        {
            return;
        }

        var columns = TypedCounts.Parse(ColumnsText, ObjectCanvas.MaxCells);
        var rows = TypedCounts.Parse(RowsText, ObjectCanvas.MaxCells);
        SizeProblem = (columns, rows) switch
        {
            (null, _) => $"Columns must be a whole number from 1 to {ObjectCanvas.MaxCells}.",
            (_, null) => $"Rows must be a whole number from 1 to {ObjectCanvas.MaxCells}.",
            _ => null,
        };
        if (columns is { } width && rows is { } height && (width, height) != (Canvas.Width, Canvas.Height))
        {
            var before = Canvas;
            Canvas = Canvas.Resize(width, height);
            Record(before);
        }
    }

    private void Record(ObjectCanvas before)
    {
        _undoable.Push(before);
        _redoable.Clear();
        NotifyHistoryChanged();
    }

    private void ShowRestoredCanvas()
    {
        ShowCanvasSize();
        NotifyHistoryChanged();
    }

    private void NotifyHistoryChanged()
    {
        UndoCommand.NotifyCanExecuteChanged();
        RedoCommand.NotifyCanExecuteChanged();
    }

    // A canvas's edits always make a new one, even erasing an empty cell, which mustn't be an undo step.
    private static bool IsSamePicture(ObjectCanvas before, ObjectCanvas after) =>
        (before.Width, before.Height) == (after.Width, after.Height)
        && before.Cells.Count == after.Cells.Count
        && before.Cells.All(pair => after.Cells.TryGetValue(pair.Key, out var cell) && cell == pair.Value);

    // The boxes follow an undone resize without resizing again halfway, when one box holds the new size and the other
    // the old.
    private void ShowCanvasSize()
    {
        _isShowingCanvasSize = true;
        ColumnsText = Canvas.Width.ToString(CultureInfo.InvariantCulture);
        RowsText = Canvas.Height.ToString(CultureInfo.InvariantCulture);
        SizeProblem = null;
        _isShowingCanvasSize = false;
    }
}
