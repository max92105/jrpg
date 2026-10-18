using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Editor.Maps.Editing;

namespace Jrpg.Editor.Maps;

/// <summary>
/// The open map: an editable draft with its undo history (ADR 0012). <see cref="Map"/> is the draft's current content,
/// refreshed after every edit, undo or redo, for drawing and saving.
/// </summary>
internal sealed partial class MapViewModel : ObservableObject
{
    private readonly MapDraft _draft;
    private readonly EditHistory _history;

    public MapViewModel(MapDefinition map)
    {
        _draft = MapDraft.From(map);
        _history = new EditHistory(_draft);
        Map = _draft.ToDefinition();
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Layers))]
    public partial MapDefinition Map { get; private set; }

    /// <summary>Bottom first, the order they're drawn in.</summary>
    public IReadOnlyList<TileLayer> Layers => Map.Layout.Layers;

    public bool IsDirty => _history.IsDirty;

    public void Do(IMapEdit edit)
    {
        _history.Do(edit);
        Refresh();
    }

    /// <summary>The draft now matches what was saved.</summary>
    public void MarkSaved()
    {
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

    [RelayCommand(CanExecute = nameof(CanUndo))]
    private void Undo()
    {
        _history.Undo();
        Refresh();
    }

    [RelayCommand(CanExecute = nameof(CanRedo))]
    private void Redo()
    {
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
