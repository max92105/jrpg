using Jrpg.Core.Effects;
using Jrpg.Core.Statuses;
using Jrpg.Core.Tests.Builders;

namespace Jrpg.Core.Tests.Effects;

// An effect compares its statuses in order (decision 809), so two effects built apart from the same grants are equal.
public sealed class EffectTests
{
    [Fact]
    public void Equals_SameStatusesInOrder_IsTrue()
    {
        Assert.Equal(Hex(Exposed(), Poison()), Hex(Exposed(), Poison()));
    }

    [Fact]
    public void Equals_StatusesInAnotherOrder_IsFalse()
    {
        Assert.NotEqual(Hex(Exposed(), Poison()), Hex(Poison(), Exposed()));
    }

    private static StatusGrant Exposed() => TestStatuses.Exposed(20, 2);

    private static StatusGrant Poison() => TestStatuses.Poison(8, 3);

    private static Effect Hex(params StatusGrant[] grants) =>
        new(EffectKind.GrantStatus, 0, Targeting.SingleEnemy) { Statuses = grants };
}
