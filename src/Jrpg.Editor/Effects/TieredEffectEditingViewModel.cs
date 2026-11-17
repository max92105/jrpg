using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Workspaces;

namespace Jrpg.Editor.Effects;

/// <summary>
/// Editing the draft's tiered effects through the Effects page (ADR 0045): the page starts from the draft as it is and
/// the whole session is applied as one undo step, everything naming an effect following it (decision 975), as
/// <see cref="Skills.SkillEditingViewModel"/> does.
/// </summary>
/// <param name="catalog">The draft the page edits.</param>
internal sealed class TieredEffectEditingViewModel(CatalogDraft catalog) : ICatalogEditing<TieredEffectCatalogViewModel>
{
    /// <summary>The page's state from the draft as it is now, so an effect undone since shows on the page.</summary>
    public TieredEffectCatalogViewModel Start() => new(catalog.Current);

    /// <summary>Gives the draft the page's effects; nothing when they didn't change.</summary>
    public void Apply(TieredEffectCatalogViewModel edited) => catalog.SetTieredEffects(edited.ToEffects());
}
