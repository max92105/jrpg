using Jrpg.Core.Content;
using Jrpg.Core.Items;
using Jrpg.Data.Crafting;
using Jrpg.Data.Items;
using Jrpg.Data.Loot;
using Jrpg.Data.Trade;

namespace Jrpg.Data.Content;

/// <summary>
/// What the loader would refuse in the items the Items page edits (decision 863), everything that reads
/// <c>items.json</c> included.
/// </summary>
public static partial class CatalogProblems
{
    /// <summary>
    /// The first problem in the items (decision 863): an item's own rule, then the Forge's materials in
    /// <c>rules.json</c>, the shops' prices, the drop tables' stacks and the new game's starting items, each checked
    /// against these items, since the page changes what they name; null when the loader would take them.
    /// </summary>
    /// <param name="items">The items as edited.</param>
    /// <param name="content">
    /// The affix pool the uniques name, and the rules, shops, drop tables and new game that name the items.
    /// </param>
    public static string? OfItems(IReadOnlyList<ItemDefinition> items, ContentCatalog content) =>
        FirstProblem(() => CheckItems(items, content));

    /// <summary>
    /// Every check the loader makes that reads <c>items.json</c> (decision 863), in its order: the items' own rules,
    /// then the Forge's materials in <c>rules.json</c>, the shops' prices, the drop tables' stacks and the whole new
    /// game's own check (its leaders, starting items, start and gold), each against these items; Save all checks the
    /// new game's rules only here (decision 1018).
    /// </summary>
    /// <param name="items">The items as they will be written.</param>
    /// <param name="content">
    /// The affix pool the uniques name, and the rules, shops, drop tables and new game that name the items.
    /// </param>
    /// <exception cref="ContentValidationException">The first problem, naming the file.</exception>
    internal static void CheckItems(IReadOnlyList<ItemDefinition> items, ContentCatalog content)
    {
        ItemCatalogValidator.Validate(ContentCatalogLoader.ItemsFile, items, content.Affixes);
        var byId = items.ToDictionary(item => item.Id, StringComparer.Ordinal);
        CraftingRulesValidator.Validate(ContentCatalogLoader.RulesFile, content.Rules.Crafting, byId);
        // By id, as the files are written, so the first problem named is the one the loader would name.
        ShopCatalogValidator.Validate(
            ContentCatalogLoader.ShopsFile, [.. content.Shops.Values.OrderBy(shop => shop.Id, StringComparer.Ordinal)], byId);
        DropTableCatalogValidator.Validate(
            ContentCatalogLoader.DropTablesFile,
            [.. content.DropTables.Values.OrderBy(table => table.Id, StringComparer.Ordinal)],
            byId);
        var newGameReferences = new NewGameReferences(content.PartyMembers.Keys.ToHashSet(), byId, content.Maps);
        NewGameValidator.Validate(ContentCatalogLoader.NewGameFile, content.NewGame, newGameReferences);
    }
}
