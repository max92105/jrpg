using JRPG.Core.Battles.Enum;

namespace JRPG.Core.Battles.Models
{
    public class Skill
    {
        public string Name { get; set; }
        public int MPCost { get; set; }
        public TargetType TargetType { get; set; }
        // maybe an Action<Actor, Actor> Execute or similar
    }
}
