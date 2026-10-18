using Jrpg.Core.Dialogue;
using Jrpg.Core.Events;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;

namespace Jrpg.ContentConverter.Events;

/// <summary>
/// Turns legacy object events and tile triggers into map events (one event system, developer decision 20). Event ids are
/// <c>{uid}_{index}</c> and <c>cell_{x}_{y}</c>. Battles are skipped and reported until question 30 is answered.
/// </summary>
internal static class EventConversion
{
    /// <summary>The prompt the legacy game showed when an event had none.</summary>
    private const string DefaultPrompt = "Check";

    public static IReadOnlyList<MapEvent> ConvertObjectEvents(
        string uid, IReadOnlyList<LegacyMapEventJson> legacyEvents, MapConversionContext context)
    {
        var events = new List<MapEvent>();
        for (var index = 0; index < legacyEvents.Count; index++)
        {
            // The index is the legacy position, so skipping one event doesn't shift the ids saves remember.
            var converted = ConvertObjectEvent($"{uid}_{index}", legacyEvents[index], context);
            if (converted is not null)
            {
                events.Add(converted);
            }
        }

        return events;
    }

    public static IReadOnlyList<CellEvent> ConvertTileTriggers(
        IEnumerable<(GridCell Cell, LegacyTriggerJson Trigger)> triggers, MapConversionContext context)
    {
        var cellEvents = new Dictionary<GridCell, CellEvent>();
        foreach (var (cell, trigger) in triggers)
        {
            var converted = ConvertTrigger(cell, trigger, context);
            if (converted is not null && !cellEvents.TryAdd(cell, converted))
            {
                throw new ConversionException($"{context.SourceFile}: cell ({cell.X}, {cell.Y}) has two triggers.");
            }
        }

        return [.. cellEvents.Values.OrderBy(cellEvent => cellEvent.Cell.Y).ThenBy(cellEvent => cellEvent.Cell.X)];
    }

    private static MapEvent? ConvertObjectEvent(string eventId, LegacyMapEventJson? legacyEvent, MapConversionContext context)
    {
        // The serializer doesn't apply nullable annotations to collection elements, so a stray null gets here.
        if (legacyEvent is null || legacyEvent.Actions.Contains(null))
        {
            throw new ConversionException($"{context.SourceFile}: event '{eventId}' is null or holds a null action.");
        }

        var steps = legacyEvent.Actions
            .Select(action => ConvertAction(eventId, action, context))
            .OfType<EventStep>()
            .ToList();
        if (steps.Count == 0)
        {
            context.Report.AddSkipped(context.SourceFile, $"event '{eventId}', which has no action left to run");
            return null;
        }

        return new MapEvent
        {
            Id = eventId,
            Activation = legacyEvent.Activation == LegacyEventActivation.Touch ? EventActivation.Touch : EventActivation.Interact,
            Prompt = string.IsNullOrWhiteSpace(legacyEvent.Prompt) ? DefaultPrompt : legacyEvent.Prompt,
            IsOnce = legacyEvent.Once,
            OnlyWhenObjectId = legacyEvent.OnlyWhenObjectId is { } legacyId ? ToNewObjectId(eventId, legacyId, context) : null,
            Steps = steps,
        };
    }

    private static EventStep? ConvertAction(string eventId, LegacyEventActionJson action, MapConversionContext context)
    {
        switch (action.Type)
        {
            case LegacyEventActionType.Dialogue:
                return new DialogueStep(new Speech(action.Lines ?? [], NullIfEmpty(action.Speaker), NullIfEmpty(action.Portrait)));
            case LegacyEventActionType.GiveItem:
                return new GiveItemStep(action.ItemId ?? string.Empty, action.Quantity);
            case LegacyEventActionType.SwapObject:
                return new SwapObjectStep(ToNewObjectId(eventId, action.ObjectId, context));
            case LegacyEventActionType.Warp:
                return WarpTo(eventId, action.TargetMap, new GridCell(action.TargetX, action.TargetY), context);
            case LegacyEventActionType.Battle:
                context.Report.AddSkipped(context.SourceFile, $"a battle action in event '{eventId}' (event battles wait for question 30)");
                return null;
            default:
                throw new ConversionException($"{context.SourceFile}: event '{eventId}' has an action of unknown type {action.Type}.");
        }
    }

    private static CellEvent? ConvertTrigger(GridCell cell, LegacyTriggerJson trigger, MapConversionContext context)
    {
        var eventId = $"cell_{cell.X}_{cell.Y}";
        EventStep step;
        switch (trigger.Type)
        {
            case LegacyTriggerType.Warp:
                step = WarpTo(eventId, trigger.TargetMap, new GridCell(trigger.TargetX, trigger.TargetY), context);
                break;
            case LegacyTriggerType.Dialogue:
                step = new DialogueStep(new Speech(trigger.Lines ?? [], NullIfEmpty(trigger.SpeakerName), NullIfEmpty(trigger.PortraitPath)));
                break;
            case LegacyTriggerType.Battle:
                context.Report.AddSkipped(context.SourceFile, $"the battle trigger on cell ({cell.X}, {cell.Y}) (event battles wait for question 30)");
                return null;
            default:
                // Item triggers: the legacy game never ran them and no map has one.
                throw new ConversionException($"{context.SourceFile}: cell ({cell.X}, {cell.Y}) has a trigger of type {trigger.Type}, which the converter doesn't support.");
        }

        return new CellEvent(cell, new MapEvent { Id = eventId, Activation = EventActivation.Touch, Steps = [step] });
    }

    private static WarpStep WarpTo(string eventId, string? targetMap, GridCell cell, MapConversionContext context)
    {
        if (string.IsNullOrWhiteSpace(targetMap))
        {
            throw new ConversionException($"{context.SourceFile}: event '{eventId}' warps without a target map.");
        }

        return new WarpStep(LegacyIds.FromName(context.SourceFile, Path.GetFileNameWithoutExtension(targetMap)), cell);
    }

    private static string ToNewObjectId(string eventId, int legacyId, MapConversionContext context) =>
        context.ObjectIdsByLegacyId.TryGetValue(legacyId, out var objectId)
            ? objectId
            : throw new ConversionException($"{context.SourceFile}: event '{eventId}' uses object id {legacyId}, which isn't in the object list.");

    private static string? NullIfEmpty(string? text) => string.IsNullOrWhiteSpace(text) ? null : text;
}
