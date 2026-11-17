using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Resources;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Party;

/// <summary>
/// One resource's Will scaling (decision 319) as the Resources tab types it, on or off: the Will one point takes, and
/// what the holder's points do to the resource.
/// </summary>
internal sealed partial class WillScalingBoxes : ResourceRuleBoxes<WillScaling>
{
    // What the boxes show while the rule is off, so ticking it starts on a rule Data takes on any resource: the lowest
    // Will per point, and a raised max, which a lasting resource takes too (decision 1016).
    private static readonly WillScaling _blank = new(1, WillEffect.RaiseMax);

    private WillScalingBoxes(WillScaling? rule)
    {
        var shown = rule ?? _blank;
        IsOn = rule is not null;
        WillPerPointText = TypedNumber.Text(shown.WillPerPoint);
        Effect = shown.Effect;
    }

    /// <param name="rule">The resource's rule as the draft holds it; null when it has none.</param>
    public static WillScalingBoxes From(WillScaling? rule) => new(rule);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    public partial string WillPerPointText { get; set; }

    [ObservableProperty]
    public partial WillEffect Effect { get; set; }

    protected override string? BoxesProblem => TypedNumber.ProblemOf("Will per point", WillPerPointText);

    /// <summary>
    /// Takes the other boxes' state as typed, a half-typed number included: a duplicated resource's (decision 585).
    /// </summary>
    public void CopyFrom(WillScalingBoxes other)
    {
        IsOn = other.IsOn;
        WillPerPointText = other.WillPerPointText;
        Effect = other.Effect;
    }

    protected override WillScaling RuleAsTyped() => new(TypedNumber.Parse(WillPerPointText)!.Value, Effect);

    partial void OnWillPerPointTextChanged(string value) => OnTyped();

    partial void OnEffectChanged(WillEffect value) => OnTyped();
}
