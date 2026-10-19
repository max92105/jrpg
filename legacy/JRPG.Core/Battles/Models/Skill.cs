using JRPG.Core.Battles.Enums;

namespace JRPG.Core.Battles.Models
{
    public class Skill
    {
        public string Name { get; set; }
        public string IconFile { get; set; }
        public int EffectValue { get; set; }
        public EffectType EffectType { get; set; }
        public int MPCost { get; set; }
        public TargetType TargetType { get; set; }
    }
}
