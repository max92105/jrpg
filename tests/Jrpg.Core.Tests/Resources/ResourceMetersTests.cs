using Jrpg.Core.Resources;

namespace Jrpg.Core.Tests.Resources;

public sealed class ResourceMetersTests
{
    private static readonly ResourceDefinition _mana = new("mana", "Mana", Max: 10, StartsAt: 3);

    [Fact]
    public void Constructor_Definitions_StartAtStartsAt()
    {
        var meters = new ResourceMeters([ResourceStart.Fixed(_mana), ResourceStart.Fixed(new ResourceDefinition("clarity", "Clarity", Max: 3, StartsAt: 2))]);

        Assert.Equal((3, 2), (meters.AmountOf("mana"), meters.AmountOf("clarity")));
    }

    [Fact]
    public void Change_AboveMax_ClampsAndReturnsApplied()
    {
        var meters = new ResourceMeters([ResourceStart.Fixed(_mana)]);

        var applied = meters.Change("mana", 9);

        Assert.Equal((7, 10), (applied, meters.AmountOf("mana")));
    }

    [Fact]
    public void Change_BelowZero_ClampsAndReturnsApplied()
    {
        var meters = new ResourceMeters([ResourceStart.Fixed(_mana)]);

        var applied = meters.Change("mana", -5);

        Assert.Equal((-3, 0), (applied, meters.AmountOf("mana")));
    }

    [Fact]
    public void Change_StartMaxAboveDefinitionMax_ClampsToStartMax()
    {
        var readiness = new ResourceDefinition("readiness", "Readiness") { MaxFromSpeed = new SpeedScaling(10) };
        var meters = new ResourceMeters([new ResourceStart(readiness, Max: 3, Amount: 1)]);

        var applied = meters.Change("readiness", 5);

        Assert.Equal((2, 3), (applied, meters.AmountOf("readiness")));
    }

    [Fact]
    public void AmountOf_UnknownId_Throws()
    {
        var meters = new ResourceMeters([ResourceStart.Fixed(_mana)]);

        var exception = Assert.Throws<ArgumentException>(() => meters.AmountOf("stamina"));

        Assert.Contains("'stamina'", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Constructor_BothZero_NeitherDominant()
    {
        var meters = Forces();

        Assert.Equal((false, false), (meters.IsDominant("growth"), meters.IsDominant("decay")));
    }

    [Fact]
    public void Change_ForceGoesAbove_BecomesDominant()
    {
        var meters = Forces();

        meters.Change("decay", 2);

        Assert.Equal((false, true), (meters.IsDominant("growth"), meters.IsDominant("decay")));
    }

    [Fact]
    public void Change_RivalTies_DominanceKept()
    {
        var meters = Forces();
        meters.Change("decay", 2);

        meters.Change("growth", 2);

        Assert.Equal((false, true), (meters.IsDominant("growth"), meters.IsDominant("decay")));
    }

    [Fact]
    public void Change_RivalGoesAbove_DominanceChanges()
    {
        var meters = Forces();
        meters.Change("decay", 2);

        meters.Change("growth", 3);

        Assert.Equal((true, false), (meters.IsDominant("growth"), meters.IsDominant("decay")));
    }

    // Spending the leader below its rival hands dominance over too: the rule is about the amounts, not who changed.
    [Fact]
    public void Change_LeaderSpentBelowRival_DominanceChanges()
    {
        var meters = Forces();
        meters.Change("decay", 4);
        meters.Change("growth", 3);

        meters.Change("decay", -3);

        Assert.Equal((true, false), (meters.IsDominant("growth"), meters.IsDominant("decay")));
    }

    [Fact]
    public void IsDominant_UnknownId_Throws()
    {
        var meters = Forces();

        Assert.Throws<ArgumentException>(() => meters.IsDominant("mana"));
    }

    private static ResourceMeters Forces() => new(
    [
        ResourceStart.Fixed(new ResourceDefinition("growth", "Growth", Max: 10) { Dominance = new DominanceRule("decay", 50) }),
        ResourceStart.Fixed(new ResourceDefinition("decay", "Decay", Max: 10) { Dominance = new DominanceRule("growth", 50) }),
    ]);
}
