using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Workspaces;

namespace Jrpg.Editor.Loot;

/// <summary>
/// Editing the draft's drop tables through the Drop tables page (ADR 0045): the page starts from the draft as it is and
/// the whole session is applied as one undo step (decision 591), as <see cref="Trade.ShopEditingViewModel"/> does.
/// </summary>
internal sealed class DropTableEditingViewModel(CatalogDraft catalog) : ICatalogEditing<DropTableCatalogViewModel>
{
    /// <summary>
    /// The page's state from the draft as it is now, so a table undone or saved since is read right.
    /// </summary>
    public DropTableCatalogViewModel Start() => new(catalog.Current);

    /// <summary>Gives the draft the page's tables; nothing when they didn't change.</summary>
    public void Apply(DropTableCatalogViewModel edited) => catalog.SetDropTables(edited.ToTables());
}
