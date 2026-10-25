using Jrpg.Core.Statuses;

namespace Jrpg.Core.Tests.Statuses;

public sealed class StatusSetTests
{
    private readonly StatusSet _statuses = new();

    [Fact]
    public void Apply_New_IsActiveAndFresh()
    {
        _statuses.Apply(new StatusGrant(StatusKind.Stun, Power: 0, Turns: 2));

        Assert.Equal([new ActiveStatus(StatusKind.Stun, 0, TurnsLeft: 2, IsFresh: true)], _statuses.Active);
        Assert.True(_statuses.Has(StatusKind.Stun));
    }

    [Fact]
    public void Apply_SameKindAgain_RefreshesTurnsKeepsStrongerPower()
    {
        _statuses.Apply(new StatusGrant(StatusKind.Stun, Power: 5, Turns: 3));
        _statuses.ClearFreshMarks();
        _statuses.CountDown();

        _statuses.Apply(new StatusGrant(StatusKind.Stun, Power: 2, Turns: 3));

        Assert.Equal([new ActiveStatus(StatusKind.Stun, 5, TurnsLeft: 3, IsFresh: true)], _statuses.Active);
    }

    [Fact]
    public void CountDown_Fresh_ClearsMarkKeepsTurns()
    {
        _statuses.Apply(new StatusGrant(StatusKind.Stun, Power: 0, Turns: 2));

        var ended = _statuses.CountDown();

        Assert.Empty(ended);
        Assert.Equal([new ActiveStatus(StatusKind.Stun, 0, TurnsLeft: 2, IsFresh: false)], _statuses.Active);
    }

    [Fact]
    public void CountDown_LastTurn_EndsAndReturnsKind()
    {
        _statuses.Apply(new StatusGrant(StatusKind.Stun, Power: 0, Turns: 1));
        _statuses.ClearFreshMarks();

        var ended = _statuses.CountDown();

        Assert.Equal([StatusKind.Stun], ended);
        Assert.Empty(_statuses.Active);
    }

    [Fact]
    public void Clear_Active_ReturnsItAndEmpties()
    {
        _statuses.Apply(new StatusGrant(StatusKind.Stun, Power: 0, Turns: 2));

        var cleared = _statuses.Clear();

        Assert.Equal([StatusKind.Stun], cleared);
        Assert.False(_statuses.Has(StatusKind.Stun));
    }
}
