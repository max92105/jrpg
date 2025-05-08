namespace JRPG.Core.Battles.Models
{
    public class TurnEntry
    {
        public BattleActor BattleActor { get; set; }
        public bool IsPlayer => BattleActor is Character;
    }
}
