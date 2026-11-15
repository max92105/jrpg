using System.Windows.Input;

namespace Jrpg.Editor.Events;

/// <summary>
/// The Add step drop-down's entries (decision 729): every step kind once, under its group's title, in the menu's order.
/// A kind added to <see cref="EventStepKind"/> without an entry here fails a test.
/// </summary>
internal static class StepKindMenu
{
    /// <param name="addStep">Adds a step of the <see cref="EventStepKind"/> it is given.</param>
    public static IReadOnlyList<DropDownChoice> Choices(ICommand addStep) =>
    [
        DropDownChoice.Title("TALK"),
        new("Dialogue", addStep, EventStepKind.Dialogue),
        new("Voice", addStep, EventStepKind.Voice),
        new("Cinematic", addStep, EventStepKind.Cinematic),
        DropDownChoice.Title("WORLD"),
        new("Give item", addStep, EventStepKind.GiveItem),
        new("Swap object", addStep, EventStepKind.SwapObject),
        new("Warp", addStep, EventStepKind.Warp),
        new("Move NPC", addStep, EventStepKind.MoveNpc),
        DropDownChoice.Title("PARTY"),
        new("Recruit", addStep, EventStepKind.Recruit),
        new("Rest", addStep, EventStepKind.Rest),
        DropDownChoice.Title("SCREENS"),
        new("Open Forge", addStep, EventStepKind.OpenForge),
        new("Open shop", addStep, EventStepKind.OpenShop),
        DropDownChoice.Title("QUESTS"),
        new("Start quest", addStep, EventStepKind.StartQuest),
        new("Advance quest", addStep, EventStepKind.AdvanceQuest),
        new("Complete quest", addStep, EventStepKind.CompleteQuest),
    ];
}
