using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Quests;

namespace Jrpg.Editor.Events;

/// <summary>
/// An event's quest condition as the dialogs edit it (ADR 0033): the quest (empty for always), the kind, and the stage
/// the two stage kinds name, offered from the chosen quest's stages in order; choosing another quest clears the stage.
/// </summary>
internal sealed partial class QuestGateViewModel : ObservableObject
{
    private readonly IReadOnlyList<QuestStageChoice> _stages;

    /// <param name="condition">The event's condition as it is; null for always.</param>
    /// <param name="stages">
    /// Every loaded quest's stages, in each quest's order (<see cref="StepChoices.QuestStages"/>).
    /// </param>
    public QuestGateViewModel(QuestCondition? condition, IReadOnlyList<QuestStageChoice> stages)
    {
        _stages = stages;
        QuestId = condition?.QuestId ?? "";
        Kind = condition?.Kind ?? QuestConditionKind.NotStarted;
        StageId = condition?.StageId ?? "";
    }

    /// <summary>The quest the event waits on; empty for always.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Label), nameof(Problem), nameof(Stages), nameof(HasQuest), nameof(CanChooseStage))]
    public partial string QuestId { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Label), nameof(Problem), nameof(NeedsStage), nameof(CanChooseStage))]
    public partial QuestConditionKind Kind { get; set; }

    /// <summary>The stage the two stage kinds name; empty until picked.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Label), nameof(Problem))]
    public partial string StageId { get; set; }

    public bool HasQuest => QuestId.Length > 0;

    /// <summary>Whether the kind names a stage: at a stage, or reached one.</summary>
    public bool NeedsStage => Kind is QuestConditionKind.AtStage or QuestConditionKind.ReachedStage;

    /// <summary>Whether the stage box takes a pick: a quest is chosen and its kind names a stage.</summary>
    public bool CanChooseStage => HasQuest && NeedsStage;

    /// <summary>
    /// The chosen quest's stage ids in its order, after the empty one (none picked yet); only that one with no quest.
    /// </summary>
    public IReadOnlyList<string> Stages => ["", .. _stages.Where(stage => stage.QuestId == QuestId).Select(stage => stage.StageId)];

    /// <summary>Why the condition can't be kept as set; null when it can.</summary>
    public string? Problem =>
        !HasQuest || !NeedsStage || StageId.Length > 0 ? null
        : Kind == QuestConditionKind.AtStage ? "Choose the stage the quest must be at."
        : "Choose the stage the quest must have reached.";

    /// <summary>
    /// What the event's summary appends, such as " when errand reached deliver"; empty with no quest.
    /// </summary>
    public string Label => !HasQuest ? "" : Kind switch
    {
        QuestConditionKind.NotStarted => $" when {QuestId} is not started",
        QuestConditionKind.Active => $" when {QuestId} is active",
        QuestConditionKind.AtStage => $" when {QuestId} is at {StageOrQuestionMark}",
        QuestConditionKind.ReachedStage => $" when {QuestId} reached {StageOrQuestionMark}",
        QuestConditionKind.Completed => $" when {QuestId} is completed",
        _ => throw new InvalidOperationException($"Condition kind {Kind} has no label."),
    };

    /// <returns>Null for always; a stage left from another kind isn't written.</returns>
    /// <exception cref="InvalidOperationException">The condition has a <see cref="Problem"/>.</exception>
    public QuestCondition? ToCondition() =>
        !HasQuest ? null
        : Problem is { } problem ? throw new InvalidOperationException(problem)
        : new QuestCondition(QuestId, Kind, NeedsStage ? StageId : null);

    // The summary shows the half-set condition with a mark until the stage is picked; Problem says what is missing.
    private string StageOrQuestionMark => StageId.Length > 0 ? StageId : "?";

    // A stage belongs to one quest, so another quest starts without one.
    partial void OnQuestIdChanged(string value) => StageId = "";
}
