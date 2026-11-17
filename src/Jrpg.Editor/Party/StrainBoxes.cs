using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Resources;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Party;

/// <summary>
/// One resource's strain (ADR 0024) as the Resources tab types it, on or off: the resource whose start its missing
/// points lower, by id as typed, and how much each missing point takes off. Data says whether this resource is lasting,
/// as strain asks, and whether the id names another resource of the class that isn't. The id's combo lists the other
/// resources' ids the class offers.
/// </summary>
internal sealed partial class StrainBoxes : ResourceRuleBoxes<StrainRule>
{
    // What the boxes show while the rule is off (decision 1016): the lowest amount Data takes, and no resource named
    // until one is chosen, as no class's resource is a safe guess.
    private static readonly StrainRule _blank = new("", 1);

    private IReadOnlyList<string> _classmateIds = [];

    private StrainBoxes(StrainRule? rule)
    {
        var shown = rule ?? _blank;
        IsOn = rule is not null;
        LowersStartOf = shown.LowersStartOf;
        PerMissingPointText = TypedNumber.Text(shown.PerMissingPoint);
    }

    /// <param name="rule">The resource's rule as the draft holds it; null when it has none.</param>
    public static StrainBoxes From(StrainRule? rule) => new(rule);

    /// <summary>The id of the resource whose start is lowered, as typed; Data's message names a wrong one.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LowersStartOfChoices))]
    public partial string LowersStartOf { get; set; }

    /// <summary>What the resource's combo lists: the ids offered, and <see cref="LowersStartOf"/> as typed.</summary>
    public IReadOnlyList<string> LowersStartOfChoices => ResourceIdChoices.Of(_classmateIds, LowersStartOf);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    public partial string PerMissingPointText { get; set; }

    protected override string? BoxesProblem => TypedNumber.ProblemOf("Strain per missing point", PerMissingPointText);

    /// <summary>
    /// Takes the other boxes' state as typed, a half-typed number included: a duplicated resource's (decision 585).
    /// </summary>
    public void CopyFrom(StrainBoxes other)
    {
        IsOn = other.IsOn;
        LowersStartOf = other.LowersStartOf;
        PerMissingPointText = other.PerMissingPointText;
    }

    /// <summary>Lists the class's other resources' ids in the combo; never a typed change.</summary>
    /// <param name="classmateIds">The ids as typed, none empty.</param>
    public void OfferIds(IReadOnlyList<string> classmateIds)
    {
        _classmateIds = classmateIds;
        OnPropertyChanged(nameof(LowersStartOfChoices));
    }

    protected override StrainRule RuleAsTyped() => new(LowersStartOf, TypedNumber.Parse(PerMissingPointText)!.Value);

    partial void OnLowersStartOfChanged(string value) => OnTyped();

    partial void OnPerMissingPointTextChanged(string value) => OnTyped();
}
