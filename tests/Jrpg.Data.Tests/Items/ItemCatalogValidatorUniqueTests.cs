using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Core.Loot;
using Jrpg.Core.Stats;
using Jrpg.Data.Content;
using Jrpg.Data.Items;

namespace Jrpg.Data.Tests.Items;

// A unique is worn and only it has fixed affixes; each names a pool affix once, with a nonzero value within the bonus
// bound on its affix's side of zero, free of the affix's roll range (ADR 0058, decision 866). Items and the pool are
// built in this test.
public sealed class ItemCatalogValidatorUniqueTests
{
    private const string Cleaver = "items.json: item 'cleaver'";

    private static readonly IReadOnlyDictionary<string, AffixDefinition> _pool = new Dictionary<string, AffixDefinition>
    {
        ["power"] = Affix("power", StatKind.Power, 2, 5),
        ["clumsiness"] = Affix("clumsiness", StatKind.Agility, -5, -2),
        ["life_steal"] = Affix("life_steal", StatKind.LifeSteal, 1, 3),
        ["fire_ward"] = Affix("fire_ward", StatKind.FireResistance, 5, 10) with { Kinds = [EquipmentKind.Chest] },
    };

    private static readonly ItemDefinition _cleaver = new()
    {
        Id = "cleaver",
        Name = "Cleaver",
        Description = "",
        IconFile = "Weapon.png",
        MaxStack = 99,
        IsUsableFromMenu = false,
        Equipment = new EquipmentStats(EquipmentKind.Weapon) { Damage = 28, DamageType = DamageType.Slashing },
        IsUnique = true,
        FixedAffixes = [Fixed("power", 5)],
    };

    public static TheoryData<ItemDefinition, string> InvalidUniques => new()
    {
        {
            _cleaver with { Equipment = null, Effect = new Effect(EffectKind.RestoreHp, Power: 15, Targeting.SingleAlly) },
            $"{Cleaver}: isUnique needs equipment: a unique is worn (ADR 0058)."
        },
        {
            _cleaver with { Equipment = null, IsMaterial = true },
            $"{Cleaver}: isUnique needs equipment: a unique is worn (ADR 0058)."
        },
        { _cleaver with { IsUnique = false }, $"{Cleaver}: fixedAffixes are only for a unique (isUnique true)." },
        { WithFixed(Fixed("power", 5), Fixed("power", 3)), $"{Cleaver}: fixedAffixes names 'power' twice." },
        { WithFixed(Fixed("power", 5), null!), $"{Cleaver}: fixedAffixes[1] is null." },
        { WithFixed(Fixed("spite", 5)), $"{Cleaver}: fixedAffixes[0] (spite): affixes.json has no such affix." },
        { WithFixed(Fixed("power", 0)), $"{Cleaver}: fixedAffixes[0] (power): {OutOfBound(0)}" },
        { WithFixed(Fixed("power", 101)), $"{Cleaver}: fixedAffixes[0] (power): {OutOfBound(101)}" },
        { WithFixed(Fixed("clumsiness", -101)), $"{Cleaver}: fixedAffixes[0] (clumsiness): {OutOfBound(-101)}" },
        {
            WithFixed(Fixed("clumsiness", int.MinValue)),
            $"{Cleaver}: fixedAffixes[0] (clumsiness): {OutOfBound(int.MinValue)}"
        },
        {
            WithFixed(Fixed("power", -3)),
            $"{Cleaver}: fixedAffixes[0] (power): value must be above 0, as affix 'power' rolls (was -3)."
        },
        {
            WithFixed(Fixed("clumsiness", 2)),
            $"{Cleaver}: fixedAffixes[0] (clumsiness): value must be below 0, as affix 'clumsiness' rolls (was 2)."
        },
        {
            WithFixed(Fixed("life_steal", -1)),
            $"{Cleaver}: fixedAffixes[0] (life_steal): value must be above 0, as affix 'life_steal' rolls (was -1)."
        },
    };

    [Theory]
    [MemberData(nameof(InvalidUniques))]
    public void Validate_InvalidUnique_ThrowsNamingTheItemAndRule(ItemDefinition item, string message)
    {
        var exception = Assert.Throws<ContentValidationException>(() => Validate(item));

        Assert.Equal(message, exception.Message);
    }

    [Theory]
    [InlineData("power", 100)]
    [InlineData("power", 1)]
    [InlineData("clumsiness", -100)]
    [InlineData("clumsiness", -1)]
    public void Validate_UniqueValueAtTheBound_Accepted(string affixId, int value)
    {
        Assert.Null(Record.Exception(() => Validate(WithFixed(Fixed(affixId, value)))));
    }

    [Fact]
    public void Validate_UniqueValueAboveItsAffixsRollRange_Accepted()
    {
        Assert.Null(Record.Exception(() => Validate(WithFixed(Fixed("power", 7)))));
    }

    [Fact]
    public void Validate_UniqueWithNoFixedAffix_Accepted()
    {
        Assert.Null(Record.Exception(() => Validate(WithFixed())));
    }

    // fire_ward rolls on chests only; a unique sword may still carry it (decision 866).
    [Fact]
    public void Validate_UniqueNamingAnAffixThatRollsOnOtherKinds_Accepted()
    {
        Assert.Null(Record.Exception(() => Validate(WithFixed(Fixed("fire_ward", 10), Fixed("life_steal", 2)))));
    }

    private static void Validate(ItemDefinition item) => ItemCatalogValidator.Validate("items.json", [item], _pool);

    private static string OutOfBound(int value) => $"value must be nonzero and within -100..100 (was {value}).";

    private static ItemDefinition WithFixed(params RolledAffix[] fixedAffixes) => _cleaver with { FixedAffixes = fixedAffixes };

    // The stat is the pool's at load; the rules read the pool, so any stat stands in here.
    private static RolledAffix Fixed(string affixId, int value) => new(affixId, StatKind.Power, value);

    private static AffixDefinition Affix(string id, StatKind stat, int min, int max) => new()
    {
        Id = id,
        Stat = stat,
        Min = min,
        Max = max,
        Kinds = [EquipmentKind.Weapon],
        Weight = 10,
        GoldValue = 20,
    };
}
