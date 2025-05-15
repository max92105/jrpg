using JRPG.Core.Battles.Enums;

namespace JRPG.Core.Battles.Models
{
    public class ActionResult
    {
        public BattleActor Target { get; set; }

        public int EffectFinalValue { get; set; }
        public EffectType EffectType { get; set; }

        public ActionResult(BattleActor target, int effectFinalValue, EffectType effectType) 
        {
            Target = target;
            EffectFinalValue = effectFinalValue;
            EffectType = effectType;
        }
    }
}
