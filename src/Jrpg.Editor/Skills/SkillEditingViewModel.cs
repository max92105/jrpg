using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Workspaces;

namespace Jrpg.Editor.Skills;

/// <summary>
/// Editing the draft's skills through the Skills page (ADR 0045): the page starts from the draft as it is and the whole
/// session is applied as one undo step (decision 974), as <see cref="Trade.ShopEditingViewModel"/> does.
/// </summary>
/// <param name="catalog">The draft the page edits.</param>
/// <param name="readChoices">
/// Reads what the page's boxes offer, built by the shell over the art and audio roots.
/// </param>
internal sealed class SkillEditingViewModel(CatalogDraft catalog, Func<SkillChoices> readChoices)
    : ICatalogEditing<SkillCatalogViewModel>
{
    /// <summary>
    /// The page's state from the draft and the folders as they are now, so a skill undone or an image added since shows
    /// on the page.
    /// </summary>
    public SkillCatalogViewModel Start() => new(catalog.Current, readChoices());

    /// <summary>Gives the draft the page's skills; nothing when they didn't change.</summary>
    public void Apply(SkillCatalogViewModel edited) => catalog.SetSkills(edited.ToSkills());
}
