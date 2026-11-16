using Jrpg.Core.Statuses;
using Jrpg.Core.Tests.Builders;

namespace Jrpg.Core.Tests.Statuses;

public sealed class GrantScalingTests
{
    private static readonly StatusGrant _poison = TestStatuses.Poison(10, 3);

    [Fact]
    public void Scaled_HundredTwentyPercent_RaisesPower()
    {
        var scaled = GrantScaling.Scaled(_poison, statusPermille: 1200, extraTurns: 0);

        Assert.Equal(12, TestStatuses.AmountOf(scaled));
    }

    [Fact]
    public void Scaled_OneExtraTurn_AddsATurn()
    {
        var scaled = GrantScaling.Scaled(_poison, statusPermille: 1000, extraTurns: 1);

        Assert.Equal(4, scaled.Turns);
    }

    // Decision 317: a stun never lasts longer, however clever its user; its chance still rises.
    [Fact]
    public void Scaled_StunWithExtraTurns_KeepsItsTurns()
    {
        var scaled = GrantScaling.Scaled(TestStatuses.Stun(60, 1), statusPermille: 1200, extraTurns: 2);

        Assert.Equal(TestStatuses.Stun(72, 1), scaled);
    }

    // Decision 801: Intelligence raises every grant's chance, not only a stun's.
    [Fact]
    public void Scaled_PoisonChance_RaisedByStatusPermille()
    {
        var scaled = GrantScaling.Scaled(_poison with { Chance = 50 }, statusPermille: 1200, extraTurns: 0);

        Assert.Equal(60, TestStatuses.ChanceOf(scaled));
    }

    [Fact]
    public void ChancePermille_StunAgainstResistance_SubtractsIt()
    {
        Assert.Equal(500, GrantScaling.ChancePermille(TestStatuses.Stun(60, 1), crowdControlResistancePermille: 100));
    }

    [Fact]
    public void ChancePermille_StunBelowResistance_IsZero()
    {
        Assert.Equal(0, GrantScaling.ChancePermille(TestStatuses.Stun(30, 1), crowdControlResistancePermille: 500));
    }

    // Only a stun is cut by crowd control resistance (decision 324).
    [Fact]
    public void ChancePermille_OtherStatusAgainstResistance_IgnoresIt()
    {
        Assert.Equal(500, GrantScaling.ChancePermille(_poison with { Chance = 50 }, crowdControlResistancePermille: 300));
    }

    [Fact]
    public void ChancePermille_AboveOneHundred_HeldAtOneThousand()
    {
        Assert.Equal(1000, GrantScaling.ChancePermille(TestStatuses.Stun(120, 1), crowdControlResistancePermille: 0));
    }
}
