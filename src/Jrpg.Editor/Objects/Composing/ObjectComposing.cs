using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Tiles;

namespace Jrpg.Editor.Objects.Composing;

/// <summary>
/// Objects > New object, Edit object, Duplicate object and New object from map selection (decisions 455, 514, 517):
/// starts the composer on the cells chosen in the Tiles panel, the object chosen in the Objects panel (to repaint or
/// to copy) or the canvas a map selection made, and applies it as one undo step.
/// </summary>
internal sealed partial class ObjectComposing : ObservableObject
{
    /// <summary>The empty canvas's size each way when no rectangle is chosen (decision 459). Placeholder.</summary>
    public const int NewCanvasCells = 3;

    private readonly CatalogDraft _catalog;
    private readonly ObjectBrowserViewModel _objects;
    private readonly TileBrowserViewModel _tiles;

    /// <param name="catalog">The objects and tiles.</param>
    /// <param name="objects">Whose chosen object Edit and Duplicate open, and which chooses the object saved.</param>
    /// <param name="tiles">Whose chosen rectangle New starts from; the composer's picker.</param>
    public ObjectComposing(CatalogDraft catalog, ObjectBrowserViewModel objects, TileBrowserViewModel tiles)
    {
        (_catalog, _objects, _tiles) = (catalog, objects, tiles);
        objects.PropertyChanged += OnObjectChoiceChanged;
    }

    /// <summary>
    /// An object with cells is chosen, within the canvas's largest size, for Edit and Duplicate alike; an NPC is a
    /// field sprite, with none to paint.
    /// </summary>
    public bool CanComposeChosenObject =>
        _objects.SelectedObject is { Tiles.Count: > 0, Width: <= ObjectCanvas.MaxCells, Height: <= ObjectCanvas.MaxCells };

    /// <summary>
    /// The composer for a new object, painted with the Tiles panel's chosen rectangle at its size (its top-left
    /// <see cref="ObjectCanvas.MaxCells"/> cells each way at most), or empty at <see cref="NewCanvasCells"/> without
    /// one.
    /// </summary>
    public ObjectComposerViewModel StartNew()
    {
        var canvas = _tiles.SelectedCells is { } cells
            ? ObjectCanvas.Empty(Math.Min(cells.Columns, ObjectCanvas.MaxCells), Math.Min(cells.Rows, ObjectCanvas.MaxCells)).Stamp(0, new GridCell(0, 0), cells)
            : ObjectCanvas.Empty(NewCanvasCells, NewCanvasCells);
        return ObjectComposerViewModel.ForNew(new CanvasPainting(canvas, _tiles), NewNaming());
    }

    /// <summary>The composer for a new object on the canvas a map selection made (decision 514).</summary>
    /// <param name="canvas">The map's cells, every layer on its level.</param>
    /// <param name="mapLayerWarning">How the object will draw differently from the cells; null when it won't.</param>
    public ObjectComposerViewModel StartNewFromMap(ObjectCanvas canvas, string? mapLayerWarning) =>
        ObjectComposerViewModel.ForNewFromMap(new CanvasPainting(canvas, _tiles), NewNaming(), mapLayerWarning);

    /// <summary>
    /// The composer on the chosen object's picture, its id kept; null unless <see cref="CanComposeChosenObject"/>.
    /// </summary>
    public ObjectComposerViewModel? StartEdit()
    {
        if (!CanComposeChosenObject)
        {
            return null;
        }

        var original = _objects.SelectedObject!;
        return ObjectComposerViewModel.ForEdit(PaintingOf(original), original, Categories());
    }

    /// <summary>
    /// The composer on a copy of the chosen object's picture, for a new object with its outline, zones and category
    /// and an id to type (decision 517); null unless <see cref="CanComposeChosenObject"/>.
    /// </summary>
    public ObjectComposerViewModel? StartDuplicate()
    {
        if (!CanComposeChosenObject)
        {
            return null;
        }

        var duplicated = _objects.SelectedObject!;
        var naming = NewNaming();
        naming.Category = duplicated.Category;
        return ObjectComposerViewModel.ForDuplicate(PaintingOf(duplicated), duplicated, naming);
    }

    /// <summary>
    /// Makes the painted cells tiles where they aren't yet (walkable, outside the history, decision 113), then sets the
    /// object as one undo step and chooses it in the Objects panel; an edited object's placements and events name it by
    /// id, so they stay.
    /// </summary>
    /// <returns>False, nothing changed, while the composer has a problem.</returns>
    public bool TryApply(ObjectComposerViewModel composer)
    {
        if (composer.Problem is not null)
        {
            return false;
        }

        var tileIds = _catalog.TileIdsOf([.. composer.Painting.Canvas.Cells.Values]);
        var definition = composer.ToObject(cell => tileIds[cell]);
        _catalog.SetObject(definition);
        // The stored record: a Save that changed nothing stores nothing, and the panel's list holds the original.
        _objects.SelectedObject = _catalog.Current.Objects[definition.Id];
        return true;
    }

    private CanvasPainting PaintingOf(ObjectDefinition definition) => new(ObjectCanvas.Of(definition, _catalog.Current.Tiles), _tiles);

    private ObjectNaming NewNaming() => new([.. _catalog.Current.Objects.Keys], Categories());

    private List<string> Categories() => [.. _catalog.Current.Objects.Values.Select(definition => definition.Category)];

    private void OnObjectChoiceChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ObjectBrowserViewModel.SelectedObject))
        {
            OnPropertyChanged(nameof(CanComposeChosenObject));
        }
    }
}
