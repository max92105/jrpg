using System.Globalization;
using Jrpg.Core.Events;
using Jrpg.Core.Maps;
using Jrpg.Core.Party;
using Jrpg.Core.Quests;
using Jrpg.Data.Content;

namespace Jrpg.Data.Maps;

/// <summary>
/// The rules for one map's object and cell events, their gates and their steps, and the order of an object's events (no
/// ungated event may shadow a later one of its activation, ADR 0033). Each problem names the event, and the step when
/// it's one.
/// </summary>
internal static class MapEventValidator
{
    /// <returns>What is wrong with the map's events, or null if nothing is.</returns>
    public static string? FindProblem(MapLayout layout, MapReferences references) =>
        FindNullEntry(layout)
        ?? FindRepeatedEventId(layout)
        ?? FindShadowedEvent(layout)
        ?? FindObjectEventProblem(layout, references)
        ?? FindCellEventProblem(layout, references);

    // The serializer doesn't apply nullable annotations to collection elements, so stray nulls get here.
    private static string? FindNullEntry(MapLayout layout)
    {
        var objectWithNullEvent = layout.Objects.FirstOrDefault(placed => placed.Events.Contains(null));
        if (objectWithNullEvent is not null)
        {
            return $"object '{objectWithNullEvent.Uid}': events holds a null entry.";
        }

        if (layout.CellEvents.Contains(null))
        {
            return "cellEvents holds a null entry.";
        }

        var eventWithNullStep = layout.AllEvents.FirstOrDefault(mapEvent => mapEvent.Steps.Contains(null));
        if (eventWithNullStep is not null)
        {
            return $"event '{eventWithNullStep.Id}': steps holds a null entry.";
        }

        var eventWithNullLine = layout.AllEvents.FirstOrDefault(
            mapEvent => mapEvent.Steps.OfType<DialogueStep>().Any(dialogue => dialogue.Speech.Lines.Contains(null)));
        return eventWithNullLine is null ? null : $"event '{eventWithNullLine.Id}': a dialogue's lines hold a null entry.";
    }

    private static string? FindRepeatedEventId(MapLayout layout)
    {
        var repeatedId = layout.AllEvents.GroupBy(mapEvent => mapEvent.Id).FirstOrDefault(group => group.Count() > 1)?.Key;
        return repeatedId is null ? null : $"event id '{repeatedId}' is used more than once.";
    }

    // An object event with no gate that isn't "once" always wins over the later events of its activation on the object,
    // which could then never run (ADR 0033): content refuses the order rather than leaving it to be found in play.
    private static string? FindShadowedEvent(MapLayout layout)
    {
        foreach (var placed in layout.Objects)
        {
            for (var index = 0; index < placed.Events.Count; index++)
            {
                var first = placed.Events[index];
                if (!CanAlwaysStart(first))
                {
                    continue;
                }

                if (placed.Events.Skip(index + 1).FirstOrDefault(later => later.Activation == first.Activation) is { } shadowed)
                {
                    return $"object '{placed.Uid}': event '{first.Id}' can always start, so the later {ContentJsonOptions.EnumName(first.Activation)} event '{shadowed.Id}' never would; gate it or make it once.";
                }
            }
        }

        return null;
    }

    private static bool CanAlwaysStart(MapEvent mapEvent) =>
        !mapEvent.IsOnce && mapEvent.OnlyWhenObjectId is null && mapEvent.OnlyWhenLeaderId is null && mapEvent.OnlyWhenQuest is null;

    private static string? FindObjectEventProblem(MapLayout layout, MapReferences references)
    {
        foreach (var placed in layout.Objects)
        {
            foreach (var mapEvent in placed.Events)
            {
                var problem = FindEventProblem(mapEvent, references);
                if (problem is not null)
                {
                    return $"event '{mapEvent.Id}': {problem}";
                }
            }
        }

        return null;
    }

    private static string? FindCellEventProblem(MapLayout layout, MapReferences references)
    {
        foreach (var cellEvent in layout.CellEvents)
        {
            var problem = !layout.Contains(cellEvent.Cell) ? "the cell is outside the map."
                : cellEvent.Event.Activation == EventActivation.Interact ? "a cell event must have touch or arrive activation."
                : cellEvent.Event.Steps.Any(step => step is SwapObjectStep) ? "swapObject only works in an object's event."
                : FindEventProblem(cellEvent.Event, references);
            if (problem is not null)
            {
                return $"cell event '{cellEvent.Event.Id}' at ({cellEvent.Cell.X}, {cellEvent.Cell.Y}): {problem}";
            }
        }

        return null;
    }

    private static string? FindEventProblem(MapEvent mapEvent, MapReferences references)
    {
        if (FindStartProblem(mapEvent, references) is { } startProblem)
        {
            return startProblem;
        }

        if (mapEvent.Steps.Count == 0)
        {
            return "steps must not be empty.";
        }

        for (var index = 0; index < mapEvent.Steps.Count; index++)
        {
            var problem = FindStepProblem(mapEvent.Steps[index], references);
            if (problem is not null)
            {
                return $"step {index}: {problem}";
            }
        }

        return null;
    }

