namespace JRPG.Core.Battles.Models
{
    public class TurnEntry
    {
        public Character Character { get; set; }
        public Enemy Enemy { get; set; }
        public bool IsPlayer => Character != null;
        public string Name => IsPlayer ? Character.Name : Enemy.Name;
    }
}
