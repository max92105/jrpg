using Jrpg.Core.Items;
using Jrpg.Core.Loot;
using Jrpg.Core.Stats;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Randomness;

namespace Jrpg.Core.Tests.Loot;

// The Forge's affix draws share the drops' (decisions 351, 352): an added affix is never one the piece has, nothing
// when none is left; a reroll draws every affix again among all of its kind's.
public sealed class GearRollCraftingTests
{
    private static readonly ItemDefinition _sword = new ItemDefinitionBuilder().WithId("sword").AsWeapon(5).Build();

    private static readonly AffixDefinition[] _pool =
    [
        new() { Id = "power", Stat = StatKind.Power, Min = 2, Max = 5, Kinds = [EquipmentKind.Weapon], Weight = 10 },
        new() { Id = "crit_chance", Stat = StatKind.CritChance, Min = 1, Max = 3, Kinds = [EquipmentKind.Weapon], Weight = 10 },
        new() { Id = "dodge", Stat = StatKind.Dodge, Min = 1, Max = 3, Kinds = [EquipmentKind.Feet], Weight = 10 },
    ];

    // Power is held and dodge is for feet, so the crit chance is the only pick: only its value is drawn.
    [Fact]
    public void AddAffix_PieceHasOne_NeverRepeatsIt()
    {
        var piece = GearPiece.Rolled(_sword, Rarity.Magic, [new RolledAffix("power", StatKind.Power, 2)]);

        var added = NewRoll(1).AddAffix(piece, Rarity.Rare);

        var expected = GearPiece.Rolled(_sword, Rarity.Rare, [new RolledAffix("power", StatKind.Power, 2), new RolledAffix("crit_chance", StatKind.CritChance, 2)]);
        Assert.Equal(expected, added);
    }

    // The scripted random holds nothing, so a draw would throw.
    [Fact]
    public void AddAffix_PoolExhausted_ReturnsNull()
    {
        var piece = GearPiece.Rolled(_sword, Rarity.Rare, [new RolledAffix("power", StatKind.Power, 2), new RolledAffix("crit_chance", StatKind.CritChance, 1)]);

        Assert.Null(NewRoll().AddAffix(piece, Rarity.Rare));
    }

    // The two weapon affixes: 10 picks the crit chance over Power, 0 its lowest value. The old Power isn't kept.
    [Fact]
    public void Reroll_PinnedDraws_GivesThoseAffixesNotTheOldOnes()
    {
        var piece = GearPiece.Rolled(_sword, Rarity.Magic, [new RolledAffix("power", StatKind.Power, 4)]);

        var rerolled = NewRoll(10, 0).Reroll(piece);

        Assert.Equal([new RolledAffix("crit_chance", StatKind.CritChance, 1)], rerolled.Affixes);
    }

    // Two affixes again, both drawn anew: the crit chance first (10, value 3), then Power, the only one left (value 5).
    [Fact]
    public void Reroll_RarePiece_KeepsRarityAndCount()
    {
        var piece = GearPiece.Rolled(_sword, Rarity.Rare, [new RolledAffix("power", StatKind.Power, 2), new RolledAffix("crit_chance", StatKind.CritChance, 1)]);

        var rerolled = NewRoll(10, 2, 3).Reroll(piece);

        var expected = GearPiece.Rolled(_sword, Rarity.Rare, [new RolledAffix("crit_chance", StatKind.CritChance, 3), new RolledAffix("power", StatKind.Power, 5)]);
        Assert.Equal(expected, rerolled);
    }

    private static GearRoll NewRoll(params int[] draws) =>
        new(_pool, new LootRules(new Dictionary<Rarity, RarityRule>()), new FixedSequenceRandom(draws));
}
