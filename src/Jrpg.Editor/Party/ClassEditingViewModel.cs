using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Workspaces;

namespace Jrpg.Editor.Party;

/// <summary>
/// Editing the draft's classes through the Classes page (decision 945): the page starts from the draft as it is and the
/// whole session is applied as one undo step, as <see cref="Encounters.EncounterEditingViewModel"/> does.
/// </summary>
/// <param name="catalog">The draft the classes are read from and applied to.</param>
internal sealed class ClassEditingViewModel(CatalogDraft catalog) : ICatalogEditing<ClassCatalogViewModel>
{
    public ClassCatalogViewModel Start() => new(catalog.Current);

    /// <summary>Gives the draft the page's classes; nothing when they didn't change.</summary>
    public void Apply(ClassCatalogViewModel edited) => catalog.SetClasses(edited.ToClasses());
}
