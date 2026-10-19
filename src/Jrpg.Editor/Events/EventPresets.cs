using Jrpg.Core.Dialogue;
using Jrpg.Core.Events;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;

namespace Jrpg.Editor.Events;

/// <summary>
/// The legacy event dialog's presets: the usual events of a chest, a door, a door the player opens first, and a sign.
/// What only the developer can say is left empty for the dialog to ask for: the chest's item, the door's destination,
/// the sign's text.
/// </summary>
internal static class EventPresets
{
    // The legacy presets' prompts.
    private const string OpenPrompt = "Open";
    private const string EnterPrompt = "Enter";
    private const string ReadPrompt = "Read";

    // An object id containing this is taken for an "opened" object, as the legacy guess did with names.
    private const string OpenedMarker = "open";

    /// <summary>Pressed once: gives an item, then turns into the opened chest.</summary>
    public static IReadOnlyList<MapEvent> Chest(EventIdAllocation target, string openedObjectId) =>
    [
        new MapEvent
        {
            Id = target.NewEventIds(1)[0],
            Activation = EventActivation.Interact,
            Prompt = OpenPrompt,
            IsOnce = true,
            Steps = [new GiveItemStep("", 1), new SwapObjectStep(openedObjectId)],
        },
    ];

    /// <summary>Walked into: warps.</summary>
    public static IReadOnlyList<MapEvent> Door(EventIdAllocation target) =>
    [
        new MapEvent { Id = target.NewEventIds(1)[0], Activation = EventActivation.Touch, Prompt = EnterPrompt, Steps = [Warp()] },
    ];

    /// <summary>Pressed while closed: opens (turns into the open door, kept in saves); walked into while open: warps.</summary>
    public static IReadOnlyList<MapEvent> DoorYouOpen(EventIdAllocation target, string openObjectId)
    {
        var ids = target.NewEventIds(2);
        return
        [
            new MapEvent
            {
                Id = ids[0],
                Activation = EventActivation.Interact,
                Prompt = OpenPrompt,
                OnlyWhenObjectId = target.ObjectId,
                Steps = [new SwapObjectStep(openObjectId)],
            },
            new MapEvent { Id = ids[1], Activation = EventActivation.Touch, Prompt = EnterPrompt, OnlyWhenObjectId = openObjectId, Steps = [Warp()] },
        ];
    }

    /// <summary>Pressed: shows a dialogue.</summary>
    public static IReadOnlyList<MapEvent> Sign(EventIdAllocation target) =>
    [
        new MapEvent
        {
            Id = target.NewEventIds(1)[0],
            Activation = EventActivation.Interact,
            Prompt = ReadPrompt,
            Steps = [new DialogueStep(new Speech([]))],
        },
    ];

    /// <summary>
    /// The likely "opened" version of an object, such as <c>chest_opened</c> for <c>chest</c>: another object whose id
    /// contains "open", one in the same category first (legacy <c>GuessOpenVersion</c>); the object itself when none.
    /// </summary>
    public static string GuessOpenedObject(string objectId, IReadOnlyDictionary<string, ObjectDefinition> definitions)
    {
        var candidates = definitions.Values
            .Where(definition => definition.Id != objectId && definition.Id.Contains(OpenedMarker, StringComparison.OrdinalIgnoreCase))
            .OrderBy(definition => definition.Id, StringComparer.Ordinal)
            .ToList();
        var category = definitions.TryGetValue(objectId, out var own) ? own.Category : null;
        var guess = candidates.FirstOrDefault(definition => definition.Category == category) ?? candidates.FirstOrDefault();
        return guess?.Id ?? objectId;
    }

    // The destination is left for the developer to choose.
    private static WarpStep Warp() => new("", new GridCell(0, 0));
}
