using JRPG.Core.Battles.Enums;
using JRPG.Core.Systems.Inventories.Enums;

namespace JRPG.Core.Systems.Inventories.Models
{
    public class InventoryItem
    {
        public string Name { get; set; }
        public ItemType Type { get; set; }
        public string IconFile { get; set; }
        public int Quantity { get; set; }
        public string Description { get; set; }
        public int EffectValue { get; set; }
        public EffectType EffectType { get; set; }
        public TargetType TargetType { get; set; }
    }
}
