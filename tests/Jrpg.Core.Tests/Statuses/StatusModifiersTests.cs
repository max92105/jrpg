using Jrpg.Core.Stats;
using Jrpg.Core.Statuses;

namespace Jrpg.Core.Tests.Statuses;

public sealed class StatusModifiersTests
{
    private static readonly CombatStats _stats = new(Attack: 20, Defense: 5);

    [Fact]
    public void Apply_Guarded_RaisesEveryResistance()
    {
        var statuses = new StatusSet();
        statuses.Apply(new StatusGrant(StatusKind.Guarded, Power: 20, Turns: 3));

        var current = StatusModifiers.Apply(_stats with { Resistances = Resistances.None with { Fire = 100 } }, statuses);

        Assert.Equal(Resistances.ByGroup(200, 200) with { Fire = 300 }, current.Resistances);
    }

    // 70 % raised by 20 would be 90 %, past the 75 % cap.
    [Fact]
    public void Apply_GuardedPastCap_StopsAtCap()
    {
        var statuses = new StatusSet();
        statuses.Apply(new StatusGrant(StatusKind.Guarded, Power: 20, Turns: 3));

        var current = StatusModifiers.Apply(_stats with { Resistances = Resistances.None with { Fire = 700 } }, statuses);

        Assert.Equal(750, current.Resistances.Fire);
    }

    [Fact]
    public void Apply_NoStatus_ReturnsStatsUnchanged()
    {
        var current = StatusModifiers.Apply(_stats, new StatusSet());

        Assert.Equal(_stats, current);
    }

    [Fact]
    public void ExtraDamageTakenPercent_Exposed_IsItsPower()
    {
        var statuses = new StatusSet();
        statuses.Apply(new StatusGrant(StatusKind.Exposed, Power: 50, Turns: 3));

        var extra = StatusModifiers.ExtraDamageTakenPercent(statuses);

        Assert.Equal(50, extra);
    }
}
