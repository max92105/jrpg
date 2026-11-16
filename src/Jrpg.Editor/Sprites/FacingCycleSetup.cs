using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Core.Sprites;

namespace Jrpg.Editor.Sprites;

/// <summary>
/// A sheet's walk as typed in the setup (ADRs 0043, 0054): each facing's row and what it reads (its row, cells or a
/// mirror, <see cref="FacingSetup"/>), the walk's frame count and the time each frame shows. A change in a facing is
/// raised under that facing's property name, so the setup's relay sees every typed change.
/// </summary>
internal sealed partial class FacingCycleSetup : ObservableObject
{
    public FacingCycleSetup()
    {
        foreach (var facing in AllFacings)
        {
            facing.PropertyChanged += OnFacingChanged;
        }
    }

    public FacingSetup Down { get; } = new(Facing.Down);

    public FacingSetup Up { get; } = new(Facing.Up);

    public FacingSetup Left { get; } = new(Facing.Left);

    public FacingSetup Right { get; } = new(Facing.Right);

    /// <summary>The frames a row walks through from column 0, at least <see cref="TileAnimation.MinFrames"/>.</summary>
    [ObservableProperty]
    public partial int Frames { get; set; }

    [ObservableProperty]
    public partial int FrameMilliseconds { get; set; }

    /// <summary>
    /// The facing a click on the image adds a cell to, at most one (decision 772): a facing switched to Cells picks,
    /// its Pick toggles it, leaving Cells clears it.
    /// </summary>
    [ObservableProperty]
    public partial FacingSetup? PickingFacing { get; private set; }

    /// <summary>Whether some facing reads its row, so the frame count is read (decision 767).</summary>
    public bool IsAnyFacingOnRow => AllFacings.Any(facing => facing.IsOnRow);

    // In the order the problem line checks them.
    private FacingSetup[] AllFacings => [Down, Up, Left, Right];

    /// <summary>Takes a record's loop as the typed values, each facing's sequence included.</summary>
    public void CopyFrom(FacingCycle cycle)
    {
        foreach (var facing in AllFacings)
        {
            facing.CopyFrom(cycle.Rows.RowOf(facing.Facing), cycle.Sequences?.Of(facing.Facing));
        }

        Frames = cycle.Frames.FrameCount;
        FrameMilliseconds = cycle.Frames.FrameMilliseconds;
        PickingFacing = null;
    }

    /// <summary>
    /// Why the loop doesn't fit a sheet cut in this many columns and rows, or null: a facing's problem (its row past
    /// the rows, its cells), then more frames than columns, only while some facing reads its row (decision 767). Data's
    /// own rules (a count below its floor, a mirror's) are its message's, through the catalog.
    /// </summary>
    public string? ProblemWithin(int columns, int rows) =>
        AllFacings.Select(facing => facing.ProblemWithin(columns, rows)).FirstOrDefault(problem => problem is not null)
        ?? (Frames > columns && IsAnyFacingOnRow
            ? $"The walk's {Frames} frames don't fit in {columns} columns."
            : null);

    /// <summary>The record's loop: every facing's row, and sequences only when some facing overrides its row.</summary>
    public FacingCycle ToCycle() => new()
    {
        Rows = new FacingRows { Down = Down.Row, Up = Up.Row, Left = Left.Row, Right = Right.Row },
        Frames = new TileAnimation { FrameCount = Frames, FrameMilliseconds = FrameMilliseconds },
        Sequences = AllFacings.All(facing => facing.IsOnRow)
            ? null
            : new FacingSequences
            {
                Down = Down.ToSequence(),
                Up = Up.ToSequence(),
                Left = Left.ToSequence(),
                Right = Right.ToSequence(),
            },
    };

    [RelayCommand(CanExecute = nameof(CanTogglePicking))]
    private void TogglePicking(FacingSetup facing) => PickingFacing = PickingFacing == facing ? null : facing;

    private static bool CanTogglePicking(FacingSetup? facing) => facing?.IsOnCells == true;

    // Only the typed fields go on: a flag or the cells' text would rerun the problem line for nothing (editor rule).
    private void OnFacingChanged(object? sender, PropertyChangedEventArgs e)
    {
        var facing = (FacingSetup)sender!;
        if (e.PropertyName == nameof(FacingSetup.Source))
        {
            OnSourceChanged(facing);
        }

        if (e.PropertyName is nameof(FacingSetup.Source) or nameof(FacingSetup.Row) or nameof(FacingSetup.Cells)
            or nameof(FacingSetup.MirrorOf))
        {
            OnPropertyChanged(NameOf(facing));
        }
    }

    // Decision 771: a facing switched to Cells with none starts from its row's run, so a sheet is edited from where it
    // was; it picks next. One leaving Cells stops picking.
    private void OnSourceChanged(FacingSetup facing)
    {
        if (facing.IsOnCells)
        {
            if (facing.Cells.Count == 0)
            {
                facing.Cells = [.. Enumerable.Range(0, Math.Max(Frames, 0)).Select(column => new SpriteCell(column, facing.Row))];
            }

            PickingFacing = facing;
        }
        else if (PickingFacing == facing)
        {
            PickingFacing = null;
        }

        OnPropertyChanged(nameof(IsAnyFacingOnRow));
        TogglePickingCommand.NotifyCanExecuteChanged();
    }

    private static string NameOf(FacingSetup facing) =>
        facing.Facing switch
        {
            Facing.Down => nameof(Down),
            Facing.Up => nameof(Up),
            Facing.Left => nameof(Left),
            _ => nameof(Right),
        };
}
