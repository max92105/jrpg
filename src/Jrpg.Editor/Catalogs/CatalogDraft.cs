using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Content;
using Jrpg.Core.Maps;

namespace Jrpg.Editor.Catalogs;

/// <summary>
/// The content the editor works on (ADR 0014): as loaded, with the maps saved since. Every panel and the map drawing read
/// <see cref="Current"/> when they need it, so a change made here reaches them all; only this type changes it.
/// </summary>
internal sealed partial class CatalogDraft : ObservableObject
{
    /// <param name="loaded">The content as the loader validated it.</param>
    public CatalogDraft(ContentCatalog loaded) => Current = loaded;

    /// <summary>An immutable snapshot, replaced whole by each change, as <c>MapViewModel.Map</c> is for maps.</summary>
    [ObservableProperty]
    public partial ContentCatalog Current { get; private set; }

    /// <summary>The saved map replaces its older version, or joins the maps when it is new, so later saves check against it.</summary>
    public void AddSavedMap(MapDefinition map) =>
        Current = Current with { Maps = new Dictionary<string, MapDefinition>(Current.Maps) { [map.Id] = map } };
}
