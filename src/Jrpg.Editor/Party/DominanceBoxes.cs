using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Resources;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Party;

/// <summary>
/// One resource's dominance (decision 195) as the Resources tab types it, on or off: the rival force, by id as typed,
/// and the bonus in percent its skills land with while it dominates. Data says whether the rival is another resource of
/// the class naming this one back. The rival's combo lists the ids the class offers.
/// </summary>
internal sealed partial class DominanceBoxes : ResourceRuleBoxes<DominanceRule>
{
    // What the boxes show while the rule is off (decision 1016): the lowest bonus Data takes, and no rival named until
    // one is chosen, as no class's resource is a safe guess.
    private static readonly DominanceRule _blank = new("", 0);

    private IReadOnlyList<string> _classmateIds = [];

    private DominanceBoxes(DominanceRule? rule)
    {
        var shown = rule ?? _blank;
        IsOn = rule is not null;
        Over = shown.Over;
        BonusPercentText = TypedNumber.Text(shown.BonusPercent);
    }

    /// <param name="rule">The resource's rule as the draft holds it; null when it has none.</param>
    public static DominanceBoxes From(DominanceRule? rule) => new(rule);

    /// <summary>The rival force's id, as typed; Data's message names a wrong one.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(OverChoices))]
    public partial string Over { get; set; }

    /// <summary>What the rival's combo lists: the ids offered, and <see cref="Over"/> as typed.</summary>
    public IReadOnlyList<string> OverChoices => ResourceIdChoices.Of(_classmateIds, Over);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    public partial string BonusPercentText { get; set; }

    protected override string? BoxesProblem => TypedNumber.ProblemOf("Dominance bonus", BonusPercentText);

    /// <summary>
    /// Takes the other boxes' state as typed, a half-typed number included: a duplicated resource's (decision 585).
    /// </summary>
    public void CopyFrom(DominanceBoxes other)
    {
        IsOn = other.IsOn;
        Over = other.Over;
        BonusPercentText = other.BonusPercentText;
    }

    /// <summary>Lists the class's other resources' ids in the combo; never a typed change.</summary>
    /// <param name="classmateIds">The ids as typed, none empty.</param>
    public void OfferIds(IReadOnlyList<string> classmateIds)
    {
        _classmateIds = classmateIds;
        OnPropertyChanged(nameof(OverChoices));
    }

    protected override DominanceRule RuleAsTyped() => new(Over, TypedNumber.Parse(BonusPercentText)!.Value);

    partial void OnOverChanged(string value) => OnTyped();

    partial void OnBonusPercentTextChanged(string value) => OnTyped();
}
