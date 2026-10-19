using JRPG.Core.Enums;

namespace JRPG.Core.Triggers.Models
{
    public class TriggerItem : Trigger
    {
        public override TriggerType Type => TriggerType.Item;
        public string ItemId { get; set; }
    }
}
