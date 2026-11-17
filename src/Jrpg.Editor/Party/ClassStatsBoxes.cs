using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Party;
using Jrpg.Core.Stats;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Party;

/// <summary>
/// A class's stats as the Classes page types them (decision 947): eight boxes, max HP, speed, accuracy and the five
/// primaries, used once for the base at level 1 and once for the growth per level. Each keeps its text as typed
/// (<see cref="TypedNumber"/>), so a half-typed number is reported, never kept; Data says which numbers are too low.
/// </summary>
internal sealed partial class ClassStatsBoxes : ObservableObject
{
    private readonly string _label;

    /// <param name="stats">The numbers the boxes start on.</param>
    /// <param name="label">"Base" or "Growth": the problem's first word, naming which boxes it is in.</param>
    public ClassStatsBoxes(ClassStats stats, string label)
    {
        _label = label;
        MaxHpText = TypedNumber.Text(stats.MaxHp);
        SpeedText = TypedNumber.Text(stats.Speed);
        AccuracyText = TypedNumber.Text(stats.Accuracy);
        PowerText = TypedNumber.Text(stats.Primaries.Power);
        AgilityText = TypedNumber.Text(stats.Primaries.Agility);
        IntelligenceText = TypedNumber.Text(stats.Primaries.Intelligence);
        ConstitutionText = TypedNumber.Text(stats.Primaries.Constitution);
        WillText = TypedNumber.Text(stats.Primaries.Will);
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem), nameof(Typed))]
    public partial string MaxHpText { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem), nameof(Typed))]
    public partial string SpeedText { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem), nameof(Typed))]
    public partial string AccuracyText { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem), nameof(Typed))]
    public partial string PowerText { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem), nameof(Typed))]
    public partial string AgilityText { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem), nameof(Typed))]
    public partial string IntelligenceText { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem), nameof(Typed))]
    public partial string ConstitutionText { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem), nameof(Typed))]
    public partial string WillText { get; set; }

    /// <summary>
    /// The first box, in the page's order, that isn't a whole number: "Growth will must be a whole number (was 'x').";
    /// null when every box reads.
    /// </summary>
    public string? Problem =>
        TypedNumber.ProblemOf($"{_label} max HP", MaxHpText)
        ?? TypedNumber.ProblemOf($"{_label} speed", SpeedText)
        ?? TypedNumber.ProblemOf($"{_label} accuracy", AccuracyText)
        ?? TypedNumber.ProblemOf($"{_label} power", PowerText)
        ?? TypedNumber.ProblemOf($"{_label} agility", AgilityText)
        ?? TypedNumber.ProblemOf($"{_label} intelligence", IntelligenceText)
        ?? TypedNumber.ProblemOf($"{_label} constitution", ConstitutionText)
        ?? TypedNumber.ProblemOf($"{_label} will", WillText);

    /// <summary>
    /// The stats as typed; null while a box doesn't read, so a reader such as the Levels tab never meets a throw.
    /// </summary>
    public ClassStats? Typed =>
        Problem is not null
            ? null
            : new ClassStats(NumberIn(MaxHpText), NumberIn(SpeedText))
            {
                Accuracy = NumberIn(AccuracyText),
                Primaries = new PrimaryStats(
                    NumberIn(PowerText),
                    NumberIn(AgilityText),
                    NumberIn(IntelligenceText),
                    NumberIn(ConstitutionText),
                    NumberIn(WillText)),
            };

    /// <summary>
    /// Takes the other boxes' texts as typed, a half-typed number included: a duplicate's (decision 585).
    /// </summary>
    public void CopyFrom(ClassStatsBoxes other)
    {
        MaxHpText = other.MaxHpText;
        SpeedText = other.SpeedText;
        AccuracyText = other.AccuracyText;
        PowerText = other.PowerText;
        AgilityText = other.AgilityText;
        IntelligenceText = other.IntelligenceText;
        ConstitutionText = other.ConstitutionText;
        WillText = other.WillText;
    }

    private static int NumberIn(string text) => TypedNumber.Parse(text)!.Value;
}
