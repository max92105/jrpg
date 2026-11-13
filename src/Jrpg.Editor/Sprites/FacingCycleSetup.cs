using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Maps;
using Jrpg.Core.Sprites;

namespace Jrpg.Editor.Sprites;

/// <summary>
/// One loop of a sprite sheet as typed in the setup (ADR 0043): the row each facing walks on, from 0 (the view counts
/// them from 1, decision 568), the walk's frame count and the time each frame shows.
/// </summary>
internal sealed partial class FacingCycleSetup : ObservableObject
{
    [ObservableProperty]
    public partial int DownRow { get; set; }

    [ObservableProperty]
    public partial int UpRow { get; set; }

    [ObservableProperty]
    public partial int LeftRow { get; set; }

    [ObservableProperty]
    public partial int RightRow { get; set; }

    /// <summary>The frames walked through from column 0, at least <see cref="TileAnimation.MinFrames"/>.</summary>
    [ObservableProperty]
    public partial int Frames { get; set; }

    [ObservableProperty]
    public partial int FrameMilliseconds { get; set; }

    /// <summary>Takes a record's loop as the typed values.</summary>
    public void CopyFrom(FacingCycle cycle)
    {
        DownRow = cycle.Rows.Down;
        UpRow = cycle.Rows.Up;
        LeftRow = cycle.Rows.Left;
        RightRow = cycle.Rows.Right;
        Frames = cycle.Frames.FrameCount;
        FrameMilliseconds = cycle.Frames.FrameMilliseconds;
    }

    /// <summary>
    /// Why the loop doesn't fit a sheet cut in this many columns and rows, or null: a facing's row past the rows, or
    /// more frames than columns. Data's own rules (a count below its floor) are its message's, through the catalog.
    /// </summary>
    public string? ProblemWithin(int columns, int rows)
    {
        foreach (var (name, row) in new[] { ("down", DownRow), ("up", UpRow), ("left", LeftRow), ("right", RightRow) })
        {
            if (row < 0 || row >= rows)
            {
                return $"The {name} row must be within 1..{rows} (was {row + 1}).";
            }
        }

        return Frames > columns ? $"The walk's {Frames} frames don't fit in {columns} columns." : null;
    }

    public FacingCycle ToCycle() => new()
    {
        Rows = new FacingRows { Down = DownRow, Up = UpRow, Left = LeftRow, Right = RightRow },
        Frames = new TileAnimation { FrameCount = Frames, FrameMilliseconds = FrameMilliseconds },
    };
}
