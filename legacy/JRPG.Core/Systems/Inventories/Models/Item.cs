namespace JRPG.Core.Systems.Inventories.Models
{
    public class Item
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string IconFile { get; set; }
        public string Description { get; set; }
        public int MaxStack { get; set; } = 99;

        public bool CanBeUsedFromInventory { get; set; } = true; // Default true
    }
}
