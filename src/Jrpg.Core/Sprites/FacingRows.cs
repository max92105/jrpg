using Jrpg.Core.Geometry;

namespace Jrpg.Core.Sprites;

/// <summary>
/// The sheet row each facing reads, from 0 (ADR 0043); rows may repeat on a sheet with fewer facings drawn.
/// </summary>
public sealed record FacingRows
{
    public required int Down { get; init; }

    public required int Up { get; init; }

    public required int Left { get; init; }

    public required int Right { get; init; }

    /// <exception cref="ArgumentOutOfRangeException">A value outside the enum.</exception>
    public int RowOf(Facing facing) => facing switch
    {
        Facing.Down => Down,
        Facing.Up => Up,
        Facing.Left => Left,
        Facing.Right => Right,
        _ => throw new ArgumentOutOfRangeException(nameof(facing), facing, "Not a facing."),
    };
}
