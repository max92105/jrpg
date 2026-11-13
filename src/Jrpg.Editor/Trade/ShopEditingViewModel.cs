using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Trade;

/// <summary>
/// Editing the draft's shops through the Shops window (ADR 0045): the window starts from the draft as it is and the
/// whole session is applied as one undo step (decision 591), as <see cref="Quests.QuestEditingViewModel"/> does.
/// </summary>
internal sealed class ShopEditingViewModel(CatalogDraft catalog)
{
    /// <summary>
    /// The window's state from the draft as it is now, so a shop undone or saved since is read right.
    /// </summary>
    public ShopCatalogViewModel Start() => new(catalog.Current);

    /// <summary>Gives the draft the window's shops; nothing when they didn't change.</summary>
    public void Apply(ShopCatalogViewModel edited) => catalog.SetShops(edited.ToShops());
}
