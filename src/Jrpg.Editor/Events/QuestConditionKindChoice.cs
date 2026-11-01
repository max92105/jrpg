using Jrpg.Core.Quests;

namespace Jrpg.Editor.Events;

/// <summary>One condition kind as the dialogs' box offers it: the kind and the words shown for it (ADR 0033).</summary>
internal sealed record QuestConditionKindChoice(QuestConditionKind Kind, string Label)
{
    /// <summary>Every kind, in the enum's order.</summary>
    public static IReadOnlyList<QuestConditionKindChoice> All { get; } =
    [
        new(QuestConditionKind.NotStarted, "not started"),
        new(QuestConditionKind.Active, "active"),
        new(QuestConditionKind.AtStage, "at stage"),
        new(QuestConditionKind.ReachedStage, "reached stage"),
        new(QuestConditionKind.Completed, "completed"),
    ];
}
