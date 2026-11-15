using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
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
    }

    /// <summary>
    /// Why the loop doesn't fit a sheet cut in this many columns and rows, or null: a facing's problem (its row past
    /// the rows, its cells), then more frames than columns, only while some facing reads its row (decision 767). Data's
    /// own rules (a count below its floor, a mirror's) are its message's, through the catalog.
    /// </summary>
    public string? ProblemWithin(int columns, int rows) =>
        AllFacings.Select(facing => facing.ProblemWithin(columns, rows)).FirstOrDefault(problem => problem is not null)
        ?? (Frames > columns && AllFacings.Any(facing => facing.Source == FacingSource.Row)
            ? $"The walk's {Frames} frames don't fit in {columns} columns."
            : null);

    /// <summary>The record's loop: every facing's row, and sequences only when some facing overrides its row.</summary>
    public FacingCycle ToCycle() => new()
    {
        Rows = new FacingRows { Down = Down.Row, Up = Up.Row, Left = Left.Row, Right = Right.Row },
        Frames = new TileAnimation { FrameCount = Frames, FrameMilliseconds = FrameMilliseconds },
        Sequences = AllFacings.All(facing => facing.Source == FacingSource.Row)
            ? null
            : new FacingSequences
            {
                Down = Down.ToSequence(),
                Up = Up.ToSequence(),
                Left = Left.ToSequence(),
                Right = Right.ToSequence(),
            },
    };

    private void OnFacingChanged(object? sender, PropertyChangedEventArgs e) =>
        OnPropertyChanged(((FacingSetup)sender!).Facing switch
        {
            Facing.Down => nameof(Down),
            Facing.Up => nameof(Up),
            Facing.Left => nameof(Left),
            _ => nameof(Right),
        });
}
