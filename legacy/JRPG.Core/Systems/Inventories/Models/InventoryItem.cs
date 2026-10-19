using JRPG.Core.Battles.Models;
using System.Collections.Generic;

namespace JRPG.Core.Systems.Inventories.Models
{
    public class InventoryItem
    {
        public Item Item { get; set; }
        public int Quantity { get; set; }

        public bool IsStackable => Item.MaxStack > 1;
        public int AvailableStock => Quantity - EquippedTo.Count;
        public List<Character> EquippedTo { get; } = new List<Character>();
    }
}
