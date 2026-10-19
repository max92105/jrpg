using JRPG.Core.Enums;

namespace JRPG.Core.Triggers.Models
{
    public abstract class Trigger
    {
        public abstract TriggerType Type { get; }
    }
}
