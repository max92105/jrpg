using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Trade;

/// <summary>
/// Editing the draft's shops through the Shops page (ADR 0045): the page starts from the draft as it is and the whole
/// session is applied as one undo step (decision 591), as <see cref="Quests.QuestEditingViewModel"/> does.
/// </summary>
internal sealed class ShopEditingViewModel(CatalogDraft catalog)
{
    /// <summary>
    /// The page's state from the draft as it is now, so a shop undone or saved since is read right.
    /// </summary>
    public ShopCatalogViewModel Start() => new(catalog.Current);

    /// <summary>Gives the draft the page's shops; nothing when they didn't change.</summary>
    public void Apply(ShopCatalogViewModel edited) => catalog.SetShops(edited.ToShops());
}
