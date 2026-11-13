namespace Jrpg.Core.Geometry;

/// <summary>What one facing has to do with another.</summary>
public static class Facings
{
    /// <summary>
    /// The way back: what someone looking this way is looked at from (an NPC facing the player, ADR 0044).
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">A value outside the enum.</exception>
    public static Facing Opposite(Facing facing) => facing switch
    {
        Facing.Down => Facing.Up,
        Facing.Up => Facing.Down,
        Facing.Left => Facing.Right,
        Facing.Right => Facing.Left,
        _ => throw new ArgumentOutOfRangeException(nameof(facing), facing, "Not a facing."),
    };
}
