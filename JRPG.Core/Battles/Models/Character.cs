using System.Collections.Generic;

namespace JRPG.Core.Battles.Models
{
    public class Character
    {
        public string Name { get; set; }

        public string PortraitFile { get; set; }
        public string BattleSpriteFile { get; set; }

        public string Class { get; set; }
        public int Level { get; set; }

        public int HP { get; set; }
        public int MaxHP { get; set; }

        public int MP { get; set; }
        public int MaxMP { get; set; }

        public bool IsActive { get; set; } = false;
        public bool IsMainCharacter { get; set; } = false;

        public int BaseAttack { get; set; }
        public int BaseDefense { get; set; }

        public int EXP { get; set; } = 0;
        public int EXPToNextLevel { get; set; } = 100; 

        public List<Skill> Skills { get; set; } = new List<Skill>();

        public string EquippedWeapon { get; set; }
        public string EquippedArmor { get; set; }

        public int GetTotalAttack(Dictionary<string, int> weaponAttackBonus)
        {
            int bonus = 0;
            if (EquippedWeapon != null && weaponAttackBonus.ContainsKey(EquippedWeapon))
                bonus = weaponAttackBonus[EquippedWeapon];

            return BaseAttack + bonus;
        }

        public int GetTotalDefense(Dictionary<string, int> armorDefenseBonus)
        {
            int bonus = 0;
            if (EquippedArmor != null && armorDefenseBonus.ContainsKey(EquippedArmor))
                bonus = armorDefenseBonus[EquippedArmor];

            return BaseDefense + bonus;
        }

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
