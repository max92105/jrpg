using JRPG.Core.Enums;

namespace JRPG.Core.Triggers.Models
{
    public class TriggerWarp : Trigger
    {
        public override TriggerType Type => TriggerType.Warp;
        public string TargetMap { get; set; }
        public int TargetX { get; set; }
        public int TargetY { get; set; }
    }
}
