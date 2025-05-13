using JRPG.Core.Battles.Models;
using System.Collections.Generic;
using System.Linq;

namespace JRPG.Core.System
{
    public class EquipmentManager
    {
        public List<string> AvailableWeapons { get; } = new List<string>();
        public List<string> AvailableArmor { get; } = new List<string>();

        public Dictionary<string, int> WeaponAttackBonus { get; } = new Dictionary<string, int>();
        public Dictionary<string, int> ArmorDefenseBonus { get; } = new Dictionary<string, int>();

        public Dictionary<string, List<string>> WeaponAllowedClasses { get; } = new Dictionary<string, List<string>>();
        public Dictionary<string, List<string>> ArmorAllowedClasses { get; } = new Dictionary<string, List<string>>();

        public Dictionary<string, int> WeaponStock { get; } = new Dictionary<string, int>();
        public Dictionary<string, int> ArmorStock { get; } = new Dictionary<string, int>();

        public EquipmentManager()
        {
            InitializeDefaults();
        }

        public void LoadFromSavedData(
            Dictionary<string, int> savedWeaponStock,
            Dictionary<string, int> savedArmorStock)
        {
            WeaponStock.Clear();
            foreach (var kvp in savedWeaponStock)
            {
                WeaponStock[kvp.Key] = kvp.Value;
                if (!AvailableWeapons.Contains(kvp.Key))
                    AvailableWeapons.Add(kvp.Key);
            }

            ArmorStock.Clear();
            foreach (var kvp in savedArmorStock)
            {
                ArmorStock[kvp.Key] = kvp.Value;
                if (!AvailableArmor.Contains(kvp.Key))
                    AvailableArmor.Add(kvp.Key);
            }
        }

        public void InitializeDefaults()
        {
            // Weapons
            AvailableWeapons.AddRange(new[] { "Bronze Sword", "Iron Sword", "Magic Staff" });
            WeaponAttackBonus["Bronze Sword"] = 5;
            WeaponAttackBonus["Iron Sword"] = 8;
            WeaponAttackBonus["Magic Staff"] = 6;

            WeaponAllowedClasses["Bronze Sword"] = new List<string>() { "Warrior", "Knight", "Thief" };
            WeaponAllowedClasses["Iron Sword"] = new List<string>() { "Warrior", "Knight" };
            WeaponAllowedClasses["Magic Staff"] = new List<string>() { "Mage", "Healer" };

            WeaponStock["Bronze Sword"] = 2;
            WeaponStock["Iron Sword"] = 1;
            WeaponStock["Magic Staff"] = 1;

            // Armor
            AvailableArmor.AddRange(new[] { "Leather Armor", "Chain Mail", "Wizard Robe" });
            ArmorDefenseBonus["Leather Armor"] = 3;
            ArmorDefenseBonus["Chain Mail"] = 6;
            ArmorDefenseBonus["Wizard Robe"] = 4;

            ArmorAllowedClasses["Leather Armor"] = new List<string>() { "Warrior", "Knight", "Thief", "Ranger" };
            ArmorAllowedClasses["Chain Mail"] = new List<string>() { "Warrior", "Knight" };
            ArmorAllowedClasses["Wizard Robe"] = new List<string>() { "Mage", "Healer" };

            ArmorStock["Leather Armor"] = 3;
            ArmorStock["Chain Mail"] = 1;
            ArmorStock["Wizard Robe"] = 2;
        }

        public void UnequipWeapon(Character c)
        {
            if (!string.IsNullOrEmpty(c.EquippedWeapon))
            {
                if (WeaponStock.ContainsKey(c.EquippedWeapon))
                    WeaponStock[c.EquippedWeapon]++;
                c.EquippedWeapon = null;
            }
        }

        public void UnequipArmor(Character c)
        {
            if (!string.IsNullOrEmpty(c.EquippedArmor))
            {
                if (ArmorStock.ContainsKey(c.EquippedArmor))
                    ArmorStock[c.EquippedArmor]++;
                c.EquippedArmor = null;
            }
        }

        public bool EquipWeapon(Character c, string weapon)
        {
            if (!WeaponAllowedClasses.TryGetValue(weapon, out var allowed) || !allowed.Contains(c.Class))
                return false;

            if (!WeaponStock.ContainsKey(weapon) || WeaponStock[weapon] <= 0)
                return false;

            UnequipWeapon(c);
            c.EquippedWeapon = weapon;
            WeaponStock[weapon]--;
            return true;
        }

        public bool EquipArmor(Character c, string armor)
        {
            if (!ArmorAllowedClasses.TryGetValue(armor, out var allowed) || !allowed.Contains(c.Class))
                return false;

            if (!ArmorStock.ContainsKey(armor) || ArmorStock[armor] <= 0)
                return false;

            UnequipArmor(c);
            c.EquippedArmor = armor;
            ArmorStock[armor]--;
            return true;
        }

        public List<string> GetAvailableWeaponsFor(string characterClass)
        {
            return AvailableWeapons
                 .Where(weapon =>
                     WeaponAllowedClasses.ContainsKey(weapon) &&
                     WeaponAllowedClasses[weapon].Contains(characterClass) &&
                     WeaponStock.ContainsKey(weapon) &&
                     WeaponStock[weapon] > 0)
                 .ToList();
        }

        public List<string> GetAvailableArmorFor(string characterClass)
        {
            return AvailableArmor
                .Where(armor =>
                    ArmorAllowedClasses.ContainsKey(armor) &&
                    ArmorAllowedClasses[armor].Contains(characterClass) &&
                    ArmorStock.ContainsKey(armor) &&
                    ArmorStock[armor] > 0)
                .ToList();
        }

        public int GetWeaponAttackBonus(string weapon)
        {
            return WeaponAttackBonus.TryGetValue(weapon, out var bonus) ? bonus : 0;
        }

        public int GetArmorDefenseBonus(string armor)
        {
            return ArmorDefenseBonus.TryGetValue(armor, out var bonus) ? bonus : 0;
        }

        public int GetWeaponStock(string weapon)
        {
            return WeaponStock.TryGetValue(weapon, out var stock) ? stock : 0;
        }

        public int GetArmorStock(string armor)
        {
            return ArmorStock.TryGetValue(armor, out var stock) ? stock : 0;
        }
    }
}
