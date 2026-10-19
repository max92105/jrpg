using JRPG.Core.Battles.Models.Enemies;
using JRPG.Core.Battles.Models;
using JRPG.Core.Triggers.Models;
using JRPG.Managers;
using System;
using System.Collections.Generic;

namespace JRPG.Systems
{
    public class TriggerHandler
    {
        private readonly Action<string, int, int> onWarp; // map file, target tile X, target tile Y
        private readonly Action<List<string>, string, string, Action> onDialogue;
        private readonly Action<string> onItemFound;
        private readonly Action onBattleStart;

        public TriggerHandler(
            Action<string, int, int> onWarp,
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
                    List<Enemy> enemies = new List<Enemy>
                    {
                        new Goblin { SlotIndex = 0 },
                        new Goblin { SlotIndex = 1 },
                        new Goblin { SlotIndex = 2 }
                    };

                    SceneManager.StartBattleAsync(enemies);
                    break;

                default:
                    break;
            }
        }
    }
}
