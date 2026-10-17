namespace Jrpg.Core.Geometry;

/// <summary>A position on a map, in tile units: (1.5, 0) is halfway across the second cell of the top row.</summary>
public readonly record struct Point2D(double X, double Y);
