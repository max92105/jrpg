using JRPG.Core.Triggers.Models;
using System;
using System.Collections.Generic;

namespace JRPG.Systems
{
    public class TriggerHandler
    {
        private readonly Action<string, double, double> onWarp;
        private readonly Action<List<string>, string, string, Action> onDialogue;
        private readonly Action<string> onItemFound;
        private readonly Action onBattleStart;

        public TriggerHandler(
            Action<string, double, double> onWarp,
            Action<List<string>, string, string, Action> onDialogue,
            Action<string> onItemFound,
            Action onBattleStart)
        {
            this.onWarp = onWarp;
            this.onDialogue = onDialogue;
            this.onItemFound = onItemFound;
            this.onBattleStart = onBattleStart;
        }

        public void HandleTrigger(Core.Triggers.Models.Trigger trigger)
        {
            switch (trigger)
            {
                case TriggerWarp warp:
                    onWarp?.Invoke(warp.TargetMap, warp.TargetX, warp.TargetY);
                    break;

                case TriggerDialogue dialogue:
                    string speaker = string.IsNullOrEmpty(dialogue.SpeakerName) ? "???" : dialogue.SpeakerName;
                    string portrait = string.IsNullOrEmpty(dialogue.PortraitPath) ? "Fighter.png" : dialogue.PortraitPath;
                    var lines = dialogue.Lines ?? new List<string> { "..." };
                    onDialogue?.Invoke(lines, speaker, portrait, null);
                    break;

                case TriggerItem item:
                    onItemFound?.Invoke(item.ItemId);
                    break;

                case TriggerBattle battle:
                    onBattleStart?.Invoke();
                    break;

                default:
                    break;
            }
        }
    }
}
