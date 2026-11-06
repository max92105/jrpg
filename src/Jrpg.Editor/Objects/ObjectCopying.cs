using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Core.Events;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Maps.Editing;

namespace Jrpg.Editor.Objects;

/// <summary>
/// Copy, Cut and Paste of placed objects with their events (decision 512): the copies are the editor's own, kept while
/// it runs, so they paste on any open map; each paste gets fresh uids and event ids (<see cref="PlacedObjectCopies"/>).
/// </summary>
internal sealed partial class ObjectCopying : ObservableObject
{
    private readonly CatalogDraft _catalog;
    private readonly ObjectPlacingViewModel _placing;

    /// <param name="catalog">The objects a paste needs; Paste follows its changes (B1).</param>
    /// <param name="placing">The shown map and its selection.</param>
    public ObjectCopying(CatalogDraft catalog, ObjectPlacingViewModel placing)
    {
        (_catalog, _placing) = (catalog, placing);
        catalog.PropertyChanged += OnCatalogChanged;
        placing.PropertyChanged += OnPlacingChanged;
    }

    /// <summary>The objects copied last, as they were then; empty until a copy.</summary>
    public IReadOnlyList<PlacedObject> Copied { get; private set; } = [];

    /// <summary>
    /// Something copied, a map shown, and every copied object, and every object a copied event swaps to, still in the
    /// catalog: an undo of its composing or its removal greys Paste, a redo brings it back (B1).
    /// </summary>
    public bool CanPaste =>
        Copied.Count > 0 && _placing.OpenMap is not null && ObjectIdsNamed().All(_catalog.Current.Objects.ContainsKey);

    /// <summary>Pastes the copies, their group's bounds centered on a point, and selects them; one undo step.</summary>
    /// <param name="center">In map cells.</param>
    public void PasteCenteredAt(Point2D center) => Paste(center, topLeft => topLeft);

    /// <summary>
    /// As <see cref="PasteCenteredAt"/>, the group's top-left landing on the nearest whole cell (decision 511).
    /// </summary>
    public void PasteOnGridCenteredAt(Point2D center) => Paste(center, GridSnap.TopLeft);

    // The game refuses a map whose object or swap names an unknown object, so a paste must not bring one in.
    private IEnumerable<string> ObjectIdsNamed() =>
        Copied.Select(placed => placed.ObjectId)
            .Concat(Copied.SelectMany(placed => placed.Events).SelectMany(mapEvent => mapEvent.Steps).OfType<SwapObjectStep>().Select(swap => swap.ObjectId));

    private bool HasSelection() => _placing.SelectedObjectUids.Count > 0;

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void Copy()
    {
        Copied = [.. _placing.OpenMap!.Map.Layout.Objects.Where(placed => _placing.IsSelected(placed.Uid))];
        OnPropertyChanged(nameof(CanPaste));
    }

    /// <summary>Copies the selection, then removes it as one undo step.</summary>
    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void Cut()
    {
        Copy();
        _placing.DeleteSelectedObjects();
    }

    private void Paste(Point2D center, Func<Point2D, Point2D> land)
    {
        // A point off the map pastes nothing, as placing: the map is drawn clipped and the copies couldn't be seen.
        if (!CanPaste || _placing.OpenMap is not { } open || !open.Map.Layout.Contains(GridCell.Containing(center)))
        {
            return;
        }

        var corner = PlacedObjectCopies.TopLeftOf(Copied);
        var width = Copied.Max(placed => placed.Position.X + _catalog.Current.Objects[placed.ObjectId].Width) - corner.X;
        var height = Copied.Max(placed => placed.Position.Y + _catalog.Current.Objects[placed.ObjectId].Height) - corner.Y;
        var copies = PlacedObjectCopies.Make(Copied, open.Map.Layout, land(new Point2D(center.X - (width / 2), center.Y - (height / 2))));
        open.Do(new PlaceObjectsEdit(copies));
        _placing.Select([.. copies.Select(copy => copy.Uid)]);
    }

    private void OnCatalogChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CatalogDraft.Current))
        {
            OnPropertyChanged(nameof(CanPaste));
        }
    }

    private void OnPlacingChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(ObjectPlacingViewModel.SelectedObjectUids) or nameof(ObjectPlacingViewModel.OpenMap))
        {
            CopyCommand.NotifyCanExecuteChanged();
            CutCommand.NotifyCanExecuteChanged();
            OnPropertyChanged(nameof(CanPaste));
        }
    }
}
