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

    // A later source, such as gear's (row 9), keeps its share: 100 - 250 = -150.
    [Fact]
    public void Apply_WeakenedOverANonZeroBase_AddsToIt()
    {
        var statuses = new StatusSet();
        statuses.Apply(TestStatuses.Weakened(-25, 3));

        var current = StatusModifiers.Apply(_stats with { DamageModifierPermille = 100 }, statuses);

        Assert.Equal(-150, current.DamageModifierPermille);
    }

    [Fact]
    public void ExtraDamageTakenPercent_Exposed_IsItsPower()
    {
        var statuses = new StatusSet();
        statuses.Apply(TestStatuses.Exposed(50, 3));

        var extra = StatusModifiers.ExtraDamageTakenPercent(statuses);

        Assert.Equal(50, extra);
    }
}
