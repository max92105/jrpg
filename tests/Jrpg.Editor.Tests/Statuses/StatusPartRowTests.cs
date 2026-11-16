using Jrpg.Core.Stats;
using Jrpg.Core.Statuses;
using Jrpg.Editor.Statuses;

namespace Jrpg.Editor.Tests.Statuses;

// One part of a status as the Statuses page types it (decision 984): the amount's label by kind, and only the boxes the
// kind reads written.
public sealed class StatusPartRowTests
{
    public static TheoryData<StatusPartKind> EveryKind => [.. Enum.GetValues<StatusPartKind>()];

    [Theory]
    [MemberData(nameof(EveryKind))]
    public void AmountLabel_EveryKind_HasOne(StatusPartKind kind)
    {
        var row = StatusPartRow.New();

        row.Kind = kind;

        var isAmountless = kind is StatusPartKind.SkipTurn or StatusPartKind.DrawAttacks;
        Assert.Equal(isAmountless, row.AmountLabel.Length == 0);
        Assert.Equal(!isAmountless, row.HasAmount);
    }

    // The amount box still holds 5 under the hidden box: only the kind decides what is written.
    [Fact]
    public void ToPart_SkipTurn_WritesNoAmount()
    {
        var row = StatusPartRow.From(new StatusPart(StatusPartKind.DamageEachTurn, 5));

        row.Kind = StatusPartKind.SkipTurn;

        Assert.Equal(new StatusPart(StatusPartKind.SkipTurn), row.ToPart());
    }

    [Fact]
    public void ToPart_TypeChosenThenKindChanged_WritesNoType()
    {
        var row = StatusPartRow.New();
        row.DamageType = DamageType.Fire;

        row.Kind = StatusPartKind.HealEachTurn;

        Assert.Equal(new StatusPart(StatusPartKind.HealEachTurn, 1), row.ToPart());
        Assert.False(row.CanTypeDamageType);
    }

    [Fact]
    public void ToPart_DamageTickWithType_WritesTheType()
    {
        var row = StatusPartRow.New();

        row.DamageType = DamageType.Fire;

        Assert.Equal(new StatusPart(StatusPartKind.DamageEachTurn, 1, DamageType.Fire), row.ToPart());
    }

    // Poison's tick has no type: the box must show none, not stay blank.
    [Fact]
    public void From_TypelessTick_ChoosesNone()
    {
        var row = StatusPartRow.From(new StatusPart(StatusPartKind.DamageEachTurn, 4));

        Assert.Equal(new DamageTypeChoice(null, "(none: ignores resistances)"), row.SelectedDamageTypeChoice);
    }

    [Fact]
    public void From_FireTick_ChoosesFire()
    {
        var row = StatusPartRow.From(new StatusPart(StatusPartKind.DamageEachTurn, 4, DamageType.Fire));

        Assert.Equal(new DamageTypeChoice(DamageType.Fire, "fire"), row.SelectedDamageTypeChoice);
    }

    [Fact]
    public void ToPart_TypeSetBackToNone_WritesNoType()
    {
        var row = StatusPartRow.From(new StatusPart(StatusPartKind.DamageEachTurn, 4, DamageType.Fire));

        row.SelectedDamageTypeChoice = row.DamageTypeChoices[0];

        Assert.Equal(new StatusPart(StatusPartKind.DamageEachTurn, 4), row.ToPart());
    }

    [Fact]
    public void DamageType_Picked_RaisesTheChosenEntry()
    {
        var row = StatusPartRow.New();
        var notified = new List<string?>();
        row.PropertyChanged += (_, e) => notified.Add(e.PropertyName);

        row.SelectedDamageTypeChoice = row.DamageTypeChoices[1];

        Assert.Contains(nameof(StatusPartRow.SelectedDamageTypeChoice), notified);
        Assert.Equal(Enum.GetValues<DamageType>()[0], row.DamageType);
    }

    [Fact]
    public void ToPart_StatChange_WritesTheStat()
    {
        var row = StatusPartRow.New();

        row.Kind = StatusPartKind.StatChange;
        row.AmountText = "-15";
        row.Stat = StatKind.Accuracy;

        Assert.Equal(new StatusPart(StatusPartKind.StatChange, -15, Stat: StatKind.Accuracy), row.ToPart());
    }

    [Fact]
    public void ToPart_StatChosenThenKindChanged_WritesNoStat()
    {
        var row = StatusPartRow.From(new StatusPart(StatusPartKind.StatChange, 10, Stat: StatKind.Dodge));

        row.Kind = StatusPartKind.ResistanceChange;

        Assert.Equal(new StatusPart(StatusPartKind.ResistanceChange, 10), row.ToPart());
        Assert.False(row.CanTypeStat);
    }

    [Fact]
    public void Problem_StatChangeWithoutStat_SaysSo()
    {
        var row = StatusPartRow.New();

        row.Kind = StatusPartKind.StatChange;

        Assert.Equal("A stat change must name its stat.", row.Problem);
    }

    [Fact]
    public void Problem_AmountNotANumber_NamesTheBox()
    {
        var row = StatusPartRow.New();

        row.AmountText = "x";

        Assert.Equal("Amount must be a whole number (was 'x').", row.Problem);
    }

    // A kind without an amount hides the box, so what it holds can't stop the part.
    [Fact]
    public void Problem_DrawAttacksWithAmountNotANumber_IsNull()
    {
        var row = StatusPartRow.New();
        row.AmountText = "x";

        row.Kind = StatusPartKind.DrawAttacks;

        Assert.Null(row.Problem);
    }

    [Fact]
    public void Kind_Changed_RaisesTheBoxStates()
    {
        var row = StatusPartRow.New();
        var notified = new List<string?>();
        row.PropertyChanged += (_, e) => notified.Add(e.PropertyName);

        row.Kind = StatusPartKind.StatChange;

        Assert.Superset(
            new HashSet<string?>
            {
                nameof(StatusPartRow.AmountLabel),
                nameof(StatusPartRow.HasAmount),
                nameof(StatusPartRow.CanTypeDamageType),
                nameof(StatusPartRow.CanTypeStat),
                nameof(StatusPartRow.Problem),
            },
            notified.ToHashSet());
    }

    [Fact]
    public void Changed_StatPicked_Raised()
    {
        var row = StatusPartRow.New();
        var raised = 0;
        row.Changed += (_, _) => raised++;

        row.Stat = StatKind.Dodge;

        Assert.Equal(1, raised);
    }

    [Fact]
    public void Stats_Offered_AreTheChangeableOnes()
    {
        var row = StatusPartRow.New();

        Assert.Equal(StatusModifiers.ChangeableStats, row.Stats);
    }
}
