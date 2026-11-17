using Jrpg.Core.Content;
using Jrpg.Core.Items;
using Jrpg.Core.Loot;
using Jrpg.Core.Stats;
using Jrpg.Data.Content;
using Jrpg.Editor.Loot;

namespace Jrpg.Editor.Tests.Loot;

/// <summary>
/// The Affixes page tests' own affixes, so no repository number is pinned: each gives every field a value away from a
/// new affix's, so a box that drops one shows in a record compared whole.
/// </summary>
internal static class AffixFixtures
{
    // Loaded at the first read, so a load failure or an id clash fails only the tests reading the catalog.
    private static readonly Lazy<ContentCatalog> _catalog =
        new(() => CatalogOf(ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"))));

    /// <summary>A crit damage affix on weapons and rings, which <see cref="FierceBlade"/> names.</summary>
    public static AffixDefinition Fierce { get; } = new()
    {
        Id = "fierce",
        Stat = StatKind.CritDamage,
        Min = 3,
        Max = 7,
        Kinds = [EquipmentKind.Weapon, EquipmentKind.Ring],
        Weight = 4,
        GoldValue = 35,
    };

    public static AffixDefinition Fireproof { get; } = new()
    {
        Id = "fireproof",
        Stat = StatKind.FireResistance,
        Min = 5,
        Max = 12,
        Kinds = [EquipmentKind.Chest],
        Weight = 6,
        GoldValue = 15,
    };

    /// <summary>A unique weapon with <see cref="Fierce"/> fixed at a value inside its range.</summary>
    public static ItemDefinition FierceBlade { get; } = new()
    {
        Id = "fierce_blade",
        Name = "Fierce Blade",
        Description = "Its affix never changes.",
        IconFile = "Sword.png",
        MaxStack = 99,
        IsUsableFromMenu = false,
        Equipment = new EquipmentStats(EquipmentKind.Weapon) { Damage = 12, DamageType = DamageType.Slashing },
        Price = 250,
        IsUnique = true,
        FixedAffixes = [new RolledAffix(Fierce.Id, Fierce.Stat, 5)],
    };

    /// <summary>
    /// The test world with the two affixes in its pool and <see cref="FierceBlade"/> beside its items, kept since its
    /// own uniques name its pool. The three ids must stay out of the test world: a clash fails every test reading it.
    /// </summary>
    public static ContentCatalog Catalog => _catalog.Value;

    /// <summary>The affix's box for <paramref name="kind"/>, picked by its kind rather than its place.</summary>
    public static EquipmentKindToggle Toggle(AffixViewModel affix, EquipmentKind kind) =>
        affix.Kinds.Single(toggle => toggle.Kind == kind);

    private static ContentCatalog CatalogOf(ContentCatalog world) =>
        world with
        {
            Affixes = world.Affixes.Values
                .Concat([Fierce, Fireproof])
                .ToDictionary(affix => affix.Id, StringComparer.Ordinal),
            Items = world.Items.Values
                .Concat([FierceBlade])
                .ToDictionary(item => item.Id, StringComparer.Ordinal),
        };
}
