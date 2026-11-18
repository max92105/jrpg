using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Workspaces;

namespace Jrpg.Editor.Progression;

/// <summary>
/// Editing the draft's experience table through the Experience page (decision 956): the page starts from the draft as
/// it is and the whole session is applied as one undo step, as <see cref="NewGame.NewGameEditingViewModel"/> does.
/// </summary>
/// <param name="catalog">The draft the table is read from and applied to.</param>
internal sealed class ExperienceEditingViewModel(CatalogDraft catalog) : ICatalogEditing<ExperienceTableViewModel>
{
    public ExperienceTableViewModel Start() => new(catalog.Current);

    /// <summary>Gives the draft the page's table; nothing when its amounts didn't change.</summary>
    public void Apply(ExperienceTableViewModel edited) => catalog.SetExperienceTable(edited.ToTable());
}
