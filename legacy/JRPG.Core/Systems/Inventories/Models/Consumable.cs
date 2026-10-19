using JRPG.Core.Battles.Enums;

namespace JRPG.Core.Systems.Inventories.Models
{
    public class Consumable : Item
    {
        public int EffectValue { get; set; }
        public EffectType EffectType { get; set; }
        public TargetType TargetType { get; set; }
    }
}
