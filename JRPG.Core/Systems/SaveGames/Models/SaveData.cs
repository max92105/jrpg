using JRPG.Core.Battles.Models;
using System;
using System.Collections.Generic;

namespace JRPG.Core.Systems.SaveGames.Models
{
    public class SaveData
    {
        public List<Character> Characters { get; set; }
        public List<SavedInventoryEntry> Inventory { get; set; }
        public string CurrentMap { get; set; }
        public double PlayerX { get; set; }
        public double PlayerY { get; set; }
        public List<string> ActivePartyOrder { get; set; }
        public int Gold { get; set; }

        public int PartyAverageLevel { get; set; }
        public string LocationName { get; set; }
        public DateTime SaveTimestamp { get; set; }
        public int PlayTimeSeconds { get; set; }

        // World state: events already done (opened chests...) and swapped objects
        public List<string> CompletedEvents { get; set; }
        public Dictionary<string, int> ObjectSwaps { get; set; }
    }
}
