namespace Jrpg.Core.Exploration;

/// <summary>
/// Where and how hard the player is pushing: a direction whose length is the share of full speed, 0..1.
/// The Game maps keys and sticks to it, so keyboard and controller move the same way.
/// </summary>
public readonly record struct MoveIntent
{
    private MoveIntent(double x, double y)
    {
        X = x;
        Y = y;
    }

    public static MoveIntent None => default;

    /// <summary>Right is positive.</summary>
    public double X { get; }

    /// <summary>Down is positive, as on the map.</summary>
    public double Y { get; }

    public double Magnitude => Math.Sqrt((X * X) + (Y * Y));

    /// <summary>
    /// Builds an intent from raw axes. A vector longer than 1 is scaled down to length 1, so a keyboard diagonal (1, 1)
    /// or a stick pushed into a corner moves at full speed and never √2 faster than straight.
    /// </summary>
    public static MoveIntent FromAxes(double x, double y)
    {
        var length = Math.Sqrt((x * x) + (y * y));
        return length > 1 ? new MoveIntent(x / length, y / length) : new MoveIntent(x, y);
    }
}
