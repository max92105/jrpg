using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Editor.Maps.Editing;

namespace Jrpg.Editor.Maps;

/// <summary>
/// The open map: an editable draft with its undo history (ADR 0012). <see cref="Map"/> is the draft's current content,
/// refreshed after every edit, undo or redo, and while a stroke is dragged, for drawing and saving.
/// </summary>
internal sealed partial class MapViewModel : ObservableObject
{
    private readonly MapDraft _draft;
    private readonly EditHistory _history;
    private TileStroke? _stroke;

    public MapViewModel(MapDefinition map)
    {
        _draft = MapDraft.From(map);
        _history = new EditHistory(_draft);
        Map = _draft.ToDefinition();
    }

    /// <summary>A map that isn't in the content yet: it counts as unsaved, even with no edits, until it is saved.</summary>
    public static MapViewModel ForNewMap(MapDefinition map)
    {
        var viewModel = new MapViewModel(map);
        viewModel._history.ForgetSavedState();
        return viewModel;
    }

    /// <summary>An edit or a finished stroke became an undo step.</summary>
    public event EventHandler? StepRecorded;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Layers))]
    public partial MapDefinition Map { get; private set; }

    /// <summary>Bottom first, the order they're drawn in.</summary>
    public IReadOnlyList<TileLayer> Layers => Map.Layout.Layers;

    public bool IsDirty => _history.IsDirty;

    public void Do(IMapEdit edit)
    {
        EndStroke();
        _history.Do(edit);
        Refresh();
        StepRecorded?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Puts one tile, or empty to erase, on many cells as one undo step; cells off the map are skipped.</summary>
    /// <param name="layerZ">The layer changed.</param>
    /// <param name="tileId">The tile put down, or empty.</param>
    /// <param name="cells">Each cell at most once.</param>
    public void PutTiles(int layerZ, int tileId, IEnumerable<GridCell> cells)
    {
        var changes = cells
            .Where(_draft.Contains)
            .Select(cell => new TileChange(cell, _draft.TileIdAt(layerZ, cell), tileId))
            .Where(change => change.Before != change.After)
            .ToList();
        if (changes.Count > 0)
        {
            Do(new SetTilesEdit(layerZ, changes));
        }
    }

    /// <summary>Starts a brush or eraser drag on a cell; each cell it reaches shows at once.</summary>
    public void StartStroke(TileBrush brush, GridCell cell)
    {
        EndStroke();
        _stroke = new TileStroke(_draft, brush);
        ContinueStroke(cell);
    }

    public void ContinueStroke(GridCell cell)
    {
        if (_stroke?.MoveTo(cell) == true)
        {
            Map = _draft.ToDefinition();
        }
    }

    /// <summary>Ends the drag: what it changed becomes one undo step.</summary>
    public void EndStroke()
    {
        var edit = _stroke?.ToEdit();
        _stroke = null;
        if (edit is not null)
        {
            _history.Record(edit);
            Refresh();
            StepRecorded?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>The draft now matches what was saved; a stroke still being dragged was saved with it and ends here.</summary>
    public void MarkSaved()
    {
        EndStroke();
        _history.MarkSaved();
        OnPropertyChanged(nameof(IsDirty));
    }

    /// <summary>The tile seen at a cell: the one on the highest layer that has a tile there; null outside the map or on an empty cell.</summary>
    public TileAtCell? TopTileAt(GridCell cell)
    {
        if (!Map.Layout.Contains(cell))
        {
            return null;
        }

        for (var index = Layers.Count - 1; index >= 0; index--)
        {
            var tileId = Layers[index].TileIdAt(cell);
            if (tileId != TileLayer.EmptyTileId)
            {
                return new TileAtCell(tileId, Layers[index].Z);
            }
        }

        return null;
    }

    // An undo or redo pressed mid-drag first closes the stroke, so the history never misses its changes.
    [RelayCommand(CanExecute = nameof(CanUndo))]
    private void Undo()
    {
        EndStroke();
        _history.Undo();
        Refresh();
    }

    [RelayCommand(CanExecute = nameof(CanRedo))]
    private void Redo()
    {
        EndStroke();
        _history.Redo();
        Refresh();
    }

    private bool CanUndo() => _history.CanUndo;

    private bool CanRedo() => _history.CanRedo;

    private void Refresh()
    {
        Map = _draft.ToDefinition();
        OnPropertyChanged(nameof(IsDirty));
        UndoCommand.NotifyCanExecuteChanged();
        RedoCommand.NotifyCanExecuteChanged();
    }
}
