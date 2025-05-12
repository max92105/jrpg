using JRPG.Core.Battles.Enum;

namespace JRPG.Core.Battles.Models
{
    public class TurnActionResult
    {
        public BattleActor Target { get; set; }

        public int EffectFinalValue { get; set; }
        public EffectType EffectType { get; set; }

        public TurnActionResult(BattleActor target, int effectFinalValue, EffectType effectType) 
        {
            Target = target;
            EffectFinalValue = effectFinalValue;
            EffectType = effectType;
        }
    }
}
