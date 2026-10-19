using System.Collections.Generic;

namespace JRPG.Core.Systems.SaveGames.Models
{
    public class SavedInventoryEntry
    {
        public string ItemId { get; set; }
        public int Quantity { get; set; }
        public List<string> EquippedToCharacterIds { get; set; } = new List<string>();
    }
}
