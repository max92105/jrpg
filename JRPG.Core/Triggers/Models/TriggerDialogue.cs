using JRPG.Core.Enums;
using System.Collections.Generic;

namespace JRPG.Core.Triggers.Models
{
    public class TriggerDialogue : Trigger
    {
        public override TriggerType Type => TriggerType.Dialogue;
        public string SpeakerName { get; set; }
        public string PortraitPath { get; set; }
        public List<string> Lines { get; set; } = new List<string>();
    }
}
