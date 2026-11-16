using Jrpg.Core.Stats;
using Jrpg.Core.Statuses;
using Jrpg.Data.Content;
using Jrpg.Data.Statuses;
using static Jrpg.Core.Statuses.StatusPartKind;

namespace Jrpg.Data.Tests.Statuses;

// statuses.json's rules (ADR 0055, decision 797), over records built here.
public sealed class StatusCatalogValidatorTests
{
    private const string FileName = "statuses.json";

    private static readonly StatusDefinition _poison = new()
    {
        Id = "poison_1",
        Name = "Poison",
        Description = "",
        IsHarmful = true,
        Family = "poison",
        Parts = [new StatusPart(DamageEachTurn, 8)],
    };

    public static TheoryData<StatusDefinition, string> InvalidStatuses => new()
    {
        { _poison with { Name = " " }, "name must not be empty." },
        { _poison with { Family = "" }, "family must name the statuses it shares, or be left out." },
        { _poison with { Parts = [] }, "parts must list at least one part." },
        { With(Part(DamageEachTurn, 8), null!), "parts[1] is null." },
        { With(Part(DamageEachTurn, 0)), "parts[0]: damageEachTurn's amount must be at least 1 (was 0)." },
        { With(Part(HealEachTurn, 0)), "parts[0]: healEachTurn's amount must be at least 1 (was 0)." },
        { With(Part(SkipTurn, 5)), "parts[0]: skipTurn has no amount (was 5)." },
        { With(Part(DrawAttacks, 1)), "parts[0]: drawAttacks has no amount (was 1)." },
        { With(Part(ResistanceChange, 0)), "parts[0]: resistanceChange's amount must not be 0." },
        { With(Part(DamageTakenChange, -100)), "parts[0]: damageTakenChange's amount must be above -100 and not 0 (was -100)." },
        { With(Part(DamageTakenChange, 0)), "parts[0]: damageTakenChange's amount must be above -100 and not 0 (was 0)." },
        { With(Part(DamageDealtChange, -100)), "parts[0]: damageDealtChange's amount must be above -100 and not 0 (was -100)." },
        { With(Part(DamageDealtChange, 0)), "parts[0]: damageDealtChange's amount must be above -100 and not 0 (was 0)." },
        {
            With(Part(DamageEachTurn, 8), Part(HealEachTurn, 4)),
            "a status ticks once: it can't hold both damageEachTurn and healEachTurn."
        },
        { With(Part(ResistanceChange, 8), Part(ResistanceChange, 4)), "parts[1]: resistanceChange is listed twice." },
        {
            With(new StatusPart(HealEachTurn, 4, DamageType.Fire)),
            "parts[0]: damageType is only for damageEachTurn (was fire on healEachTurn)."
        },
        { With(Part(StatChange, -15)), "parts[0]: statChange must name a stat." },
        {
            With(new StatusPart(DamageEachTurn, 8, Stat: StatKind.Accuracy)),
            "parts[0]: stat is only for statChange (was accuracy on damageEachTurn)."
        },
        {
            With(Changing(StatKind.Accuracy, -15), Changing(StatKind.Speed, -15)),
            "parts[1]: statChange can't change speed (it changes accuracy, dodge, critChance, crowdControlResistance)."
        },
        {
            With(Changing(StatKind.Accuracy, -15), Changing(StatKind.Accuracy, -5)),
            "parts[1]: statChange of accuracy is listed twice."
        },
        { With(Changing(StatKind.Dodge, 0)), "parts[0]: statChange's amount must not be 0." },
        { _poison with { MarkLetter = "LL" }, "markLetter must be one letter or digit (was 'LL')." },
        { _poison with { MarkLetter = " " }, "markLetter must be one letter or digit (was ' ')." },
        { _poison with { MarkLetter = "" }, "markLetter must be one letter or digit (was '')." },
    };

    [Theory]
    [MemberData(nameof(InvalidStatuses))]
    public void Validate_InvalidStatus_ThrowsNamingTheStatusAndTheProblem(StatusDefinition status, string problem)
    {
        var exception = Assert.Throws<ContentValidationException>(() => Validate(status));

        Assert.Equal($"statuses.json: status 'poison_1': {problem}", exception.Message);
    }

    [Fact]
    public void Validate_IdTwice_Throws()
    {
        var exception = Assert.Throws<ContentValidationException>(() => Validate(_poison, _poison));

        Assert.Equal("statuses.json: status id 'poison_1' is used more than once.", exception.Message);
    }

