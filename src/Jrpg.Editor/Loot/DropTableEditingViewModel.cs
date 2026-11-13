using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Loot;

/// <summary>
/// Editing the draft's drop tables through the Drop tables window (ADR 0045): the window starts from the draft as it is
/// and the whole session is applied as one undo step (decision 591), as <see cref="Trade.ShopEditingViewModel"/> does.
/// </summary>
internal sealed class DropTableEditingViewModel(CatalogDraft catalog)
{
    /// <summary>
    /// The window's state from the draft as it is now, so a table undone or saved since is read right.
    /// </summary>
    public DropTableCatalogViewModel Start() => new(catalog.Current);

    /// <summary>Gives the draft the window's tables; nothing when they didn't change.</summary>
    public void Apply(DropTableCatalogViewModel edited) => catalog.SetDropTables(edited.ToTables());
}
