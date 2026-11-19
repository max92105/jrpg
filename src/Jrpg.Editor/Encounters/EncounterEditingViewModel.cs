using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Workspaces;

namespace Jrpg.Editor.Encounters;

/// <summary>
/// Editing the draft's encounters through the Encounters page (decision 905): the page starts from the draft as it is
/// and the whole session is applied as one undo step, as <see cref="Cinematics.CinematicEditingViewModel"/> does. The
/// page's links to maps go to the maps at once, and the page last started reads them again whenever a map changes.
/// </summary>
internal sealed class EncounterEditingViewModel : ICatalogEditing<EncounterCatalogViewModel>
{
    private readonly CatalogDraft _catalog;
    private readonly Func<EncounterChoices> _readChoices;
    private readonly EncounterMapLinks _maps;
    private EncounterCatalogViewModel? _started;

    /// <param name="catalog">The draft the encounters are read from and applied to.</param>
    /// <param name="readChoices">
    /// The enemies, backgrounds and tracks as the draft and the folders hold them when the page opens.
    /// </param>
    /// <param name="maps">The session's maps, which the page links encounters to.</param>
    public EncounterEditingViewModel(CatalogDraft catalog, Func<EncounterChoices> readChoices, EncounterMapLinks maps)
    {
        _catalog = catalog;
        _readChoices = readChoices;
        _maps = maps;
        maps.MapsChanged += (_, _) => _started?.RefreshLinkedMaps();
    }

    /// <summary>
    /// The page's state from the draft as it is now, its boxes read now, so an image or track dropped into a folder
    /// since the editor started is offered.
    /// </summary>
    public EncounterCatalogViewModel Start() => _started = new(_catalog.Current, _readChoices(), _maps);

    /// <summary>Gives the draft the page's encounters; nothing when they didn't change.</summary>
    public void Apply(EncounterCatalogViewModel edited) => _catalog.SetEncounters(edited.ToEncounters());
}
