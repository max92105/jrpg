using Jrpg.Core.Items;
using Jrpg.Core.Stats;
using Jrpg.Core.Tests.Builders;

namespace Jrpg.Core.Tests.Items;

public sealed class GearPieceTests
{
    private static readonly ItemDefinition _sword = new ItemDefinitionBuilder()
        .WithId("bronze_sword")
        .AsWeapon(24)
        .WithDamageType(DamageType.Slashing)
        .WithBonuses(Bonus(StatKind.Power, 2))
        .Build();

    private static readonly ItemDefinition _potion = new ItemDefinitionBuilder().WithId("potion").Build();

    // Crit listed before Power, against the stats' order, so a sort by stat would show.
    private static readonly ItemDefinition _cleaver = _sword with
    {
        Id = "chiefs_cleaver",
        IsUnique = true,
        FixedAffixes = [Affix(StatKind.CritChance, 4), Affix(StatKind.Power, 7)],
    };

    [Fact]
    public void Unique_UniqueItem_IsUniqueWithItsFixedAffixesInOrder()
    {
        var piece = GearPiece.Unique(_cleaver);

        Assert.Equal((_cleaver, Rarity.Unique), (piece.Base, piece.Rarity));
        Assert.Equal([Affix(StatKind.CritChance, 4), Affix(StatKind.Power, 7)], piece.Affixes);
    }

    [Fact]
    public void Unique_ItemNotMarkedUnique_Throws()
    {
        var exception = Assert.Throws<ArgumentException>(() => GearPiece.Unique(_sword));

        Assert.Contains("'bronze_sword'", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Unique_UniqueConsumable_Throws()
    {
        var uniquePotion = _potion with { IsUnique = true };

        Assert.Throws<ArgumentException>(() => GearPiece.Unique(uniquePotion));
    }

    [Fact]
    public void Unrolled_UniqueItem_IsItsUniquePiece()
    {
        var piece = GearPiece.Unrolled(_cleaver);

        Assert.Equal(GearPiece.Unique(_cleaver), piece);
    }

    [Fact]
    public void Unrolled_OrdinaryItem_IsPlain()
    {
        var piece = GearPiece.Unrolled(_sword);

        Assert.Equal(GearPiece.Plain(_sword), piece);
    }

    // The base gives Power 2; the fixed affixes add 7 to it and 4 crit.
    [Fact]
    public void Bonuses_UniquePiece_AddsItsFixedAffixesToTheBase()
    {
        var piece = GearPiece.Unique(_cleaver);

        Assert.Equal((9, 4), (piece.Bonuses.Of(StatKind.Power), piece.Bonuses.Of(StatKind.CritChance)));
    }

    [Fact]
    public void Plain_Consumable_Throws()
    {
        var exception = Assert.Throws<ArgumentException>(() => GearPiece.Plain(_potion));

        Assert.Contains("'potion'", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Rolled_Consumable_Throws()
    {
        Assert.Throws<ArgumentException>(() => GearPiece.Rolled(_potion, Rarity.Magic, [Affix(StatKind.Power, 3)]));
    }

    [Fact]
    public void Plain_Sword_TakesItsDamageAndBonuses()
    {
        var piece = GearPiece.Plain(_sword);

        Assert.Equal((Rarity.Common, 24, DamageType.Slashing), (piece.Rarity, piece.Damage, piece.DamageType));
        Assert.Equal(Bonus(StatKind.Power, 2), piece.Bonuses);
        Assert.Empty(piece.Affixes);
    }

    [Fact]
    public void Bonuses_AffixOnABaseStat_AddsUp()
    {
        var piece = GearPiece.Rolled(_sword, Rarity.Magic, [Affix(StatKind.Power, 3)]);

        Assert.Equal(5, piece.Bonuses.Of(StatKind.Power));
    }

    [Fact]
    public void Bonuses_TwoAffixesOnOneStat_AddUp()
    {
        var piece = GearPiece.Rolled(_sword, Rarity.Rare, [Affix(StatKind.CritChance, 2), Affix(StatKind.CritChance, 3)]);

        Assert.Equal(5, piece.Bonuses.Of(StatKind.CritChance));
        Assert.Equal(2, piece.Bonuses.Of(StatKind.Power));
    }

    [Fact]
    public void Equals_TwoPlainPiecesOfOneItem_IsTrue()
    {
        var first = GearPiece.Plain(_sword);
        var second = GearPiece.Plain(_sword);

        Assert.True(first.Equals(second));
        Assert.True(first == second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
    }

    [Fact]
    public void Equals_OtherAffixValue_IsFalse()
    {
        var three = GearPiece.Rolled(_sword, Rarity.Magic, [Affix(StatKind.Power, 3)]);
        var four = GearPiece.Rolled(_sword, Rarity.Magic, [Affix(StatKind.Power, 4)]);

        Assert.False(three.Equals(four));
    }

    [Fact]
    public void Equals_OtherRarity_IsFalse()
    {
        var magic = GearPiece.Rolled(_sword, Rarity.Magic, []);
        var rare = GearPiece.Rolled(_sword, Rarity.Rare, []);

        Assert.False(magic.Equals(rare));
    }

    [Fact]
    public void Equals_SameAffixesInOtherOrder_IsFalse()
    {
        var powerFirst = GearPiece.Rolled(_sword, Rarity.Rare, [Affix(StatKind.Power, 3), Affix(StatKind.CritChance, 2)]);
        var critFirst = GearPiece.Rolled(_sword, Rarity.Rare, [Affix(StatKind.CritChance, 2), Affix(StatKind.Power, 3)]);

        Assert.False(powerFirst == critFirst);
    }

    [Fact]
    public void Rolled_AffixListChangedAfterwards_KeepsItsRolls()
    {
        var affixes = new List<RolledAffix> { Affix(StatKind.Power, 3) };
        var piece = GearPiece.Rolled(_sword, Rarity.Magic, affixes);

        affixes.Add(Affix(StatKind.Power, 4));

        Assert.Single(piece.Affixes);
    }

    private static RolledAffix Affix(StatKind stat, int value) => new(stat.ToString(), stat, value);

    private static StatBonuses Bonus(StatKind stat, int value) => new(new Dictionary<StatKind, int> { [stat] = value });
}
