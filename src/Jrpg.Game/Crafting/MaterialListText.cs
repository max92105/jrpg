using Jrpg.Core.Items;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Crafting;

/// <summary>A quote's materials as the player reads them: "2 Scrap, 1 Essence", in the quote's order.</summary>
internal static class MaterialListText
{
    public static string Of(IReadOnlyList<InventoryEntry> materials, IReadOnlyDictionary<string, ItemDefinition> items, UiMessages messages) =>
        string.Join(
            messages.Text(MessageIds.ForgeMaterialSeparator),
            materials.Select(entry => messages.Format(MessageIds.ForgeMaterialCount, MessageArgument.Number("count", entry.Quantity), new MessageArgument("name", items[entry.ItemId].Name))));
}
