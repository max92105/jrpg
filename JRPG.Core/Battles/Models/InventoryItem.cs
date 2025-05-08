using JRPG.Core.Battles.Enum;

namespace JRPG.Core.Battles.Models
{
    public class InventoryItem
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
        public string Description { get; set; }
        public int EffectValue { get; set; }
        public TargetType TargetType { get; set; }
    }
}
