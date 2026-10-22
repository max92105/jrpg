using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Maps;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Objects;

/// <summary>Choosing the object to place: every object definition, listed by category as in the legacy browser.</summary>
internal sealed partial class ObjectBrowserViewModel : ObservableObject
{
    private readonly CatalogDraft _catalog;
    // The objects the list was built from; a catalog change replaces this dictionary only when it changes an object.
    private IReadOnlyDictionary<string, ObjectDefinition> _listedObjects;

    /// <param name="catalog">The object definitions; the list follows its changes (an edited outline, its undo).</param>
    public ObjectBrowserViewModel(CatalogDraft catalog)
    {
        _catalog = catalog;
        _listedObjects = catalog.Current.Objects;
        Objects = Sorted(_listedObjects.Values);
        catalog.PropertyChanged += OnCatalogChanged;
    }

    /// <summary>By category, then id; the view groups them under their category.</summary>
    [ObservableProperty]
    public partial IReadOnlyList<ObjectDefinition> Objects { get; private set; }

    /// <summary>The object a click places; null until one is chosen.</summary>
    [ObservableProperty]
    public partial ObjectDefinition? SelectedObject { get; set; }

    private static ObjectDefinition[] Sorted(IEnumerable<ObjectDefinition> objects) =>
        [.. objects.OrderBy(definition => definition.Category, StringComparer.Ordinal).ThenBy(definition => definition.Id, StringComparer.Ordinal)];

    // The chosen object is chosen again by id, so the preview and the ghost show its new record.
    private void OnCatalogChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(CatalogDraft.Current) || ReferenceEquals(_catalog.Current.Objects, _listedObjects))
        {
            return;
        }

        var chosenId = SelectedObject?.Id;
        _listedObjects = _catalog.Current.Objects;
        Objects = Sorted(_listedObjects.Values);
        SelectedObject = chosenId is null ? null : _listedObjects.GetValueOrDefault(chosenId);
    }
}
