using JRPG.Core.Systems.Equipments.Models;
using System;
using System.Collections.Generic;

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

        public Weapon EquippedWeapon { get; set; }
        public Armor EquippedArmor { get; set; }

        public override int GetTotalAttack()
        {
            return BaseAttack + (EquippedWeapon?.AttackBonus ?? 0);
        }

        public override int GetTotalDefense()
        {
            return BaseDefense + (EquippedArmor?.DefenseBonus ?? 0);
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

        /// <summary>
        /// Same as GainEXP, but one level at a time, reporting each step
        /// so the victory screen can fill the EXP bar level by level.
        /// </summary>
        public List<ExpStep> GainEXPByLevel(int amount)
        {
            var steps = new List<ExpStep>();

            while (amount > 0)
            {
                int gained = Math.Min(amount, Math.Max(0, EXPToNextLevel - EXP));
                int levelBefore = Level;

                var step = new ExpStep
                {
                    Level = Level,
                    FromEXP = EXP,
                    ToEXP = EXP + gained,
                    EXPToNextLevel = EXPToNextLevel
                };

                GainEXP(gained);

                step.LeveledUp = Level > levelBefore;
                steps.Add(step);
                amount -= gained;
            }

            return steps;
        }
    }
}
