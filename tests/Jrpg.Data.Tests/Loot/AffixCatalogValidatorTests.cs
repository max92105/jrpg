using Jrpg.Core.Items;
using Jrpg.Core.Loot;
using Jrpg.Core.Stats;
using Jrpg.Data.Content;
using Jrpg.Data.Loot;

namespace Jrpg.Data.Tests.Loot;

public sealed class AffixCatalogValidatorTests
{
    private static readonly AffixDefinition _power = new()
    {
        Id = "power",
        Stat = StatKind.Power,
        Min = 2,
        Max = 5,
        Kinds = [EquipmentKind.Weapon, EquipmentKind.Ring],
        Weight = 10,
    };

    public static TheoryData<AffixDefinition, string> InvalidAffixes => new()
    {
        { _power with { Min = 6 }, "min must be at most max (was 6 and 5)." },
        { _power with { Min = 0 }, "min must be nonzero and within -100..100 (was 0)." },
        { _power with { Max = 101 }, "max must be nonzero and within -100..100 (was 101)." },
        { _power with { Kinds = [] }, "kinds must list at least one kind of gear." },
        { _power with { Weight = 0 }, "weight must be at least 1 (was 0)." },
        { _power with { Min = -3, Max = 2 }, "min and max must be on the same side of 0, or a roll could add nothing (was -3 and 2)." },
    };

    [Fact]
    public void Validate_ValidAffix_DoesNotThrow()
    {
        var exception = Record.Exception(() => AffixCatalogValidator.Validate("affixes.json", [_power]));

        Assert.Null(exception);
    }

    [Theory]
    [MemberData(nameof(InvalidAffixes))]
    public void Validate_InvalidField_ThrowsNamingTheAffixAndField(AffixDefinition affix, string problem)
    {
        var exception = Assert.Throws<ContentValidationException>(() => AffixCatalogValidator.Validate("affixes.json", [affix]));

        Assert.Equal($"affixes.json: affix 'power': {problem}", exception.Message);
    }
}
