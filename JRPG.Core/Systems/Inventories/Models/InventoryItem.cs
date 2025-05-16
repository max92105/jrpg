namespace JRPG.Core.Systems.Inventories.Models
{
    public class InventoryItem
    {
        public Item Item { get; set; }
        public int Quantity { get; set; }

        public bool IsStackable => Item.MaxStack > 1;
    }
}
