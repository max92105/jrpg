using System;

namespace JRPG
{
    public class Fighter
    {
        public string Name { get; set; }
        public int MaxHP { get; set; }
        public int CurrentHP { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }

        public bool IsAlive => CurrentHP > 0;

        public Fighter(string name, int hp, int attack, int defense)
        {
            Name = name;
            MaxHP = hp;
            CurrentHP = hp;
            Attack = attack;
            Defense = defense;
        }

        public int TakeDamage(int rawDamage)
        {
            int damage = Math.Max(1, rawDamage - Defense);
            CurrentHP = Math.Max(0, CurrentHP - damage);
            return damage;
        }

        public void Heal(int amount)
        {
            CurrentHP = Math.Min(MaxHP, CurrentHP + amount);
        }
    }
}
