using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Stats;
using Jrpg.Core.Statuses;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Statuses;

/// <summary>
/// One part of a status as the Statuses page types it (decision 984): its kind, its amount labelled by the kind, the
/// type a damage tick meets resistance as and the stat a stat change changes. A box the kind doesn't read keeps its
/// value, so switching back finds it, and is never written. <see cref="Problem"/> is only what keeps the part from
/// being built; an amount outside its kind's range or a kind listed twice is Data's rule, shown by the list.
/// </summary>
internal sealed partial class StatusPartRow : ObservableObject
{
    // A part as Add starts it: the least damage tick Data takes (row 6's decision 797).
    private const int NewAmount = 1;

    /// <param name="kind">What the part does.</param>
    /// <param name="amountText">The amount as typed.</param>
    /// <param name="damageType">A damage tick's type; null ignores resistances (decision 803).</param>
    /// <param name="stat">A stat change's stat; null while none is picked.</param>
    public StatusPartRow(StatusPartKind kind, string amountText, DamageType? damageType, StatKind? stat)
    {
        Kind = kind;
        AmountText = amountText;
        DamageType = damageType;
        Stat = stat;
    }

    /// <summary>
    /// A kind, the amount, the type or the stat typed; never the label's or the problem's own notices.
    /// </summary>
    public event EventHandler? Changed;

    public static StatusPartRow From(StatusPart part) =>
        new(part.Kind, TypedNumber.Text(part.Amount), part.DamageType, part.Stat);

    /// <summary>A row as Add starts it: a damage tick of 1 HP that ignores resistances.</summary>
    public static StatusPartRow New() => new(StatusPartKind.DamageEachTurn, TypedNumber.Text(NewAmount), null, null);

    public IReadOnlyList<StatusPartKind> Kinds { get; } = Enum.GetValues<StatusPartKind>();

    /// <summary>None first, then Core's types: a typeless tick ignores resistances (decision 803).</summary>
    public IReadOnlyList<DamageTypeChoice> DamageTypeChoices { get; } = DamageTypeChoice.All;

    /// <summary>The stats Core lets a stat change name (decision 807).</summary>
    public IReadOnlyList<StatKind> Stats { get; } = StatusModifiers.ChangeableStats;

    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(AmountLabel),
        nameof(HasAmount),
        nameof(CanTypeDamageType),
        nameof(CanTypeStat),
        nameof(Problem))]
    public partial StatusPartKind Kind { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    public partial string AmountText { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedDamageTypeChoice))]
    public partial DamageType? DamageType { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    public partial StatKind? Stat { get; set; }

    /// <summary>
    /// The Damage type box's entry for <see cref="DamageType"/>: the box binds its item, since a null value would leave
    /// the none entry unselected.
    /// </summary>
    public DamageTypeChoice SelectedDamageTypeChoice
    {
        get => DamageTypeChoices.Single(choice => choice.Type == DamageType);
        set => DamageType = value.Type;
    }

    /// <summary>
    /// What the amount means for the kind; empty for a kind without one (a skipped turn, drawn attacks).
    /// </summary>
    public string AmountLabel => Kind switch
    {
        StatusPartKind.DamageEachTurn => "HP lost each turn",
        StatusPartKind.HealEachTurn => "HP regained each turn",
        StatusPartKind.SkipTurn or StatusPartKind.DrawAttacks => "",
        StatusPartKind.ResistanceChange => "points on every resistance",
        StatusPartKind.DamageTakenChange => "% more taken from each hit",
        StatusPartKind.DamageDealtChange => "% on the damage dealt",
        StatusPartKind.StatChange => "points on the stat",
        _ => throw new InvalidOperationException($"No amount label for {Kind}."),
    };

    public bool HasAmount => AmountLabel.Length > 0;

    /// <summary>Only a damage tick meets resistance (decision 803).</summary>
    public bool CanTypeDamageType => Kind == StatusPartKind.DamageEachTurn;

    public bool CanTypeStat => Kind == StatusPartKind.StatChange;

    /// <summary>
    /// An amount the kind reads that isn't a whole number, or a stat change with no stat; null when it can be built.
    /// </summary>
    public string? Problem =>
        (HasAmount ? TypedNumber.ProblemOf("Amount", AmountText) : null)
        ?? (CanTypeStat && Stat is null ? "A stat change must name its stat." : null);

    /// <summary>
    /// The part as typed: amount 0 for a kind without one, the type and the stat only where the kind reads them.
    /// </summary>
    /// <exception cref="InvalidOperationException">There is a <see cref="Problem"/>.</exception>
    public StatusPart ToPart() =>
        Problem is { } problem
            ? throw new InvalidOperationException(problem)
            : new StatusPart(
                Kind,
                HasAmount ? TypedNumber.Parse(AmountText)!.Value : 0,
                CanTypeDamageType ? DamageType : null,
                CanTypeStat ? Stat : null);

    partial void OnKindChanged(StatusPartKind value) => Changed?.Invoke(this, EventArgs.Empty);

    partial void OnAmountTextChanged(string value) => Changed?.Invoke(this, EventArgs.Empty);

    partial void OnDamageTypeChanged(DamageType? value) => Changed?.Invoke(this, EventArgs.Empty);

    partial void OnStatChanged(StatKind? value) => Changed?.Invoke(this, EventArgs.Empty);
}
