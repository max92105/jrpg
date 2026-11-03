using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Core.Maps;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Objects;

/// <summary>
/// Choosing the object to place: every object definition, listed by category; and taking the chosen one out of the
/// catalog when nothing uses it (decision 443: unused only, never a cascade into maps).
/// </summary>
internal sealed partial class ObjectBrowserViewModel : ObservableObject
{
    private readonly CatalogDraft _catalog;
    private readonly Func<MapLayout?> _openLayout;
    // The objects the list was built from; a catalog change replaces this dictionary only when it changes an object.
    private IReadOnlyDictionary<string, ObjectDefinition> _listedObjects;

    /// <param name="catalog">The object definitions; the list follows its changes (an edited outline, its undo).</param>
    /// <param name="openLayout">
    /// The open map's layout, unsaved changes included, or null; what a removal checks in place of its saved file.
    /// </param>
    public ObjectBrowserViewModel(CatalogDraft catalog, Func<MapLayout?> openLayout)
    {
        _catalog = catalog;
        _openLayout = openLayout;
        _listedObjects = catalog.Current.Objects;
        Objects = Sorted(_listedObjects.Values);
        catalog.PropertyChanged += OnCatalogChanged;
    }

    /// <summary>By category, then id; the view groups them under their category.</summary>
    [ObservableProperty]
    public partial IReadOnlyList<ObjectDefinition> Objects { get; private set; }

    /// <summary>The object a click places; null until one is chosen.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveSelectedCommand))]
    public partial ObjectDefinition? SelectedObject { get; set; }

    /// <summary>
    /// What the last Remove did, or why it refused, naming the first user; cleared when the choice changes.
    /// </summary>
    [ObservableProperty]
    public partial string? RemovalStatus { get; private set; }

    /// <summary>
    /// Takes the chosen object out of the catalog as one undo step when no map places it and no event swaps to it;
    /// otherwise nothing changes and <see cref="RemovalStatus"/> says why.
    /// </summary>
    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void RemoveSelected()
    {
        var objectId = SelectedObject!.Id;
        if (CatalogUsage.UsersOfObject(_catalog.Current, objectId, _openLayout()) is [var user, ..])
        {
            RemovalStatus = $"Can't remove object {objectId}: it is used on {user}.";
            return;
        }

        _catalog.RemoveObject(objectId);
        RemovalStatus = $"Removed object {objectId}.";
    }

    private bool HasSelection() => SelectedObject is not null;

    partial void OnSelectedObjectChanged(ObjectDefinition? value) => RemovalStatus = null;

    private static ObjectDefinition[] Sorted(IEnumerable<ObjectDefinition> objects) =>
        [.. objects.OrderBy(definition => definition.Category, StringComparer.Ordinal).ThenBy(definition => definition.Id, StringComparer.Ordinal)];

    // The chosen object is chosen again by id, so the preview and the ghost show its new record. The removal line is
    // about the list as it was: an undo that brings the object back, or any later change, drops it (Remove writes its
    // line after the change it makes).
    private void OnCatalogChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(CatalogDraft.Current) || ReferenceEquals(_catalog.Current.Objects, _listedObjects))
        {
            return;
        }

        RemovalStatus = null;

        var chosenId = SelectedObject?.Id;
        _listedObjects = _catalog.Current.Objects;
        Objects = Sorted(_listedObjects.Values);
        SelectedObject = chosenId is null ? null : _listedObjects.GetValueOrDefault(chosenId);
    }
}
