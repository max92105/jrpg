using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Workspaces;

namespace Jrpg.Editor.Encounters;

/// <summary>
/// Editing the draft's encounters through the Encounters page (decision 905): the page starts from the draft as it is
/// and the whole session is applied as one undo step, as <see cref="Cinematics.CinematicEditingViewModel"/> does.
/// </summary>
/// <param name="catalog">The draft the encounters are read from and applied to.</param>
/// <param name="readChoices">The enemies, backgrounds and tracks as the draft and the folders hold them when the page
/// opens.</param>
internal sealed class EncounterEditingViewModel(CatalogDraft catalog, Func<EncounterChoices> readChoices)
    : ICatalogEditing<EncounterCatalogViewModel>
{
    /// <summary>
    /// The page's state from the draft as it is now, its boxes read now, so an image or track dropped into a folder
    /// since the editor started is offered.
    /// </summary>
    public EncounterCatalogViewModel Start() => new(catalog.Current, readChoices());

    /// <summary>Gives the draft the page's encounters; nothing when they didn't change.</summary>
    public void Apply(EncounterCatalogViewModel edited) => catalog.SetEncounters(edited.ToEncounters());
}
