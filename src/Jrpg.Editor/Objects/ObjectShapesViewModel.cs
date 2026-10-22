using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Editor.Maps;

namespace Jrpg.Editor.Objects;

/// <summary>
/// The object shapes window's rules: the collision outline and the above-player zones being edited for one object
/// definition, in Core's format (0..1 over the object's cells), and the problem that stops them being applied.
/// </summary>
internal sealed partial class ObjectShapesViewModel : ObservableObject
{
    private readonly ObjectDefinition _definition;

    public ObjectShapesViewModel(ObjectDefinition definition)
    {
        _definition = definition;
        CollisionOutline = definition.CollisionPolygon;
        Zones = definition.AbovePlayerZones;
    }

    public string ObjectId => _definition.Id;

    /// <summary>In cells.</summary>
    public int Width => _definition.Width;

    /// <summary>In cells.</summary>
    public int Height => _definition.Height;

    /// <summary>Replaced whole on each change, so the window redraws on one notification.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem), nameof(ActivePoints))]
    public partial IReadOnlyList<Point2D> CollisionOutline { get; private set; }

    /// <summary>The parts of the object drawn over the player (canopies, roofs); replaced whole on each change.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ZoneEntries), nameof(Problem), nameof(ActivePoints))]
    public partial IReadOnlyList<IReadOnlyList<Point2D>> Zones { get; private set; }

    /// <summary>Whether clicks draw the chosen zone rather than the collision outline (legacy's shape choice).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEditingCollision), nameof(ActivePoints))]
    public partial bool IsEditingZones { get; set; }

    /// <summary>The inverse of <see cref="IsEditingZones"/>, for the window's second radio button.</summary>
    public bool IsEditingCollision
    {
        get => !IsEditingZones;
        set => IsEditingZones = !value;
    }

    /// <summary>The zone the tools act on while editing zones; -1 when none is chosen.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ActivePoints))]
    [NotifyCanExecuteChangedFor(nameof(DeleteZoneCommand))]
    public partial int SelectedZoneIndex { get; set; } = -1;

    /// <summary>"Zone 1 (6 points)", one per zone.</summary>
    public IReadOnlyList<string> ZoneEntries => [.. Zones.Select((zone, index) => $"Zone {index + 1} ({zone.Count} points)")];

    /// <summary>The points the tools act on: the collision outline, or the chosen zone (none when no zone is chosen).</summary>
    public IReadOnlyList<Point2D> ActivePoints =>
        !IsEditingZones ? CollisionOutline : SelectedZoneIndex >= 0 ? Zones[SelectedZoneIndex] : [];

    /// <summary>
    /// Why the shapes can't be applied, or null: Core treats 1 or 2 points as no shape, so they're refused here. An empty
    /// zone is no problem: it is dropped on apply.
    /// </summary>
    public string? Problem
    {
        get
        {
            if (CollisionOutline.Count is 1 or 2)
            {
                return $"The collision outline needs at least 3 points (has {CollisionOutline.Count}), or none.";
            }

            var index = Zones.ToList().FindIndex(zone => zone.Count is 1 or 2);
            return index < 0 ? null : $"Zone {index + 1} needs at least 3 points (has {Zones[index].Count}).";
        }
    }

    /// <summary>
    /// Adds a point after the last one of the active shape; editing zones with none chosen starts a new zone (legacy). It
    /// is clamped into the object, as the validator refuses points outside, and rounded to the object's map pixels.
    /// </summary>
    /// <param name="point">In 0..1 over the object's width and height.</param>
    public void AddPoint(Point2D point) =>
        SetActivePoints([.. ActivePoints, new Point2D(OnPixelGrid(point.X, Width), OnPixelGrid(point.Y, Height))]);

    /// <summary>Takes back the active shape's newest point (legacy Backspace); nothing when it has none.</summary>
    public void RemoveLastPoint()
    {
        if (ActivePoints.Count > 0)
        {
            SetActivePoints([.. ActivePoints.Take(ActivePoints.Count - 1)]);
        }
    }

    /// <exception cref="InvalidOperationException">The shapes have a <see cref="Problem"/>; its text is the message.</exception>
    public ObjectDefinition ToDefinition() =>
        Problem is { } problem
            ? throw new InvalidOperationException(problem)
            : _definition with { CollisionPolygon = CollisionOutline, AbovePlayerZones = ZonesWithPoints() };

    // An empty zone draws nothing and the validator refuses it; the list is kept as it is when none is empty.
    private IReadOnlyList<IReadOnlyList<Point2D>> ZonesWithPoints() =>
        Zones.Any(zone => zone.Count == 0) ? [.. Zones.Where(zone => zone.Count > 0)] : Zones;

    // One map pixel is 1 / (cells x CellPixels) of the object's size.
    private static double OnPixelGrid(double fraction, int cells)
    {
        var pixels = cells * MapDrawing.CellPixels;
        return Math.Round(Math.Clamp(fraction, 0, 1) * pixels) / pixels;
    }

    private void SetActivePoints(IReadOnlyList<Point2D> points)
    {
        if (!IsEditingZones)
        {
            CollisionOutline = points;
        }
        else if (SelectedZoneIndex < 0)
        {
            Zones = [.. Zones, points];
            SelectedZoneIndex = Zones.Count - 1;
        }
        else
        {
            Zones = [.. Zones.Select((zone, index) => index == SelectedZoneIndex ? points : zone)];
        }
    }

    /// <summary>Empties the active shape: no outline never blocks the player; an empty zone is dropped on apply.</summary>
    [RelayCommand]
    private void ClearShape()
    {
        if (!IsEditingZones || SelectedZoneIndex >= 0)
        {
            SetActivePoints([]);
        }
    }

    [RelayCommand]
    private void NewZone()
    {
        Zones = [.. Zones, []];
        SelectedZoneIndex = Zones.Count - 1;
    }

    /// <summary>Removes the chosen zone and chooses the one before it, or none.</summary>
    [RelayCommand(CanExecute = nameof(CanDeleteZone))]
    private void DeleteZone()
    {
        var deleted = SelectedZoneIndex;
        SelectedZoneIndex = -1;
        Zones = [.. Zones.Where((_, index) => index != deleted)];
        SelectedZoneIndex = deleted - 1;
    }

    private bool CanDeleteZone() => SelectedZoneIndex >= 0;
}
