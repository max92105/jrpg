using Jrpg.Core.Content;
using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Core.Stats;
using Jrpg.Data.Content;

namespace Jrpg.Editor.Tests.Items;

/// <summary>
/// The Items page tests' own records, one per category, so no repository number is pinned; each gives every field it
/// can a value of its own, so a box that drops one shows in a record compared whole.
/// </summary>
internal static class ItemFixtures
{
    /// <summary>
    /// A heal used from the menu: every general field away from its default. It has no strip of its own, which Data
    /// allows only on a damaging effect (decision 707), so the list's problem line starts empty.
    /// </summary>
    public static ItemDefinition Tonic { get; } = new()
    {
        Id = "tonic",
        Name = "Tonic",
        Description = "Restores a little HP.",
        IconFile = "Tonic.png",
        MaxStack = 7,
        IsUsableFromMenu = true,
        Effect = new Effect(EffectKind.RestoreHp, 12, Targeting.SingleAlly),
        Price = 30,
    };

    public static ItemDefinition CopperVest { get; } = new()
    {
        Id = "copper_vest",
        Name = "Copper Vest",
        Description = "Light chest armor.",
        IconFile = "Vest.png",
        MaxStack = 99,
        IsUsableFromMenu = false,
        Equipment = new EquipmentStats(EquipmentKind.Chest)
        {
            Bonuses = new StatBonuses(new Dictionary<StatKind, int> { [StatKind.Constitution] = 2 }),
        },
        Price = 80,
    };

    public static ItemDefinition CopperAxe { get; } = new()
    {
        Id = "copper_axe",
        Name = "Copper Axe",
        Description = "A heavy axe.",
        IconFile = "Axe.png",
        MaxStack = 99,
        IsUsableFromMenu = false,
        Equipment = new EquipmentStats(EquipmentKind.Weapon)
        {
            Damage = 9,
            DamageType = DamageType.Slashing,
            Bonuses = new StatBonuses(new Dictionary<StatKind, int> { [StatKind.Power] = 1 }),
        },
        Price = 100,
    };

    public static ItemDefinition Ore { get; } = new()
    {
        Id = "ore",
        Name = "Ore",
        Description = "The Forge melts it.",
        IconFile = "Ore.png",
        MaxStack = 50,
        IsUsableFromMenu = false,
        IsMaterial = true,
        Price = 4,
    };

    /// <summary>The copper axe made unique: its own name and price, and two of the test world's pool affixes.</summary>
    public static ItemDefinition WarlordAxe { get; } = CopperAxe with
    {
        Id = "warlord_axe",
        Name = "Warlord's Axe",
        Description = "Its affixes never change.",
        Price = 300,
        IsUnique = true,
        FixedAffixes = [new RolledAffix("power", StatKind.Power, 6), new RolledAffix("crit_chance", StatKind.CritChance, 3)],
    };

    /// <summary>
    /// The test world with the five records above beside its own items, kept since its shops, drop tables, Forge and
    /// new game name them. The five ids must stay out of the test world: a clash makes every Items test fail to start.
    /// </summary>
    public static ContentCatalog Catalog { get; } =
        CatalogOf(ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world")));

    private static ContentCatalog CatalogOf(ContentCatalog world) =>
        world with
        {
            Items = world.Items.Values
                .Concat([Tonic, CopperVest, CopperAxe, Ore, WarlordAxe])
                .ToDictionary(item => item.Id, StringComparer.Ordinal),
        };
}
