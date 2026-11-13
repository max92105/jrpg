using Jrpg.Core.Items;
using Jrpg.Core.Loot;

namespace Jrpg.Editor.Tests.Loot;

/// <summary>
/// The Drop tables tests' own items and table, so no repository name or entry is pinned: a four-entry "goblin" table
/// over a potion, a sword, scrap (one or two copies) and essence.
/// </summary>
internal static class DropTableFixtures
{
    public static IReadOnlyDictionary<string, ItemDefinition> Items { get; } = new Dictionary<string, ItemDefinition>
    {
        ["potion"] = Item("potion", "Potion"),
        ["bronze_sword"] = Item("bronze_sword", "Bronze Sword"),
        ["scrap"] = Item("scrap", "Scrap"),
        ["essence"] = Item("essence", "Essence"),
    };

    public static DropTable Goblin { get; } = new(
        "goblin",
        [
            new DropEntry("potion", 30),
            new DropEntry("bronze_sword", 6),
            new DropEntry("scrap", 20) { MaxQuantity = 2 },
            new DropEntry("essence", 5),
        ]);

    private static ItemDefinition Item(string id, string name) => new()
    {
        Id = id,
        Name = name,
        Description = name,
        IconFile = "Placeholder.png",
        MaxStack = 9,
        IsUsableFromMenu = false,
    };
}
