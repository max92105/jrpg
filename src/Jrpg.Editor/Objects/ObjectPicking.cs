using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Core.Geometry;
using Jrpg.Editor.Maps;
using Jrpg.Editor.Maps.Editing;

namespace Jrpg.Editor.Objects;

/// <summary>
/// Ctrl + click's pick (decision 523): the object drawn on top at the point or, clicked again on the same spot with
/// that object selected, the next one under it, wrapping; the Placed object panel's note on what the pick found and its
/// Next under button, which picks at the last point again.
/// </summary>
internal sealed partial class ObjectPicking : ObservableObject
{
    private readonly ObjectPlacingViewModel _placing;
    private readonly LayerChoiceViewModel _layer;
    private Point2D _lastPoint;
    private int _lastCount;
    private string? _pickedUid;

    /// <param name="placing">Whose selection the pick sets, and which finds the objects at a point.</param>
    /// <param name="layer">Whose lock decides what a pick reaches (decision 525): a change there ends the note.</param>
    public ObjectPicking(ObjectPlacingViewModel placing, LayerChoiceViewModel layer)
    {
        (_placing, _layer) = (placing, layer);
        placing.PropertyChanged += OnPlacingChanged;
        layer.PropertyChanged += (_, _) => Forget();
    }

    /// <summary>
    /// "bench_2 selected: 2 of 3 objects here"; "Locked: only layer z 1 takes clicks" when the lock left the pick with
    /// nothing (decision 525); empty while the pick found one object, or none at all.
    /// </summary>
    [ObservableProperty]
    public partial string Note { get; private set; } = "";

    /// <summary>
    /// Selects the object the hits at the point give next (<see cref="PlacedObjectHits.NextUnder"/>); when nothing is
    /// there, the selection is cleared.
    /// </summary>
    /// <param name="point">In map cells.</param>
    public void PickAt(Point2D point)
    {
        var hits = _placing.ObjectsAt(point);
        var selected = _placing.SelectedObjectUids is [var only] ? only : null;
        var picked = PlacedObjectHits.NextUnder(hits, selected);
        (_lastPoint, _lastCount, _pickedUid) = (point, hits.Count, picked?.Uid);
        _placing.Select(picked is null ? [] : [picked.Uid]);
        Note = picked switch
        {
            null => _placing.IsLockHidingObjectsAt(point) ? $"Locked: only layer z {_layer.Z} takes clicks" : "",
            _ when hits.Count > 1 => $"{picked.Uid} selected: {hits.ToList().IndexOf(picked) + 1} of {hits.Count} objects here",
            _ => "",
        };
        SelectNextUnderCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanSelectNextUnder))]
    private void SelectNextUnder() => PickAt(_lastPoint);

    private bool CanSelectNextUnder() => _lastCount > 1;

    /// <summary>
    /// Ends the note and Next under: the map changed under the last pick (the picked object moved or removed, an
    /// undo), so what lies at its point isn't what it found.
    /// </summary>
    public void Forget()
    {
        (Note, _lastCount, _pickedUid) = ("", 0, null);
        SelectNextUnderCommand.NotifyCanExecuteChanged();
    }

    // The note is about the picked object: any other selection, or another map, ends it.
    private void OnPlacingChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ObjectPlacingViewModel.SelectedObjectUids) && !(_placing.SelectedObjectUids is [var only] && only == _pickedUid))
        {
            Forget();
        }
    }
}
