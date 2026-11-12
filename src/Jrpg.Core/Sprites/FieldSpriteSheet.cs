using Jrpg.Core.Geometry;

namespace Jrpg.Core.Sprites;

/// <summary>
/// One field sprite image's layout as content (ADR 0043), keyed by the image's file name: the frame grid it is cut in,
/// how big a frame draws and where its feet are, and its walk. The player's and every NPC's sheet is one.
/// </summary>
public sealed record FieldSpriteSheet
{
    /// <summary>The largest whole-number draw scale; a technical bound.</summary>
    public const int MaxScale = 8;

    /// <summary>The PNG's name in the field-sprite art folder, exact case (decision 567).</summary>
    public required string File { get; init; }

    /// <summary>A frame's width in sheet pixels, from 1 (decision 565).</summary>
    public required int FrameWidth { get; init; }

    /// <summary>A frame's height in sheet pixels, from 1 (decision 565).</summary>
    public required int FrameHeight { get; init; }

    /// <summary>A frame draws at frame size times this, 1 to <see cref="MaxScale"/> (decision 562).</summary>
    public required int Scale { get; init; }

    /// <summary>
    /// Sheet pixels between a frame's bottom edge and the feet line that stands on the cell's bottom edge (decision
    /// 563): 0 to the frame height exclusive; 0 by default and left out of the file at 0.
    /// </summary>
    public int FeetPixels { get; init; }

    public required FacingCycle Walk { get; init; }

    /// <summary>The frame shown standing: the walk's first on the facing's row (decision 564).</summary>
    public SpriteCell StandingCell(Facing facing) => new(0, Walk.Rows.RowOf(facing));

    /// <summary>The walk's frame after <paramref name="walked"/> on the facing's row.</summary>
    /// <param name="facing">Picks the row.</param>
    /// <param name="walked">The time walked since last standing; the shells carry it, Core never reads a clock.</param>
    public SpriteCell WalkCellAt(Facing facing, TimeSpan walked) => new(Walk.Frames.FrameAt(walked), Walk.Rows.RowOf(facing));

    /// <summary>A frame's left edge in sheet pixels.</summary>
    public int FrameLeft(SpriteCell cell) => cell.Column * FrameWidth;

    /// <summary>A frame's top edge in sheet pixels.</summary>
    public int FrameTop(SpriteCell cell) => cell.Row * FrameHeight;

    /// <summary>
    /// Where a frame is drawn over the cell it stands on (decision 562): scaled, centered across the cell, its feet
    /// line (decision 563) on the cell's bottom edge; a 48 px frame at scale 1 fills a 48 px cell exactly.
    /// </summary>
    /// <param name="cellPixels">A map cell's side on screen.</param>
    public SpriteArea AreaOver(int cellPixels)
    {
        var width = FrameWidth * Scale;
        var height = FrameHeight * Scale;
        return new SpriteArea((cellPixels - width) / 2, cellPixels - ((FrameHeight - FeetPixels) * Scale), width, height);
    }

    /// <summary>The whole frames the image must hold across: the walk's frames, from column 0.</summary>
    public int ColumnsNeeded => Walk.Frames.FrameCount;

    /// <summary>
    /// The whole frames the image must hold down: through the highest row a facing reads. In long, since a typed row at
    /// the int limit plus one would wrap and pass every size check.
    /// </summary>
    public long RowsNeeded => (long)HighestRow + 1;

    private int HighestRow => Math.Max(Math.Max(Walk.Rows.Down, Walk.Rows.Up), Math.Max(Walk.Rows.Left, Walk.Rows.Right));
}
