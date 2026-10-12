using JRPG.Core.Maps.Models;
using System.Collections.Generic;
using System.IO;

namespace JRPG.Core.Systems
{
    /// <summary>
    /// What the player changed in the world: events already done (opened chests...) and
    /// objects that were swapped (closed chest -> open chest). Saved with the game.
    /// Keys are "map file/object uid", so the same uid on two maps doesn't clash.
    /// </summary>
    public class WorldState
    {
        public HashSet<string> CompletedEvents { get; private set; } = new HashSet<string>();
        public Dictionary<string, int> ObjectSwaps { get; private set; } = new Dictionary<string, int>();

        public static string Key(string mapFile, string objectUid) => $"{Path.GetFileName(mapFile)}/{objectUid}";

        public bool IsEventDone(string mapFile, string objectUid) => CompletedEvents.Contains(Key(mapFile, objectUid));

        public void MarkEventDone(string mapFile, string objectUid) => CompletedEvents.Add(Key(mapFile, objectUid));

        public void SetObjectSwap(string mapFile, string objectUid, int objectId) => ObjectSwaps[Key(mapFile, objectUid)] = objectId;

        /// <summary>Applies remembered swaps to a freshly loaded map (an opened chest stays open).</summary>
        public void ApplyTo(Map map, string mapFile)
        {
            foreach (var obj in map.Objects)
            {
                if (obj.Uid != null && ObjectSwaps.TryGetValue(Key(mapFile, obj.Uid), out int objectId))
                    obj.Id = objectId;
            }
        }

        public void LoadFrom(IEnumerable<string> completedEvents, Dictionary<string, int> objectSwaps)
        {
            CompletedEvents = new HashSet<string>(completedEvents ?? new List<string>());
            ObjectSwaps = objectSwaps != null ? new Dictionary<string, int>(objectSwaps) : new Dictionary<string, int>();
        }
    }
}
