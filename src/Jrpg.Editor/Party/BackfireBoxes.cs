using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Resources;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Party;

/// <summary>
/// One resource's backfire (decision 194) as the Resources tab types it, on or off: the chance per point held, and the
/// damage, a share of the caster's max HP, both in percent.
/// </summary>
internal sealed partial class BackfireBoxes : ResourceRuleBoxes<BackfireRule>
{
    // What the boxes show while the rule is off, so ticking it starts on a rule Data takes (decision 1016): the lowest
    // percent in both boxes.
    private static readonly BackfireRule _blank = new(1, 1);

    private BackfireBoxes(BackfireRule? rule)
    {
        var shown = rule ?? _blank;
        IsOn = rule is not null;
        ChancePerPointText = TypedNumber.Text(shown.ChancePerPointPercent);
        DamagePercentText = TypedNumber.Text(shown.DamagePercentOfMaxHp);
    }

    /// <param name="rule">The resource's rule as the draft holds it; null when it has none.</param>
    public static BackfireBoxes From(BackfireRule? rule) => new(rule);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    public partial string ChancePerPointText { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    public partial string DamagePercentText { get; set; }

    protected override string? BoxesProblem =>
        TypedNumber.ProblemOf("Backfire chance per point", ChancePerPointText)
        ?? TypedNumber.ProblemOf("Backfire damage", DamagePercentText);

    /// <summary>
    /// Takes the other boxes' state as typed, a half-typed number included: a duplicated resource's (decision 585).
    /// </summary>
    public void CopyFrom(BackfireBoxes other)
    {
        IsOn = other.IsOn;
        ChancePerPointText = other.ChancePerPointText;
        DamagePercentText = other.DamagePercentText;
    }

    protected override BackfireRule RuleAsTyped() =>
        new(TypedNumber.Parse(ChancePerPointText)!.Value, TypedNumber.Parse(DamagePercentText)!.Value);

    partial void OnChancePerPointTextChanged(string value) => OnTyped();

    partial void OnDamagePercentTextChanged(string value) => OnTyped();
}