    [Fact]
    public void Validate_OneFamilyWithOtherPartKinds_Throws()
    {
        var unlike = _poison with { Id = "poison_2", Parts = [Part(HealEachTurn, 8)] };

        var exception = Assert.Throws<ContentValidationException>(() => Validate(_poison, unlike));

        Assert.Equal("statuses.json: family 'poison': poison_2's parts differ from poison_1's.", exception.Message);
    }

    [Fact]
    public void Validate_OneFamilyWithAnotherDamageType_Throws()
    {
        var fire = _poison with { Id = "poison_2", Parts = [new StatusPart(DamageEachTurn, 15, DamageType.Fire)] };

        var exception = Assert.Throws<ContentValidationException>(() => Validate(_poison, fire));

        Assert.Equal("statuses.json: family 'poison': poison_2's parts differ from poison_1's.", exception.Message);
    }

    // A refresh takes the granted record's timing: a family ticking at two times could tick twice in one turn.
    [Fact]
    public void Validate_OneFamilyWithAnotherTiming_Throws()
    {
        var early = _poison with { Id = "poison_2", ActsAt = StatusTiming.StartOfTurn };

        var exception = Assert.Throws<ContentValidationException>(() => Validate(_poison, early));

        Assert.Equal("statuses.json: family 'poison': poison_2 acts at another time than poison_1.", exception.Message);
    }

    // Part for part: a refresh compares the parts in order (decision 800).
    [Fact]
    public void Validate_OneFamilyWithTheSameKindsInAnotherOrder_Throws()
    {
        var first = With(Part(DamageEachTurn, 8), Part(DamageTakenChange, 10));
        var swapped = first with { Id = "poison_2", Parts = [Part(DamageTakenChange, 10), Part(DamageEachTurn, 8)] };

        var exception = Assert.Throws<ContentValidationException>(() => Validate(first, swapped));

        Assert.Equal("statuses.json: family 'poison': poison_2's parts differ from poison_1's.", exception.Message);
    }

    // Decision 800: a family's records change the same stats, so a refresh can keep the stronger of each.
    [Fact]
    public void Validate_OneFamilyWithAnotherStat_Throws()
    {
        var slowed = _poison with { Family = "slowed", Parts = [Changing(StatKind.Accuracy, -15)] };
        var dodging = slowed with { Id = "poison_2", Parts = [Changing(StatKind.Dodge, -15)] };

        var exception = Assert.Throws<ContentValidationException>(() => Validate(slowed, dodging));

        Assert.Equal("statuses.json: family 'slowed': poison_2's parts differ from poison_1's.", exception.Message);
    }

    [Fact]
    public void Validate_TwoStatChangesOfDifferentStats_Accepted() =>
        Assert.Null(Record.Exception(() => Validate(With(Changing(StatKind.Accuracy, -15), Changing(StatKind.CritChance, -5)))));

    [Theory]
    [InlineData("L")]
    [InlineData("7")]
    public void Validate_MarkLetterOfOneLetterOrDigit_Accepted(string markLetter) =>
        Assert.Null(Record.Exception(() => Validate(_poison with { MarkLetter = markLetter })));

    [Fact]
    public void Validate_DamageEachTurnOfOne_Accepted() =>
        Assert.Null(Record.Exception(() => Validate(With(Part(DamageEachTurn, 1)))));

    [Fact]
    public void Validate_TypedDamageEachTurn_Accepted() =>
        Assert.Null(Record.Exception(() => Validate(With(new StatusPart(DamageEachTurn, 8, DamageType.Fire)))));

    [Fact]
    public void Validate_DamageTakenChangeOfMinus99_Accepted() =>
        Assert.Null(Record.Exception(() => Validate(With(Part(DamageTakenChange, -99)))));

    [Fact]
    public void Validate_DamageDealtChangeOfMinus99_Accepted() =>
        Assert.Null(Record.Exception(() => Validate(With(Part(DamageDealtChange, -99)))));

    [Fact]
    public void Validate_NegativeResistanceChange_Accepted() =>
        Assert.Null(Record.Exception(() => Validate(With(Part(ResistanceChange, -10)))));

    [Fact]
    public void Validate_TwoRecordsOfOneFamilyWithOtherAmounts_Accepted()
    {
        var stronger = _poison with { Id = "poison_2", Parts = [Part(DamageEachTurn, 15)] };

        Assert.Null(Record.Exception(() => Validate(_poison, stronger)));
    }

    private static void Validate(params StatusDefinition[] statuses) => StatusCatalogValidator.Validate(FileName, statuses);

    private static StatusDefinition With(params StatusPart[] parts) => _poison with { Parts = parts };

    private static StatusPart Part(StatusPartKind kind, int amount) => new(kind, amount);

    private static StatusPart Changing(StatKind stat, int points) => new(StatChange, points, Stat: stat);
}
