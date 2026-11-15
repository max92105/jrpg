using Jrpg.Core.Geometry;

namespace Jrpg.Core.Sprites;

/// <summary>
/// The facings whose sequence of cells overrides their row (ADR 0054, decision 762); a facing left null reads its row.
/// </summary>
public sealed record FacingSequences
{
    public FacingSequence? Down { get; init; }

    public FacingSequence? Up { get; init; }

    public FacingSequence? Left { get; init; }

    public FacingSequence? Right { get; init; }

    /// <summary>The facing's sequence; null when it reads its row.</summary>
    /// <exception cref="ArgumentOutOfRangeException">A value outside the enum.</exception>
    public FacingSequence? Of(Facing facing) => facing switch
    {
        Facing.Down => Down,
        Facing.Up => Up,
        Facing.Left => Left,
        Facing.Right => Right,
        _ => throw new ArgumentOutOfRangeException(nameof(facing), facing, "Not a facing."),
    };
}
