namespace JRPG.Core.Battles.Models.Enemies
{
    public class Goblin : Enemy
    {
        public Goblin() 
        {
            Name = "Goblin";

            PortraitFile = "goblin.png";
            BattleSpriteFile = "Goblin.png";
            AttackVFXFile = "AttackEnemy.gif";

            HP = 40;
            MaxHP = 40;

            MP = 10;
            MaxMP = 10;

            BaseAttack = 20;
            BaseDefense = 2;

            ExpReward = 40;
        }
    }
}
