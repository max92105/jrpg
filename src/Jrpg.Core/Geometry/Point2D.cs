namespace Jrpg.Core.Geometry;

/// <summary>
/// A position in two dimensions: tile units on a map, where (1.5, 0) is halfway across the second cell of the top row;
/// shares of a still's shown picture (<see cref="Cinematics.CinematicStill.ZoomOrigin"/>).
/// </summary>
public readonly record struct Point2D(double X, double Y);
