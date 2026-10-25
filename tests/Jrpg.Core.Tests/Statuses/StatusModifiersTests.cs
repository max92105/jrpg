using Jrpg.Core.Stats;
using Jrpg.Core.Statuses;

namespace Jrpg.Core.Tests.Statuses;

public sealed class StatusModifiersTests
{
    private static readonly CombatStats _stats = new(Attack: 20, Defense: 5);

    [Fact]
    public void Apply_Guarded_AddsPowerToDefense()
    {
        var statuses = new StatusSet();
        statuses.Apply(new StatusGrant(StatusKind.Guarded, Power: 5, Turns: 3));

        var current = StatusModifiers.Apply(_stats, statuses);

        Assert.Equal(new CombatStats(Attack: 20, Defense: 10), current);
    }

    [Fact]
    public void Apply_NoStatus_ReturnsStatsUnchanged()
    {
        var current = StatusModifiers.Apply(_stats, new StatusSet());

        Assert.Equal(_stats, current);
    }
}
