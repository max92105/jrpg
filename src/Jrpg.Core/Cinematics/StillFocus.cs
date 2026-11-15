using Jrpg.Core.Geometry;

namespace Jrpg.Core.Cinematics;

/// <summary>
/// Where on a still's shown picture its zoom heads (decision 746), as two whole percents of the part of the picture
/// that covers the screen, measured from its top-left (decision 747).
/// </summary>
public sealed record StillFocus
{
    /// <summary>The shown picture's left or top edge (decision 747).</summary>
    public const int MinPercent = 0;

    /// <summary>The shown picture's right or bottom edge (decision 747).</summary>
    public const int MaxPercent = 100;

    public static StillFocus Center { get; } = new() { XPercent = 50, YPercent = 50 };

    public required int XPercent { get; init; }

    public required int YPercent { get; init; }

    /// <summary>The point as shares of the shown picture's width and height, each from 0 to 1.</summary>
    public Point2D Share => new(XPercent / 100.0, YPercent / 100.0);
}
