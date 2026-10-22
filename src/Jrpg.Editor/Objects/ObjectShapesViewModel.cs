using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Editor.Maps;

namespace Jrpg.Editor.Objects;

/// <summary>
/// The object shapes window's rules: the collision outline being edited for one object definition, in Core's format
/// (0..1 over the object's cells), and the problem that stops it being applied.
/// </summary>
internal sealed partial class ObjectShapesViewModel : ObservableObject
{
    private readonly ObjectDefinition _definition;

    public ObjectShapesViewModel(ObjectDefinition definition)
    {
        _definition = definition;
        CollisionOutline = definition.CollisionPolygon;
    }

    public string ObjectId => _definition.Id;

    /// <summary>In cells.</summary>
    public int Width => _definition.Width;

    /// <summary>In cells.</summary>
    public int Height => _definition.Height;

    /// <summary>Replaced whole on each change, so the window redraws on one notification.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    public partial IReadOnlyList<Point2D> CollisionOutline { get; private set; }

    /// <summary>Why the outline can't be applied, or null: Core treats 1 or 2 points as no outline, so they're refused here.</summary>
    public string? Problem => CollisionOutline.Count is 1 or 2
        ? $"The collision outline needs at least 3 points (has {CollisionOutline.Count}), or none."
        : null;

    /// <summary>
    /// Adds a point after the last one. It is clamped into the object, as the validator refuses points outside, and rounded
    /// to the object's map pixels, so outlines stay on whole pixels.
    /// </summary>
    /// <param name="point">In 0..1 over the object's width and height.</param>
    public void AddPoint(Point2D point) =>
        CollisionOutline = [.. CollisionOutline, new Point2D(OnPixelGrid(point.X, Width), OnPixelGrid(point.Y, Height))];

    /// <summary>Takes back the newest point (legacy Backspace); nothing when there is none.</summary>
    public void RemoveLastPoint()
    {
        if (CollisionOutline.Count > 0)
        {
            CollisionOutline = [.. CollisionOutline.Take(CollisionOutline.Count - 1)];
        }
    }

    /// <exception cref="InvalidOperationException">The outline has a <see cref="Problem"/>; its text is the message.</exception>
    public ObjectDefinition ToDefinition() =>
        Problem is { } problem ? throw new InvalidOperationException(problem) : _definition with { CollisionPolygon = CollisionOutline };

    // One map pixel is 1 / (cells x CellPixels) of the object's size.
    private static double OnPixelGrid(double fraction, int cells)
    {
        var pixels = cells * MapDrawing.CellPixels;
        return Math.Round(Math.Clamp(fraction, 0, 1) * pixels) / pixels;
    }

    /// <summary>Empties the outline: an object with none never blocks the player.</summary>
    [RelayCommand]
    private void ClearShape() => CollisionOutline = [];
}
