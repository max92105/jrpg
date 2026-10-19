using Jrpg.Core.Dialogue;
using Jrpg.Core.Events;

namespace Jrpg.Editor.Events;

/// <summary>
/// Whether two event lists say the same thing. Records compare their lists by reference, so the steps and a
/// dialogue's lines are compared item by item here.
/// </summary>
internal static class MapEventComparison
{
    // One shared empty list, so two events compare equal on everything but their steps.
    private static readonly IReadOnlyList<EventStep> _noSteps = [];

    public static bool AreSame(IReadOnlyList<MapEvent> left, IReadOnlyList<MapEvent> right) =>
        left.Count == right.Count && left.Zip(right).All(pair => AreSame(pair.First, pair.Second));

    private static bool AreSame(MapEvent left, MapEvent right) =>
        left with { Steps = _noSteps } == right with { Steps = _noSteps }
        && left.Steps.Count == right.Steps.Count
        && left.Steps.Zip(right.Steps).All(pair => AreSame(pair.First, pair.Second));

    private static bool AreSame(EventStep left, EventStep right) => (left, right) switch
    {
        (DialogueStep first, DialogueStep second) => AreSame(first.Speech, second.Speech),
        _ => left == right,
    };

    private static bool AreSame(Speech left, Speech right) =>
        left.Speaker == right.Speaker && left.PortraitFile == right.PortraitFile && left.Lines.SequenceEqual(right.Lines);
}
