using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Workspaces;

namespace Jrpg.Editor.Loot;

/// <summary>
/// Editing the draft's affix pool through the Affixes page: the page starts from the draft as it is and the whole
/// session is applied as one undo step that the uniques follow (decision 884), as
/// <see cref="DropTableEditingViewModel"/> does for the tables.
/// </summary>
internal sealed class AffixEditingViewModel(CatalogDraft catalog) : ICatalogEditing<AffixCatalogViewModel>
{
    /// <summary>The page's state from the draft as it is now, so an affix undone since shows on the page.</summary>
    public AffixCatalogViewModel Start() => new(catalog.Current);

    /// <summary>Gives the draft the page's affixes; nothing when they didn't change.</summary>
    public void Apply(AffixCatalogViewModel edited) => catalog.SetAffixes(edited.ToAffixes());
}
