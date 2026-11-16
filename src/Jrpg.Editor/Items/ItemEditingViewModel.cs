using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Items;

/// <summary>
/// Editing the draft's items through the Items page: the page starts from the draft as it is and the whole session is
/// applied as one undo step (decision 877), as <see cref="Skills.SkillEditingViewModel"/> does.
/// </summary>
/// <param name="catalog">The draft the page edits.</param>
internal sealed class ItemEditingViewModel(CatalogDraft catalog)
{
    /// <summary>The page's state from the draft as it is now, so an item undone since shows on the page.</summary>
    public ItemCatalogViewModel Start() => new(catalog.Current);

    /// <summary>Gives the draft the page's items; nothing when they didn't change.</summary>
    public void Apply(ItemCatalogViewModel edited) => catalog.SetItems(edited.ToItems());
}
