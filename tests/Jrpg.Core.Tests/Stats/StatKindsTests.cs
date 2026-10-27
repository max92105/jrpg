using Jrpg.Core.Stats;

namespace Jrpg.Core.Tests.Stats;

public sealed class StatKindsTests
{
    public static TheoryData<DamageType> EveryDamageType => [.. Enum.GetValues<DamageType>()];

    [Fact]
    public void ResistanceTo_Fire_IsFireResistance()
    {
        Assert.Equal(StatKind.FireResistance, StatKinds.ResistanceTo(DamageType.Fire));
    }

    [Theory]
    [MemberData(nameof(EveryDamageType))]
    public void TypeResistedBy_EachTypesResistance_IsThatType(DamageType type)
    {
        Assert.Equal(type, StatKinds.TypeResistedBy(StatKinds.ResistanceTo(type)));
    }

    [Fact]
    public void TypeResistedBy_Power_IsNull()
    {
        Assert.Null(StatKinds.TypeResistedBy(StatKind.Power));
    }
}
