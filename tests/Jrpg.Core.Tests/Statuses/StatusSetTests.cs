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
    public void Apply_SameStatusAgain_RefreshesTurnsKeepsStrongerAmount()
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
    public void Apply_TwoRecordsOfOneFamily_KeepsOne()
    {
        _statuses.Apply(new StatusGrant(Poison("poison_1", 8), 3));

        _statuses.Apply(new StatusGrant(Poison("poison_2", 12), 2));

        var status = Assert.Single(_statuses.Active);
        Assert.Equal(("poison_2", 2), (status.Status.Id, status.TurnsLeft));
    }

    // A ward raises resistances and softens hits; the held one resists more, the granted one softens more.
    [Fact]
    public void Apply_WeakerRecordOfAHeldFamily_KeepsTheStrongerAmountPerPart()
    {
        _statuses.Apply(new StatusGrant(Ward("ward_2", resistance: 20, damageTaken: -10), 3));

        _statuses.Apply(new StatusGrant(Ward("ward_1", resistance: 10, damageTaken: -30), 3));

        var status = Assert.Single(_statuses.Active);
        Assert.Equal("ward_1", status.Status.Id);
        Assert.Equal([20, -30], status.Status.Parts.Select(part => part.Amount));
    }

    [Fact]
    public void Apply_NegativeAmountsOfOneFamily_KeepsTheFartherFromZero()
    {
        _statuses.Apply(new StatusGrant(Shield("shield_2", damageTaken: -25), 3));

        _statuses.Apply(new StatusGrant(Shield("shield_1", damageTaken: -10), 3));

        Assert.Equal(-25, Assert.Single(_statuses.Active).Status.Parts.Single().Amount);
    }

    // Decision 800's named change: two families, so a Poison doesn't refresh a Burning away.
    [Fact]
    public void Apply_TwoFamilies_KeepsBoth()
    {
        _statuses.Apply(new StatusGrant(Burning(), 2));

        _statuses.Apply(new StatusGrant(Poison("poison_1", 8), 3));

        Assert.Equal(["burning_1", "poison_1"], _statuses.Active.Select(status => status.Status.Id));
    }

    [Fact]
    public void FirstWith_SkipTurnHeld_ReturnsThatStatus()
    {
        _statuses.Apply(new StatusGrant(Burning(), 2));
        _statuses.Apply(TestStatuses.Stun(100, 1));

        var skipping = _statuses.FirstWith(StatusPartKind.SkipTurn);

        Assert.Equal(TestStatuses.StunId, skipping?.Id);
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
    public void CountDown_LastTurn_EndsAndReturnsIt()
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

    private static StatusDefinition Poison(string id, int amount) =>
        Record(id, "poison", new StatusPart(StatusPartKind.DamageEachTurn, amount));

    private static StatusDefinition Burning() => Record("burning_1", "burning", new StatusPart(StatusPartKind.DamageEachTurn, 5));

    private static StatusDefinition Ward(string id, int resistance, int damageTaken) => Record(
        id,
        "ward",
        new StatusPart(StatusPartKind.ResistanceChange, resistance),
        new StatusPart(StatusPartKind.DamageTakenChange, damageTaken));

    private static StatusDefinition Shield(string id, int damageTaken) =>
        Record(id, "shield", new StatusPart(StatusPartKind.DamageTakenChange, damageTaken));

    private static StatusDefinition Record(string id, string family, params StatusPart[] parts) =>
        new() { Id = id, Name = id, Description = "", Family = family, Parts = parts };
}
