using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Tiles;

namespace Jrpg.Editor.Objects;

/// <summary>
/// Objects > Replace cells of the chosen object (decision 449): the object chosen in the Objects panel takes the cells
/// chosen in the Tiles panel as its picture and size, as one undo step; cells not tiles yet become tiles first, as New
/// object does. Not for an NPC (a field sprite, no cells), nor cells of a sheet whose image is missing.
/// </summary>
internal sealed partial class ObjectReskinning : ObservableObject
{
    private readonly CatalogDraft _catalog;
    private readonly ObjectBrowserViewModel _objects;
    private readonly TileBrowserViewModel _tiles;

    /// <param name="catalog">The objects and tiles.</param>
    /// <param name="objects">Whose chosen object is re-skinned.</param>
    /// <param name="tiles">Whose chosen cell or rectangle gives the new picture.</param>
    public ObjectReskinning(CatalogDraft catalog, ObjectBrowserViewModel objects, TileBrowserViewModel tiles)
    {
        _catalog = catalog;
        _objects = objects;
        _tiles = tiles;
        objects.PropertyChanged += OnChoiceChanged;
        tiles.PropertyChanged += OnChoiceChanged;
    }

    // The marked cells' own sheet must have its image, whichever sheet the panel shows now: a cell picked off the map
    // stays chosen when another sheet is shown.
    public bool CanReskin =>
        _objects.SelectedObject is { FieldSpriteFile: null }
        && _tiles.MarkedCells is { } cells
        && _tiles.SheetChoices.Any(choice => choice.Sheet?.Id == cells.SheetId && !choice.IsImageMissing);

    [RelayCommand(CanExecute = nameof(CanReskin))]
    private void Reskin()
    {
        var cells = _tiles.MarkedCells!.Value;
        var tileIds = _catalog.TileIdsOf(cells);
        _catalog.SetObject(ObjectReskin.Reskin(_objects.SelectedObject!, cells, cell => tileIds[cell]));
    }

    private void OnChoiceChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(ObjectBrowserViewModel.SelectedObject) or nameof(TileBrowserViewModel.MarkedCells) or nameof(TileBrowserViewModel.SelectedChoice))
        {
            OnPropertyChanged(nameof(CanReskin));
            ReskinCommand.NotifyCanExecuteChanged();
        }
    }
}
