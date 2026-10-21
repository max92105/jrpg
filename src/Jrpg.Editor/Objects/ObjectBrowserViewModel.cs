using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Maps;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Objects;

/// <summary>Choosing the object to place: every object definition, listed by category as in the legacy browser.</summary>
internal sealed partial class ObjectBrowserViewModel(CatalogDraft catalog) : ObservableObject
{
    /// <summary>By category, then id; the view groups them under their category.</summary>
    public IReadOnlyList<ObjectDefinition> Objects { get; } =
        [.. catalog.Current.Objects.Values.OrderBy(definition => definition.Category, StringComparer.Ordinal).ThenBy(definition => definition.Id, StringComparer.Ordinal)];

    /// <summary>The object a click places; null until one is chosen.</summary>
    [ObservableProperty]
    public partial ObjectDefinition? SelectedObject { get; set; }
}
