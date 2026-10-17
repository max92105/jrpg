using JRPG.Core.Maps.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JRPG.Core.Events.Models
{
    /// <summary>How an event starts.</summary>
    public enum EventActivation
    {
        Interact,   // the player faces it and presses the action button (chests, signs, NPCs)
        Touch       // the player walks into it (doors, traps)
    }

    public enum EventActionType
    {
        Dialogue,
        GiveItem,
        SwapObject,
        Warp,
        Battle
    }

    /// <summary>
    /// One step of an event. Only the fields of its Type are used (kept flat so maps stay
    /// simple JSON and the editor can show one form per type).
    /// </summary>
    public class EventAction
    {
        public EventActionType Type { get; set; }

        // Dialogue
        public string Speaker { get; set; }
        public string Portrait { get; set; }
        public List<string> Lines { get; set; } = new List<string>();

        // GiveItem
        public string ItemId { get; set; }
        public int Quantity { get; set; } = 1;

        // SwapObject: the object turns into this object definition (e.g. closed chest -> open chest)
        public int ObjectId { get; set; }

        // Warp: target map file and tile coordinates (the player lands in the middle of that tile)
        public string TargetMap { get; set; }
        public int TargetX { get; set; }
        public int TargetY { get; set; }

        /// <summary>One-line summary for lists in the editor.</summary>
        public string Describe(System.Func<int, string> objectName = null)
        {
            switch (Type)
            {
                case EventActionType.Dialogue:
                    string first = Lines?.FirstOrDefault() ?? "";
                    if (first.Length > 30) first = first.Substring(0, 30) + "...";
                    return $"Dialogue: {(string.IsNullOrEmpty(Speaker) ? "" : Speaker + ": ")}\"{first}\"" +
                           (Lines != null && Lines.Count > 1 ? $" (+{Lines.Count - 1})" : "");
                case EventActionType.GiveItem:
                    return $"Give item: {ItemId} x{Quantity}";
                case EventActionType.SwapObject:
                    return $"Swap object to: {objectName?.Invoke(ObjectId) ?? "#" + ObjectId}";
                case EventActionType.Warp:
                    return $"Warp to: {TargetMap} ({TargetX}, {TargetY})";
                case EventActionType.Battle:
                    return "Start battle";
                default:
                    return Type.ToString();
            }
        }
    }

    /// <summary>An event attached to a placed object (chest, door, sign...).</summary>
    public class MapEvent
    {
        public EventActivation Activation { get; set; } = EventActivation.Interact;

        // Shown next to the object when the player can interact ("Open", "Read"...)
        public string Prompt { get; set; } = "Check";

        // Runs only once per game (remembered in the save), e.g. a chest
        public bool Once { get; set; }

        // Only runs while the object is this object definition (null = always). Lets one object
        // react by state, e.g. a closed door opens on a press, the open door warps on touch.
        public int? OnlyWhenObjectId { get; set; }

        public List<EventAction> Actions { get; set; } = new List<EventAction>();

        /// <summary>True if this event can start on this object right now (ignores "once").</summary>
        public bool IsActiveOn(PlacedObject obj) =>
            Actions != null && Actions.Count > 0 &&
            (OnlyWhenObjectId == null || OnlyWhenObjectId == obj.Id);

        /// <summary>One-line summary for lists in the editor.</summary>
        public string Describe(System.Func<int, string> objectName = null)
        {
            string when = Activation == EventActivation.Touch ? "Walk into" : $"Press \"{Prompt}\"";
            string condition = OnlyWhenObjectId.HasValue
                ? $" if {objectName?.Invoke(OnlyWhenObjectId.Value) ?? "#" + OnlyWhenObjectId}"
                : "";
            return $"{when}{condition}: {Actions?.Count ?? 0} action(s){(Once ? ", once" : "")}";
        }

        private static readonly JsonSerializerOptions CloneOptions = new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter() }
        };

        /// <summary>Deep copy (the editor edits a copy so Cancel and Undo work).</summary>
        public MapEvent Clone()
        {
            return JsonSerializer.Deserialize<MapEvent>(JsonSerializer.Serialize(this, CloneOptions), CloneOptions);
        }
    }
}
