using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Resources;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Party;

/// <summary>
/// A resource's gains as the Resources tab types them (decision 950): seven boxes, one per trigger, and the share of
/// max HP that makes a hit heavy, in the form's order. Each keeps its text as typed (<see cref="TypedNumber"/>), so a
/// half-typed number is reported, never kept; Data says whether the heavy-hit share is within its range.
/// </summary>
internal sealed partial class ResourceGainsBoxes : ObservableObject
{
    /// <param name="gains">The numbers the boxes start on.</param>
    public ResourceGainsBoxes(ResourceGains gains)
    {
        PerTurnText = TypedNumber.Text(gains.PerTurn);
        PerHitTakenText = TypedNumber.Text(gains.PerHitTaken);
        PerHeavyHitText = TypedNumber.Text(gains.PerHeavyHit);
        HeavyHitPercentText = TypedNumber.Text(gains.HeavyHitPercent);
        PerHitLandedText = TypedNumber.Text(gains.PerHitLanded);
        PerAllyFallenText = TypedNumber.Text(gains.PerAllyFallen);
        PerEnemyFallenText = TypedNumber.Text(gains.PerEnemyFallen);
        PerBattleFoughtText = TypedNumber.Text(gains.PerBattleFought);
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem), nameof(Typed))]
    public partial string PerTurnText { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem), nameof(Typed))]
    public partial string PerHitTakenText { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem), nameof(Typed))]
    public partial string PerHeavyHitText { get; set; }

    /// <summary>The share of max HP, in percent, that makes a hit heavy.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem), nameof(Typed))]
    public partial string HeavyHitPercentText { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem), nameof(Typed))]
    public partial string PerHitLandedText { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem), nameof(Typed))]
    public partial string PerAllyFallenText { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem), nameof(Typed))]
    public partial string PerEnemyFallenText { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem), nameof(Typed))]
    public partial string PerBattleFoughtText { get; set; }

    /// <summary>
    /// The first box, in the form's order, that isn't a whole number: "Per hit taken must be a whole number (was
    /// 'x')."; null when every box reads.
    /// </summary>
    public string? Problem =>
        TypedNumber.ProblemOf("Per turn", PerTurnText)
        ?? TypedNumber.ProblemOf("Per hit taken", PerHitTakenText)
        ?? TypedNumber.ProblemOf("Per heavy hit", PerHeavyHitText)
        ?? TypedNumber.ProblemOf("Heavy hit share", HeavyHitPercentText)
        ?? TypedNumber.ProblemOf("Per hit landed", PerHitLandedText)
        ?? TypedNumber.ProblemOf("Per ally fallen", PerAllyFallenText)
        ?? TypedNumber.ProblemOf("Per enemy fallen", PerEnemyFallenText)
        ?? TypedNumber.ProblemOf("Per battle fought", PerBattleFoughtText);

    /// <summary>The gains as typed; null while a box doesn't read, so a reader never meets a throw.</summary>
    public ResourceGains? Typed =>
        Problem is not null
            ? null
            : new ResourceGains
            {
                PerTurn = NumberIn(PerTurnText),
                PerHitTaken = NumberIn(PerHitTakenText),
                PerHeavyHit = NumberIn(PerHeavyHitText),
                HeavyHitPercent = NumberIn(HeavyHitPercentText),
                PerHitLanded = NumberIn(PerHitLandedText),
                PerAllyFallen = NumberIn(PerAllyFallenText),
                PerEnemyFallen = NumberIn(PerEnemyFallenText),
                PerBattleFought = NumberIn(PerBattleFoughtText),
            };

    /// <summary>
    /// Takes the other boxes' texts as typed, a half-typed number included: a duplicated resource's (decision 585).
    /// </summary>
    public void CopyFrom(ResourceGainsBoxes other)
    {
        PerTurnText = other.PerTurnText;
        PerHitTakenText = other.PerHitTakenText;
        PerHeavyHitText = other.PerHeavyHitText;
        HeavyHitPercentText = other.HeavyHitPercentText;
        PerHitLandedText = other.PerHitLandedText;
        PerAllyFallenText = other.PerAllyFallenText;
        PerEnemyFallenText = other.PerEnemyFallenText;
        PerBattleFoughtText = other.PerBattleFoughtText;
    }

    private static int NumberIn(string text) => TypedNumber.Parse(text)!.Value;
}
