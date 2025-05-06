using JRPG.Core.Battles.Models;
using System;
using System.Collections.Generic;

namespace JRPG
{
    public class SaveData
    {
        public List<Character> Characters { get; set; }
        public List<InventoryItem> Inventory { get; set; }
        public Dictionary<string, int> WeaponStock { get; set; }
        public Dictionary<string, int> ArmorStock { get; set; }
        public string CurrentMap { get; set; }
        public double PlayerX { get; set; }
        public double PlayerY { get; set; }
        public List<string> ActivePartyOrder { get; set; }
        public int Gold { get; set; }

        public int PartyAverageLevel { get; set; }
        public string LocationName { get; set; }
        public DateTime SaveTimestamp { get; set; }
        public int PlayTimeSeconds { get; set; }
    }
}
