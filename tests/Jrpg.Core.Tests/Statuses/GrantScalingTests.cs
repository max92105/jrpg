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

    [Fact]
    public void StunChancePermille_ResistanceBelowChance_IsTheDifference()
    {
        Assert.Equal(500, GrantScaling.StunChancePermille(scaledPower: 60, crowdControlResistancePermille: 100));
    }

    [Fact]
    public void StunChancePermille_ResistanceAboveChance_IsZero()
    {
        Assert.Equal(0, GrantScaling.StunChancePermille(scaledPower: 30, crowdControlResistancePermille: 500));
    }

    [Fact]
    public void StunChancePermille_PowerPastHundred_IsWhole()
    {
        Assert.Equal(1000, GrantScaling.StunChancePermille(scaledPower: 120, crowdControlResistancePermille: 0));
    }
}
