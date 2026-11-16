using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Battle;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Battle;

/// <summary>
/// One rule of an enemy as the Tactics tab types it (decision 849): its When choice and number, its skill, its target,
/// its weight and whether it is a priority rule (decision 841), and the numbered sentence the list shows. The number
/// box keeps what was typed while its kind reads none, so switching back finds it, and is never written then.
/// <see cref="Problem"/> is only what keeps the action from being built; a percent outside 1..99 or a target that
/// doesn't fit the skill is Data's rule, shown by the list.
/// </summary>
internal sealed partial class TacticRuleRow : ObservableObject
{
    // A new rule's weight (decision 859): the least share of the pick Data takes.
    private const int NewWeight = 1;

    // As a new rule starts (decision 859): always, the attack, automatic, nothing typed in the hidden number box.
    private TacticRuleRow()
    {
        When = TacticConditionChoice.Of(null);
        NumberText = "";
        SkillId = "";
        On = TacticTargetChoice.Of(null);
        WeightText = TypedNumber.Text(NewWeight);
    }

    // A percent is 0 only where the kind reads none (Data refuses 0 where one is read), so its box starts empty there.
    public static TacticRuleRow From(EnemyAction action) => new()
    {
        When = TacticConditionChoice.Of(action.When?.Kind),
        NumberText = action.When is { Percent: > 0 and var percent } ? TypedNumber.Text(percent) : "",
        SkillId = action.SkillId ?? "",
        On = TacticTargetChoice.Of(action.On),
        WeightText = TypedNumber.Text(action.Weight),
        IsPriority = action.IsPriority,
    };

    /// <summary>A rule as Add starts it: always, the attack, automatic, weight 1 (decision 859).</summary>
    public static TacticRuleRow New() => new();

    /// <summary>
    /// A copy of the boxes as typed, a problem included, so a copy never throws; its list numbers it.
    /// </summary>
    public static TacticRuleRow CopyOf(TacticRuleRow row) => new()
    {
        When = row.When,
        NumberText = row.NumberText,
        SkillId = row.SkillId,
        On = row.On,
        WeightText = row.WeightText,
        IsPriority = row.IsPriority,
    };

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNumberShown), nameof(NumberUnit), nameof(Sentence), nameof(Problem))]
    public partial TacticConditionChoice When { get; set; }

    /// <summary>The number the When kind reads, as typed.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Sentence), nameof(Problem))]
    public partial string NumberText { get; set; }

    /// <summary>The skill's id; empty for the attack, as the Use box's first entry is.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Sentence))]
    public partial string SkillId { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Sentence))]
    public partial TacticTargetChoice On { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Sentence), nameof(Problem))]
    public partial string WeightText { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Sentence))]
    public partial bool IsPriority { get; set; }

    /// <summary>
    /// Its place in its list, set by the list: Data's <c>tactics[i]</c>, so a problem's rule is found.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Sentence))]
    public partial int Index { get; set; }

    public bool IsNumberShown => Number != TacticConditionNumber.None;

    /// <summary>What the number box counts; empty while the kind reads no number.</summary>
    public string NumberUnit => Number switch
    {
        TacticConditionNumber.None => "",
        TacticConditionNumber.Percent => "%",
        _ => throw new InvalidOperationException($"No unit for {Number}."),
    };

    /// <summary>
    /// "2. When an ally's HP is below 50 %: field_dressing on that one, weight 3", the boxes as typed, ", priority"
    /// after a priority rule's weight; the skill by id, since the row doesn't hold the catalog (decision 853).
    /// </summary>
    public string Sentence =>
        $"{Index}. {WhenWords}: {(SkillId.Length == 0 ? "attack" : SkillId)} on {On.Label}, weight {WeightText}"
        + (IsPriority ? ", priority" : "");

    /// <summary>
    /// The weight, then a number the kind reads, that isn't a whole number; null when it can be built.
    /// </summary>
    public string? Problem =>
        TypedNumber.ProblemOf("Weight", WeightText) ?? (IsNumberShown ? TypedNumber.ProblemOf("Percent", NumberText) : null);

    /// <summary>The action as typed: the attack's id left out, a percent of 0 for a kind that reads none.</summary>
    /// <exception cref="InvalidOperationException">There is a <see cref="Problem"/>.</exception>
    public EnemyAction ToAction() =>
        Problem is { } problem
            ? throw new InvalidOperationException(problem)
            : new EnemyAction
            {
                SkillId = SkillId.Length == 0 ? null : SkillId,
                Weight = TypedNumber.Parse(WeightText)!.Value,
                When = When.Kind is { } kind ? new TacticCondition(kind, TypedPercent) : null,
                On = On.Target,
                IsPriority = IsPriority,
            };

    private TacticConditionNumber Number =>
        When.Kind is { } kind ? TacticConditionKinds.ShapeOf(kind).Number : TacticConditionNumber.None;

    private string WhenWords => (When.Kind, IsNumberShown) switch
    {
        (null, _) => "Always",
        (_, true) => $"When {When.Label} {ShownNumber} {NumberUnit}",
        _ => $"When {When.Label}",
    };

    // The box as typed, trimmed; "?" while empty, its problem line naming it.
    private string ShownNumber => NumberText.Trim() is { Length: > 0 } typed ? typed : "?";

    private int TypedPercent => IsNumberShown ? TypedNumber.Parse(NumberText)!.Value : 0;
}
