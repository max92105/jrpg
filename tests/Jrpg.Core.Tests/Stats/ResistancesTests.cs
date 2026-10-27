using Jrpg.Core.Stats;

namespace Jrpg.Core.Tests.Stats;

public sealed class ResistancesTests
{
    [Fact]
    public void Of_Fire_ReadsFire()
    {
        var resistances = Resistances.None with { Fire = 120, Ice = 80 };

        Assert.Equal(120, resistances.Of(DamageType.Fire));
    }

    [Fact]
    public void ByGroup_PhysicalAndMagical_FillsEachGroup()
    {
        var resistances = Resistances.ByGroup(physicalPermille: 30, magicalPermille: 70);

        Assert.Equal(new Resistances(30, 30, 30, 70, 70, 70, 70, 70), resistances);
    }

    [Fact]
    public void Clamped_AboveCap_IsCap()
    {
        var resistances = Resistances.None with { Light = 900 };

        Assert.Equal(750, resistances.Clamped().Light);
    }

    [Fact]
    public void Clamped_BelowFloor_IsFloor()
    {
        var resistances = Resistances.None with { Dark = -1500 };

        Assert.Equal(-1000, resistances.Clamped().Dark);
    }

    [Fact]
    public void Clamped_Weakness_KeepsIt()
    {
        var resistances = Resistances.None with { Fire = -250 };

        Assert.Equal(-250, resistances.Clamped().Fire);
    }
}
