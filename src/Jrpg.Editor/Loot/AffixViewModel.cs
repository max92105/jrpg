using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Items;
using Jrpg.Core.Loot;
using Jrpg.Core.Stats;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Objects;

namespace Jrpg.Editor.Loot;

/// <summary>
/// One affix as the Affixes page edits it (decision 884): its id (typed while the affix is new, fixed once the draft
/// holds it, decision 582), stat, range, weight and gold value as typed, and a box per equipment kind it may roll on,
/// written over the record it came from. <see cref="Problem"/> is only the typed id's and the number boxes'; the rest
/// (nonzero values, the sign rule, at least one kind, the bound) is Data's rules, which the list shows.
/// </summary>
internal sealed partial class AffixViewModel : ObservableObject
{
    // A new affix's starting record (decision 885): the developer retypes each; with no kind ticked, Data's problem
    // names the kinds until one is.
    private const StatKind NewStat = StatKind.Power;
    private const int NewRangeEnd = 1;
    private const int NewWeight = 10;
    private const int NewGoldValue = 20;

    private readonly AffixDefinition _source;

    // The boxes start on the source's fields: a new affix's id is empty.
    private AffixViewModel(AffixDefinition source)
    {
        _source = source;
        Id = source.Id;
        Stat = source.Stat;
        MinText = TypedNumber.Text(source.Min);
        MaxText = TypedNumber.Text(source.Max);
        WeightText = TypedNumber.Text(source.Weight);
        GoldValueText = TypedNumber.Text(source.GoldValue);
        Kinds = [.. Enum.GetValues<EquipmentKind>().Select(kind => new EquipmentKindToggle(kind, source.Kinds.Contains(kind)))];
        foreach (var toggle in Kinds)
        {
            toggle.PropertyChanged += OnKindToggled;
        }
    }

    /// <summary>
    /// A typed box changed or a kind box was ticked or cleared: what the list's problem line listens to, never the
    /// label's or the problem's own notices, so the list runs Data's check once per change.
    /// </summary>
    public event EventHandler? Changed;

    /// <param name="affix">The record as the draft holds it.</param>
    public static AffixViewModel From(AffixDefinition affix) => new(affix);

    /// <summary>An affix added this session: its id left to type, the rest from the tunable starting record.</summary>
    public static AffixViewModel New() =>
        new(new AffixDefinition
        {
            Id = "",
            Stat = NewStat,
            Min = NewRangeEnd,
            Max = NewRangeEnd,
            Kinds = [],
            Weight = NewWeight,
            GoldValue = NewGoldValue,
        })
        {
            IsNew = true,
        };

    /// <summary>
    /// True while the affix was added this session: the only time its id can be typed (decision 582).
    /// </summary>
    public bool IsNew { get; private init; }

    /// <summary>
    /// Every stat, the affix-only ones included (ADR 0030), in <see cref="StatKind"/> order: the box must hold any stat
    /// a saved affix names.
    /// </summary>
    public IReadOnlyList<StatKind> StatChoices { get; } = Enum.GetValues<StatKind>();

    /// <summary>
    /// One box per equipment kind, in <see cref="EquipmentKind"/> order, ticked for the kinds it rolls on.
    /// </summary>
    public IReadOnlyList<EquipmentKindToggle> Kinds { get; }

    /// <summary>The id as typed; the page lets it be typed only while <see cref="IsNew"/>.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Label), nameof(Problem))]
    public partial string Id { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Label))]
    public partial StatKind Stat { get; set; }

    /// <summary>The lowest value a roll gives, as typed, in the stat's bonus unit (ADR 0026).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Label), nameof(Problem))]
    public partial string MinText { get; set; }

    /// <summary>The highest value a roll gives, as typed.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Label), nameof(Problem))]
    public partial string MaxText { get; set; }

    /// <summary>Its chance against the other affixes a piece may roll, as typed.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    public partial string WeightText { get; set; }

    /// <summary>What the affix adds to a piece's price in a shop, as typed (decision 871).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    public partial string GoldValueText { get; set; }

    /// <summary>The list's line: "power: Power 2..5", the range as typed; "(new affix)" while it has no id.</summary>
    public string Label => Id.Length > 0 ? $"{Id}: {Stat} {MinText.Trim()}..{MaxText.Trim()}" : "(new affix)";

    /// <summary>
    /// Why the affix can't be built: the typed id while new, then the first box, in the page's order, that isn't a
    /// whole number; null when it can.
    /// </summary>
    public string? Problem =>
        (IsNew ? ObjectIds.ProblemOf(Id, []) : null)
        ?? TypedNumber.ProblemOf("Min", MinText)
        ?? TypedNumber.ProblemOf("Max", MaxText)
        ?? TypedNumber.ProblemOf("Weight", WeightText)
        ?? TypedNumber.ProblemOf("Gold value", GoldValueText);

    /// <summary>
    /// A new affix copying this one's boxes as typed, a half-typed number included, its id left to type (decision 585).
    /// </summary>
    public AffixViewModel Duplicate() =>
        new(RecordWithTypedChoices() with { Id = "" })
        {
            IsNew = true,
            MinText = MinText,
            MaxText = MaxText,
            WeightText = WeightText,
            GoldValueText = GoldValueText,
        };

    /// <summary>The record it came from with the typed fields over it, the ticked kinds in enum order.</summary>
    /// <exception cref="InvalidOperationException">There is a <see cref="Problem"/>.</exception>
    public AffixDefinition ToAffix() =>
        Problem is { } problem
            ? throw new InvalidOperationException(problem)
            : RecordWithTypedChoices() with
            {
                Min = TypedNumber.Parse(MinText)!.Value,
                Max = TypedNumber.Parse(MaxText)!.Value,
                Weight = TypedNumber.Parse(WeightText)!.Value,
                GoldValue = TypedNumber.Parse(GoldValueText)!.Value,
            };

    // The source's numbers are kept, not the boxes': a duplicate copies them as typed, and only ToAffix reads them.
    private AffixDefinition RecordWithTypedChoices() =>
        _source with
        {
            Id = Id,
            Stat = Stat,
            Kinds = [.. Kinds.Where(toggle => toggle.IsChosen).Select(toggle => toggle.Kind)],
        };

    private void OnKindToggled(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(EquipmentKindToggle.IsChosen))
        {
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }

    partial void OnIdChanged(string value) => Changed?.Invoke(this, EventArgs.Empty);

    partial void OnStatChanged(StatKind value) => Changed?.Invoke(this, EventArgs.Empty);

    partial void OnMinTextChanged(string value) => Changed?.Invoke(this, EventArgs.Empty);

    partial void OnMaxTextChanged(string value) => Changed?.Invoke(this, EventArgs.Empty);

    partial void OnWeightTextChanged(string value) => Changed?.Invoke(this, EventArgs.Empty);

    partial void OnGoldValueTextChanged(string value) => Changed?.Invoke(this, EventArgs.Empty);
}
