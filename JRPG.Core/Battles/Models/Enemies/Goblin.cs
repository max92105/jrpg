namespace JRPG.Core.Battles.Models.Enemies
{
    public class Goblin : Enemy
    {
        public Goblin() 
        {
            Name = "Goblin";

            PortraitFile = "goblin.png";
            BattleSpriteFile = "PlaceholderEnemy.png";

            HP = 40;
            MaxHP = 40;

            MP = 10;
            MaxMP = 10;

            BaseAttack = 10;
            BaseDefense = 2;
        }
    }
}
