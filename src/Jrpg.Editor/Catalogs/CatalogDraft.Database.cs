using Jrpg.Core.Trade;

namespace Jrpg.Editor.Catalogs;

/// <summary>The catalogs the Database windows edit (ADR 0045): the shops, each window's session one step.</summary>
internal sealed partial class CatalogDraft
{
    /// <summary>
    /// Replaces the shops as one undo step (decision 591); nothing when they are the same by content, items included.
    /// </summary>
    public void SetShops(IReadOnlyList<ShopDefinition> shops)
    {
        var before = Current.Shops;
        var after = shops.ToDictionary(shop => shop.Id, StringComparer.Ordinal);
        if (before.Count == after.Count && after.All(pair => before.TryGetValue(pair.Key, out var shop) && shop == pair.Value))
        {
            return;
        }

        Do(new CatalogEdit(catalog => catalog with { Shops = after }, catalog => catalog with { Shops = before }));
    }

    /// <summary>The shops the next undo would remove, so it can be refused while a map's event opens one.</summary>
    public IReadOnlyList<string> ShopsRemovedByUndo() =>
        _history.NextUndo is { } edit
            ? [.. Current.Shops.Keys.Except(edit.Revert(Current).Shops.Keys).Order(StringComparer.Ordinal)]
            : [];
}
