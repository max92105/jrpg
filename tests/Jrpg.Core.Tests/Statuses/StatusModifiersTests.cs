using Jrpg.Core.Stats;
using Jrpg.Core.Statuses;
using Jrpg.Core.Tests.Builders;

namespace Jrpg.Core.Tests.Statuses;

public sealed class StatusModifiersTests
{
    private static readonly CombatStats _stats = new(AttackDamage: 20);

    [Fact]
    public void Apply_Guarded_RaisesEveryResistance()
    {
        var statuses = new StatusSet();
        statuses.Apply(TestStatuses.Guard(20, 3));

        var current = StatusModifiers.Apply(_stats with { Resistances = Resistances.None with { Fire = 100 } }, statuses);

        Assert.Equal(Resistances.ByGroup(200, 200) with { Fire = 300 }, current.Resistances);
    }

    // 70 % raised by 20 would be 90 %, past the 75 % cap.
    [Fact]
    public void Apply_GuardedPastCap_StopsAtCap()
    {
        var statuses = new StatusSet();
        statuses.Apply(TestStatuses.Guard(20, 3));

        var current = StatusModifiers.Apply(_stats with { Resistances = Resistances.None with { Fire = 700 } }, statuses);

        Assert.Equal(750, current.Resistances.Fire);
    }

    [Fact]
    public void Apply_NoStatus_ReturnsStatsUnchanged()
    {
        var current = StatusModifiers.Apply(_stats, new StatusSet());

        Assert.Equal(_stats, current);
    }

    // Weakened's -25 % is -250 in the modifiers' sum, in permille.
    [Fact]
    public void Apply_Weakened_LowersTheDamageModifier()
    {
        var statuses = new StatusSet();
        statuses.Apply(TestStatuses.Weakened(-25, 3));

        var current = StatusModifiers.Apply(_stats, statuses);

        Assert.Equal(-250, current.DamageModifierPermille);
    }

    // Two families each change the damage dealt; the formula reads their sum (decision 704): -250 + 100.
    [Fact]
    public void Apply_TwoDamageDealtChanges_AddUp()
    {
        var statuses = new StatusSet();
        statuses.Apply(TestStatuses.Weakened(-25, 3));
        statuses.Apply(TestStatuses.Empowered(10, 3));

        var current = StatusModifiers.Apply(_stats, statuses);

        Assert.Equal(-150, current.DamageModifierPermille);
    }

    // A later source, such as gear's (decision 886), keeps its share: 100 - 250 = -150.
    [Fact]
    public void Apply_WeakenedOverANonZeroBase_AddsToIt()
    {
        var statuses = new StatusSet();
        statuses.Apply(TestStatuses.Weakened(-25, 3));

        var current = StatusModifiers.Apply(_stats with { DamageModifierPermille = 100 }, statuses);

        Assert.Equal(-150, current.DamageModifierPermille);
    }

    // A stat change is in percentage points, the stats in permille: 100 % - 15 points = 850.
    [Fact]
    public void Apply_AccuracyChange_AddsItsPoints()
    {
        var current = StatusModifiers.Apply(_stats, Holding(TestStatuses.Slowed(StatKind.Accuracy, -15, 3)));

        Assert.Equal(850, current.AccuracyPermille);
    }

    [Fact]
    public void Apply_DodgeChange_AddsItsPoints()
    {
        var current = StatusModifiers.Apply(
            _stats with { DodgePermille = 100 }, Holding(TestStatuses.Focused(StatKind.Dodge, 10, 3)));

        Assert.Equal(200, current.DodgePermille);
    }

    [Fact]
    public void Apply_CritChanceChange_AddsItsPoints()
    {
        var current = StatusModifiers.Apply(
            _stats with { CritChancePermille = 100 }, Holding(TestStatuses.Slowed(StatKind.CritChance, -5, 3)));

        Assert.Equal(50, current.CritChancePermille);
    }

    [Fact]
    public void Apply_CrowdControlResistanceChange_AddsItsPoints()
    {
        var current = StatusModifiers.Apply(
            _stats with { CrowdControlResistancePermille = 100 },
            Holding(TestStatuses.Focused(StatKind.CrowdControlResistance, 20, 3)));

        Assert.Equal(300, current.CrowdControlResistancePermille);
    }

    [Fact]
    public void Apply_AccuracyBelowZero_HeldAtZero()
    {
        var current = StatusModifiers.Apply(
            _stats with { AccuracyPermille = 100 }, Holding(TestStatuses.Slowed(StatKind.Accuracy, -15, 3)));

        Assert.Equal(0, current.AccuracyPermille);
    }

    [Fact]
    public void Apply_DodgeBelowZero_HeldAtZero()
    {
        var current = StatusModifiers.Apply(
            _stats with { DodgePermille = 100 }, Holding(TestStatuses.Slowed(StatKind.Dodge, -15, 3)));

        Assert.Equal(0, current.DodgePermille);
    }

    // 45 % + 10 points would be 55 %, past the 50 % cap (decision 295).
    [Fact]
    public void Apply_DodgeAboveTheCap_HeldAtTheCap()
    {
        var current = StatusModifiers.Apply(
            _stats with { DodgePermille = 450 }, Holding(TestStatuses.Focused(StatKind.Dodge, 10, 3)));

        Assert.Equal(500, current.DodgePermille);
    }

    [Fact]
    public void Apply_CritChanceBelowZero_HeldAtZero()
    {
        var current = StatusModifiers.Apply(
            _stats with { CritChancePermille = 30 }, Holding(TestStatuses.Slowed(StatKind.CritChance, -5, 3)));

        Assert.Equal(0, current.CritChancePermille);
    }

    [Fact]
    public void Apply_CrowdControlResistanceBelowZero_HeldAtZero()
    {
        var current = StatusModifiers.Apply(
            _stats with { CrowdControlResistancePermille = 30 },
            Holding(TestStatuses.Slowed(StatKind.CrowdControlResistance, -5, 3)));

        Assert.Equal(0, current.CrowdControlResistancePermille);
    }

    // Two families change accuracy: 100 % - 15 + 5 points = 900.
    [Fact]
    public void Apply_TwoStatChangesOfOneStat_AddUp()
    {
        var statuses = Holding(TestStatuses.Slowed(StatKind.Accuracy, -15, 3), TestStatuses.Focused(StatKind.Accuracy, 5, 3));

        var current = StatusModifiers.Apply(_stats, statuses);

        Assert.Equal(900, current.AccuracyPermille);
    }

    // 0 - 10 + 15 points is 5 %; holding the drop at 0 before the rise would give 15 %.
    [Fact]
    public void Apply_DropPastZeroThenARise_AddsUpBeforeTheBound()
    {
        var statuses = Holding(TestStatuses.Slowed(StatKind.Dodge, -10, 3), TestStatuses.Focused(StatKind.Dodge, 15, 3));

        var current = StatusModifiers.Apply(_stats, statuses);

        Assert.Equal(50, current.DodgePermille);
    }

    [Fact]
    public void ExtraDamageTakenPercent_Exposed_IsItsPower()
    {
        var statuses = new StatusSet();
        statuses.Apply(TestStatuses.Exposed(50, 3));

        var extra = StatusModifiers.ExtraDamageTakenPercent(statuses);

        Assert.Equal(50, extra);
    }

    private static StatusSet Holding(params StatusGrant[] grants)
    {
        var statuses = new StatusSet();
        foreach (var grant in grants)
        {
            statuses.Apply(grant);
        }

        return statuses;
    }
}
