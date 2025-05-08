using System;

namespace JRPG.Core.Battles.Models
{
    public class Character : BattleActor
    {
        public string Class { get; set; }
        public int Level { get; set; }

        public bool IsActive { get; set; } = false;
        public bool IsMainCharacter { get; set; } = false;

        public int EXP { get; set; } = 0;
        public int EXPToNextLevel { get; set; } = 100; 

        public string EquippedWeapon { get; set; }
        public string EquippedArmor { get; set; }

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

        public void UseMP(int rawMP)
        {
            MP = Math.Max(0, MP - rawMP);
        }

        public override void Heal(int rawHeal)
        {
            HP = Math.Min(MaxHP, HP + rawHeal);
        }

        //public override int GetTotalAttack(Dictionary<string, int> weaponAttackBonus)
        //{
        //    int bonus = 0;
        //    if (EquippedWeapon != null && weaponAttackBonus.ContainsKey(EquippedWeapon))
        //        bonus = weaponAttackBonus[EquippedWeapon];

        //    return BaseAttack + bonus;
        //}

        //public override int GetTotalDefense(Dictionary<string, int> armorDefenseBonus)
        //{
        //    int bonus = 0;
        //    if (EquippedArmor != null && armorDefenseBonus.ContainsKey(EquippedArmor))
        //        bonus = armorDefenseBonus[EquippedArmor];

        //    return BaseDefense + bonus;
        //}

        public void LevelUp()
        {
            Level++;
            BaseAttack += 2;   // Gain Attack
            BaseDefense += 2;  // Gain Defense
            HP += 20;          // Gain HP

            EXP -= EXPToNextLevel; // Remove used EXP

            // New EXP requirement: simple formula, could be more complex later
            EXPToNextLevel = (int)(EXPToNextLevel * 1.2);
        }

        public void GainEXP(int amount)
        {
            EXP += amount;

            while (EXP >= EXPToNextLevel)
            {
                LevelUp();
            }
        }
    }
}
