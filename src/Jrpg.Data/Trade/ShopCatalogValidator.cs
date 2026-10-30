using Jrpg.Core.Items;
using Jrpg.Core.Trade;
using Jrpg.Data.Content;

namespace Jrpg.Data.Trade;

/// <summary>
/// The rules for <c>shops.json</c>, checked against the loaded items (ADR 0031): a name, at least one item, no null
/// entry, every item known and priced (a shop never sells for nothing), and none listed twice.
/// </summary>
internal static class ShopCatalogValidator
{
    public static void Validate(string fileName, IReadOnlyList<ShopDefinition> shops, IReadOnlyDictionary<string, ItemDefinition> items) =>
        new ContentListValidator<ShopDefinition>(fileName, "shop", shop => shop.Id)
            .Validate(shops, shop => FindProblem(shop, items));

    private static string? FindProblem(ShopDefinition shop, IReadOnlyDictionary<string, ItemDefinition> items)
    {
        if (string.IsNullOrWhiteSpace(shop.Name))
        {
            return "name must not be empty.";
        }

        if (shop.ItemIds.Count == 0)
        {
            return "itemIds must list at least one item.";
        }

        for (var index = 0; index < shop.ItemIds.Count; index++)
        {
            if (FindItemProblem(shop.ItemIds[index], items) is { } problem)
            {
                return $"itemIds[{index}] {problem}";
            }
        }

        return shop.ItemIds.GroupBy(id => id).FirstOrDefault(same => same.Count() > 1) is { } doubled
            ? $"itemIds lists '{doubled.Key}' more than once."
            : null;
    }

    // The serializer doesn't apply nullable annotations to list elements, so a stray null gets here.
    private static string? FindItemProblem(string? itemId, IReadOnlyDictionary<string, ItemDefinition> items) =>
        itemId is null ? "is null."
        : !items.TryGetValue(itemId, out var item) ? $"has unknown item id '{itemId}'."
        : item.Price is null ? $"'{itemId}' has no price, so no shop can sell it."
        : null;
}
