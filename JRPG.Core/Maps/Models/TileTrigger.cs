using JRPG.Core.Enums;

namespace JRPG.Core.Maps.Models
{
    public class TileTrigger
    {
        public int X { get; set; }
        public int Y { get; set; }
        public TileTriggerType TileTriggerType { get; set; }
        public string TileTriggerData { get; set; }
    }
}
