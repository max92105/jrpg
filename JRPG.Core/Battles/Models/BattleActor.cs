using System.Collections.Generic;

namespace JRPG.Core.Battles.Models
{
    public abstract class BattleActor
    {
        public string Name { get; set; }

        public string PortraitFile { get; set; }
        public string BattleSpriteFile { get; set; }
        public string AttackVFXFile { get; set; }

        public int HP { get; set; }
        public int MaxHP { get; set; }

        public bool IsAlive => HP > 0;

        public int MP { get; set; }
        public int MaxMP { get; set; }

        public int BaseAttack { get; set; }
        public int BaseDefense { get; set; }

        public List<Skill> Skills { get; set; } = new List<Skill>();

        public abstract int GetTotalAttack();

        public abstract int GetTotalDefense();

        public abstract int TakeDamage(int rawDamage);

        public abstract void Heal(int rawHeal);
    }
}
