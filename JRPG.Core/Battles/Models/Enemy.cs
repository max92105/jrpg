using System;

namespace JRPG.Core.Battles.Models
{
    public class Enemy : BattleActor
    {
        public bool SpriteFlip { get; set; } = false;

        public Enemy()
        {
        }

        public override int GetTotalAttack()
        {
            return BaseAttack;
        }

        public override int GetTotalDefense()
        {
            return BaseDefense;
        }

        public override int TakeDamage(int rawDamage)
        {
            int damage = Math.Max(1, rawDamage - GetTotalDefense());
            HP = Math.Max(0, HP - damage);

            return damage;
        }

        public override void Heal(int rawHeal)
        {
            HP = Math.Min(MaxHP, HP + rawHeal);
        }
    }
}
