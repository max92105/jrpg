using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Core.Geometry;
using Jrpg.Core.Sprites;

namespace Jrpg.Editor.Sprites;

/// <summary>
/// One facing of a walk as the Character sprites window types it (ADR 0054): its row, kept and saved whatever it reads
/// (decision 767), and what it reads: the row, a sequence of cells or the mirror of another facing. The cells and the
/// mirror are kept while another source is chosen, so switching back loses nothing.
/// </summary>
/// <param name="facing">The facing this setup is for; fixed.</param>
internal sealed partial class FacingSetup(Facing facing) : ObservableObject
{
    public Facing Facing { get; } = facing;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsOnRow), nameof(IsOnCells), nameof(IsMirror))]
    public partial FacingSource Source { get; set; }

    /// <summary>
    /// The facing's row from 0 (the window counts from 1, decision 568); read only on <see cref="FacingSource.Row"/>.
    /// </summary>
    [ObservableProperty]
    public partial int Row { get; set; }

    /// <summary>The picked cells in order; replaced whole on a change, so a change notifies.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CellsText))]
    [NotifyCanExecuteChangedFor(nameof(RemoveLastCellCommand), nameof(ClearCellsCommand))]
    public partial IReadOnlyList<SpriteCell> Cells { get; set; } = [];

    /// <summary>
    /// The facing shown flipped on <see cref="FacingSource.Mirror"/>; the first other facing by default.
    /// </summary>
    [ObservableProperty]
    public partial Facing MirrorOf { get; set; } = facing == Facing.Down ? Facing.Up : Facing.Down;

    /// <summary>The facings this one can mirror: the three others, in the enum's order.</summary>
    public IReadOnlyList<Facing> MirrorChoices { get; } = [.. Enum.GetValues<Facing>().Where(other => other != facing)];

    public bool IsOnRow => Source == FacingSource.Row;

    public bool IsOnCells => Source == FacingSource.Cells;

    public bool IsMirror => Source == FacingSource.Mirror;

    /// <summary>
    /// "4 cells: (1, 3) (2, 3) (3, 3) (2, 3)": column then row, counted from 1 as the image numbers them.
    /// </summary>
    public string CellsText =>
        $"{Cells.Count} {(Cells.Count == 1 ? "cell" : "cells")}"
        + (Cells.Count == 0 ? "" : ": " + string.Join(' ', Cells.Select(cell => string.Create(
            CultureInfo.InvariantCulture, $"({cell.Column + 1}, {cell.Row + 1})"))));

    /// <summary>
    /// Adds a picked cell at the end of the sequence; the same cell again is a repeat (decision 772).
    /// </summary>
    public void AddCell(SpriteCell cell) => Cells = [.. Cells, cell];

    /// <summary>Takes a record's row and what overrides it.</summary>
    /// <param name="row">The record's row for this facing.</param>
    /// <param name="sequence">The facing's sequence; null when it reads its row.</param>
    public void CopyFrom(int row, FacingSequence? sequence)
    {
        Row = row;
        Cells = sequence?.Cells ?? [];
        if (sequence?.MirrorOf is { } mirrored)
        {
            MirrorOf = mirrored;
        }

        Source = sequence switch
        {
            null => FacingSource.Row,
            { MirrorOf: not null } => FacingSource.Mirror,
            _ => FacingSource.Cells,
        };
    }

    /// <summary>What overrides the row in the record; null on <see cref="FacingSource.Row"/>.</summary>
    public FacingSequence? ToSequence() => Source switch
    {
        FacingSource.Cells => new FacingSequence { Cells = Cells },
        FacingSource.Mirror => new FacingSequence { MirrorOf = MirrorOf },
        _ => null,
    };

    /// <summary>
    /// Why what the facing reads doesn't fit a sheet cut in this many columns and rows, or null (decision 778): its row
    /// past the rows, no cell yet, or a cell past the cut, its place, column and row counted from 1 as the image
    /// numbers them. A mirror's rules are Data's message.
    /// </summary>
    public string? ProblemWithin(int columns, int rows) => Source switch
    {
        FacingSource.Row when Row < 0 || Row >= rows =>
            $"The {Facing.ToString().ToLowerInvariant()} row must be within 1..{rows} (was {Row + 1}).",
        FacingSource.Cells => CellsProblemWithin(columns, rows),
        _ => null,
    };

    [RelayCommand(CanExecute = nameof(HasCells))]
    private void RemoveLastCell() => Cells = [.. Cells.Take(Cells.Count - 1)];

    [RelayCommand(CanExecute = nameof(HasCells))]
    private void ClearCells() => Cells = [];

    private bool HasCells() => Cells.Count > 0;

    private string? CellsProblemWithin(int columns, int rows)
    {
        if (Cells.Count == 0)
        {
            return $"{Facing} has no cells: pick them on the image.";
        }

        for (var index = 0; index < Cells.Count; index++)
        {
            var cell = Cells[index];
            if (cell.Column < 0 || cell.Column >= columns || cell.Row < 0 || cell.Row >= rows)
            {
                var place = $"cell {index + 1} ({cell.Column + 1}, {cell.Row + 1})";
                return $"{Facing}'s {place} is outside the {columns} x {rows} frames.";
            }
        }

        return null;
    }
}