    // What starts the event and when it may run: its activation's prompt and its conditions.
    private static string? FindStartProblem(MapEvent mapEvent, MapReferences references)
    {
        if (mapEvent.Activation == EventActivation.Interact && string.IsNullOrWhiteSpace(mapEvent.Prompt))
        {
            return "an interact event needs a prompt.";
        }

        if (mapEvent.Activation == EventActivation.Arrive && mapEvent.Prompt is not null)
        {
            return "an arrive event has no prompt: nothing is pressed to start it.";
        }

        if (mapEvent.OnlyWhenObjectId is { } onlyWhen && !references.Content.Objects.ContainsKey(onlyWhen))
        {
            return $"onlyWhenObjectId has unknown object id '{onlyWhen}'.";
        }

        if (mapEvent.OnlyWhenLeaderId is { } leaderId && !references.Content.MemberIds.Contains(leaderId))
        {
            return $"onlyWhenLeaderId has unknown party member id '{leaderId}'.";
        }

        return mapEvent.OnlyWhenQuest is { } condition ? FindQuestConditionProblem(condition, references) : null;
    }

    // The quest must exist; the two stage kinds need a stage the quest has, the others take none (ADR 0033).
    private static string? FindQuestConditionProblem(QuestCondition condition, MapReferences references)
    {
        if (!references.Content.Quests.TryGetValue(condition.QuestId, out var quest))
        {
            return $"onlyWhenQuest has unknown quest id '{condition.QuestId}'.";
        }

        var kind = ContentJsonOptions.EnumName(condition.Kind);
        if (condition.Kind is not (QuestConditionKind.AtStage or QuestConditionKind.ReachedStage))
        {
            return condition.StageId is null ? null : $"onlyWhenQuest kind '{kind}' names no stage; remove stageId '{condition.StageId}'.";
        }

        return condition.StageId is null ? $"onlyWhenQuest kind '{kind}' needs a stageId."
            : !quest.HasStage(condition.StageId) ? $"onlyWhenQuest has unknown stage '{condition.StageId}' of quest '{condition.QuestId}'."
            : null;
    }

    private static string? FindStepProblem(EventStep step, MapReferences references) => step switch
    {
        DialogueStep dialogue when dialogue.Speech.Lines.Count == 0 => "dialogue needs at least one line.",
        GiveItemStep give when !references.Content.ItemIds.Contains(give.ItemId) => $"giveItem has unknown item id '{give.ItemId}'.",
        GiveItemStep give when give.Quantity < 1 => $"giveItem quantity must be at least 1 (was {give.Quantity}).",
        RecruitStep recruit when !references.Content.MemberIds.Contains(recruit.MemberId) => $"recruit has unknown party member id '{recruit.MemberId}'.",
        SwapObjectStep swap when !references.Content.Objects.ContainsKey(swap.ObjectId) => $"swapObject has unknown object id '{swap.ObjectId}'.",
        WarpStep warp when !references.Layouts.TryGetValue(warp.MapId, out _) => $"warp has unknown map id '{warp.MapId}'.",
        WarpStep warp when !references.Layouts[warp.MapId].Contains(warp.Cell) =>
            $"warp cell ({warp.Cell.X}, {warp.Cell.Y}) is outside map '{warp.MapId}'.",
        StillStep still => FindStillProblem(still),
        VoiceStep voice when string.IsNullOrWhiteSpace(voice.File) => "voice must name its file.",
        OpenShopStep shop => FindOpenShopProblem(shop, references),
        RestStep { Price: { } price } => FindRestPriceProblem(price),
        StartQuestStep or AdvanceQuestStep or CompleteQuestStep => FindQuestStepProblem(step, references),
        _ => null,
    };

    // The quest, and for an advance its stage, must exist in content; the stage is looked up in the quest's own list.
    private static string? FindQuestStepProblem(EventStep step, MapReferences references)
    {
        var (name, questId) = step switch
        {
            StartQuestStep start => ("startQuest", start.QuestId),
            AdvanceQuestStep advance => ("advanceQuest", advance.QuestId),
            CompleteQuestStep complete => ("completeQuest", complete.QuestId),
            _ => throw new ArgumentOutOfRangeException(nameof(step), step, "Not a quest step."),
        };
        if (!references.Content.Quests.TryGetValue(questId, out var quest))
        {
            return $"{name} has unknown quest id '{questId}'.";
        }

        return step is AdvanceQuestStep { StageId: var stageId } && !quest.HasStage(stageId)
            ? $"advanceQuest names stage '{stageId}', which quest '{questId}' doesn't have."
            : null;
    }

    private static string? FindRestPriceProblem(int price) =>
        price >= 1 && price <= Purse.MaxGold ? null : $"rest price must be 1 to {Purse.MaxGold} (was {price}).";

    private static string? FindOpenShopProblem(OpenShopStep shop, MapReferences references) =>
        references.Content.ShopIds.Contains(shop.ShopId) ? null : $"openShop has unknown shop id '{shop.ShopId}'.";

    private static string? FindStillProblem(StillStep still) =>
        string.IsNullOrWhiteSpace(still.ImageFile) ? "still must name its imageFile."
        : !(still.Seconds > 0 && still.Seconds < TimeSpan.MaxValue.TotalSeconds)
            ? $"still seconds must be above 0 and a time the game can count (was {still.Seconds.ToString(CultureInfo.InvariantCulture)})."
        : still.Caption is not null && string.IsNullOrWhiteSpace(still.Caption) ? "still caption must be null or have text."
        : null;
}
