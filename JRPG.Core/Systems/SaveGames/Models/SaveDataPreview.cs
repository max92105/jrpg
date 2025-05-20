using JRPG.Core.Battles.Models;
using System;
using System.Collections.Generic;

namespace JRPG.Core.Systems.SaveGames.Models
{
    public class SaveDataPreview
    {
        public int SlotIndex { get; set; }
        public bool IsEmpty { get; set; }
        public bool IsCorrupt { get; set; }

        public DateTime? Timestamp { get; set; }
        public string MapName { get; set; }
        public List<Character> Party { get; set; } = new List<Character>();
    }
}