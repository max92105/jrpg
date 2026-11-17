using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Workspaces;

namespace Jrpg.Editor.Cinematics;

/// <summary>
/// Editing the draft's cinematics through the Cinematics page (ADR 0046): the page starts from the draft as it is and
/// the whole session is applied as one undo step, as <see cref="Loot.DropTableEditingViewModel"/> does.
/// </summary>
/// <param name="catalog">The draft the cinematics are read from and applied to.</param>
/// <param name="readChoices">The pictures, voices and tracks as the folders hold them when the page opens.</param>
internal sealed class CinematicEditingViewModel(CatalogDraft catalog, Func<CinematicMediaChoices> readChoices)
    : ICatalogEditing<CinematicCatalogViewModel>
{
    /// <summary>
    /// The page's state from the draft as it is now, so a cinematic undone or saved since is read right.
    /// </summary>
    public CinematicCatalogViewModel Start() => new(catalog.Current.Cinematics.Values, readChoices());

    /// <summary>Gives the draft the page's cinematics; nothing when they didn't change.</summary>
    public void Apply(CinematicCatalogViewModel edited) => catalog.SetCinematics(edited.ToCinematics());
}
