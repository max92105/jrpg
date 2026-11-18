using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Workspaces;

namespace Jrpg.Editor.Balance;

/// <summary>
/// The balance table's session (decision 992): the page reads the draft as it is, unsaved edits included, and changes
/// nothing in it, so its Apply records no step (the page recipe's step 8).
/// </summary>
/// <param name="catalog">The draft the table reads.</param>
internal sealed class BalanceTableEditingViewModel(CatalogDraft catalog) : ICatalogEditing<BalanceTableViewModel>
{
    public BalanceTableViewModel Start() => new(catalog.Current);

    /// <summary>Nothing: the typed boxes are the page's own and leave the draft as it is.</summary>
    public void Apply(BalanceTableViewModel edited)
    {
    }
}
