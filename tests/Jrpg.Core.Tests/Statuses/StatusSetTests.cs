using Jrpg.Core.Statuses;
using Jrpg.Core.Tests.Builders;

namespace Jrpg.Core.Tests.Statuses;

public sealed class StatusSetTests
{
    private readonly StatusSet _statuses = new();

    [Fact]
    public void Apply_New_IsActiveAndFresh()
    {
        _statuses.Apply(TestStatuses.Stun(0, 2));

        var status = Assert.Single(_statuses.Active);
        Assert.Equal(
            (TestStatuses.StunId, 0, 2, true),
            (TestStatuses.IdOf(status), TestStatuses.AmountOf(status), status.TurnsLeft, status.IsFresh));
        Assert.True(_statuses.Has(TestStatuses.StunId));
    }

    [Fact]
    public void Apply_SameKindAgain_RefreshesTurnsKeepsStrongerPower()
    {
        _statuses.Apply(TestStatuses.Exposed(5, 3));
        _statuses.ClearFreshMarks();
        _statuses.CountDown();

        _statuses.Apply(TestStatuses.Exposed(2, 3));

        var status = Assert.Single(_statuses.Active);
        Assert.Equal(
            (TestStatuses.ExposedId, 5, 3, true),
            (TestStatuses.IdOf(status), TestStatuses.AmountOf(status), status.TurnsLeft, status.IsFresh));
    }

    [Fact]
    public void CountDown_Fresh_ClearsMarkKeepsTurns()
    {
        _statuses.Apply(TestStatuses.Stun(0, 2));

        var ended = _statuses.CountDown();

        Assert.Empty(ended);
        var status = Assert.Single(_statuses.Active);
        Assert.Equal(
            (TestStatuses.StunId, 0, 2, false),
            (TestStatuses.IdOf(status), TestStatuses.AmountOf(status), status.TurnsLeft, status.IsFresh));
    }

    [Fact]
    public void CountDown_LastTurn_EndsAndReturnsKind()
    {
        _statuses.Apply(TestStatuses.Stun(0, 1));
        _statuses.ClearFreshMarks();

        var ended = _statuses.CountDown();

        Assert.Equal([TestStatuses.StunId], ended.Select(TestStatuses.IdOf));
        Assert.Empty(_statuses.Active);
    }

    [Fact]
    public void Clear_Active_ReturnsItAndEmpties()
    {
        _statuses.Apply(TestStatuses.Stun(0, 2));

        var cleared = _statuses.Clear();

        Assert.Equal([TestStatuses.StunId], cleared.Select(TestStatuses.IdOf));
        Assert.False(_statuses.Has(TestStatuses.StunId));
    }
}
