using JRPG.Core.Battles.Models;
using JRPG.Core.Systems.Equipments.Models;
using System.Collections.Generic;
using System.Linq;

namespace JRPG.Core.Systems.Equipments
{
    public class EquipmentManager
    {
        public List<Weapon> Weapons { get; } = new List<Weapon>();
        public List<Armor> Armors { get; } = new List<Armor>();

        public List<EquipmentEntry> EquipmentInventory { get; } = new List<EquipmentEntry>();

        public EquipmentManager()
        {
            InitializeDefaults();
        }

        public void LoadFromSavedData(Dictionary<string, int> savedWeaponStock, Dictionary<string, int> savedArmorStock)
        {
            foreach (var entry in EquipmentInventory)
            {
                if (entry.Equipment is Weapon weapon && savedWeaponStock.TryGetValue(weapon.Id, out var weaponStock))
                {
                    entry.Quantity = weaponStock;
                }
                else if (entry.Equipment is Armor armor && savedArmorStock.TryGetValue(armor.Id, out var armorStock))
                {
                    entry.Quantity = armorStock;
                }
            }
        }

        public Dictionary<string, int> GetWeaponStockForSave()
        {
            return EquipmentInventory.Where(obj => obj.Equipment is Weapon).ToDictionary(e => e.Equipment.Id, e => e.Quantity);
        }

        public Dictionary<string, int> GetArmorStockForSave()
        {
            return EquipmentInventory.Where(obj => obj.Equipment is Armor).ToDictionary(e => e.Equipment.Id, e => e.Quantity);
        }

        public void InitializeDefaults()
        {
            AddEquipment(new Weapon { Id = "BronzeSword", Name = "Bronze Sword", AttackBonus = 5, AllowedClasses = new List<string> { "Warrior", "Knight", "Thief" } }, 2);
            AddEquipment(new Weapon { Id = "IronSword", Name = "Iron Sword", AttackBonus = 8, AllowedClasses = new List<string> { "Warrior", "Knight" } }, 1);
            AddEquipment(new Weapon { Id = "MagicStaff", Name = "Magic Staff", AttackBonus = 6, AllowedClasses = new List<string> { "Mage", "Healer" } }, 1);

            AddEquipment(new Armor { Id = "LeatherArmor", Name = "Leather Armor", DefenseBonus = 3, AllowedClasses = new List<string> { "Warrior", "Knight", "Thief", "Ranger" } }, 3);
            AddEquipment(new Armor { Id = "ChainMail", Name = "Chain Mail", DefenseBonus = 6, AllowedClasses = new List<string> { "Warrior", "Knight" } }, 1);
            AddEquipment(new Armor { Id = "WizardRobe", Name = "Wizard Robe", DefenseBonus = 4, AllowedClasses = new List<string> { "Mage", "Healer" } }, 2);
        }

        private void AddEquipment(Equipment equipment, int quantity)
        {
            EquipmentInventory.Add(new EquipmentEntry(equipment, quantity));
            if (equipment is Weapon weapon) Weapons.Add(weapon);
            if (equipment is Armor armor) Armors.Add(armor);
        }

        public void UnequipWeapon(Character c)
        {
            var entry = EquipmentInventory.FirstOrDefault(e => e.Equipment.Id == c.EquippedWeapon?.Id && e.Equipment is Weapon);
            if (entry != null)
            {
                entry.Quantity++;
                c.EquippedWeapon = null;
            }
        }

        public void UnequipArmor(Character c)
        {
            var entry = EquipmentInventory.FirstOrDefault(e => e.Equipment.Id == c.EquippedArmor?.Id && e.Equipment is Armor);
            if (entry != null)
            {
                entry.Quantity++;
                c.EquippedArmor = null;
            }
        }

        public bool Equip(Character c, Equipment equipment)
        {
            if (equipment is Weapon)
            {
                var entry = EquipmentInventory.FirstOrDefault(e => e.Equipment.Id == equipment.Id && equipment.AllowedClasses.Contains(c.Class));
                if (entry == null || entry.Quantity <= 0) return false;

                UnequipWeapon(c);
                c.EquippedWeapon = (Weapon)equipment;
                entry.Quantity--;
                return true;
            }

            if (equipment is Armor)
            {
                var entry = EquipmentInventory.FirstOrDefault(e => e.Equipment.Id == equipment.Id && equipment.AllowedClasses.Contains(c.Class));
                if (entry == null || entry.Quantity <= 0) return false;

                UnequipArmor(c);
                c.EquippedArmor = (Armor)equipment;
                entry.Quantity--;
                return true;
            }

            return false;
        }

        public List<string> GetAvailableWeaponsFor(string characterClass)
        {
            return EquipmentInventory
                .Where(e => e.Equipment is Weapon w && w.AllowedClasses.Contains(characterClass) && e.Quantity > 0)
                .Select(e => e.Equipment.Id)
                .ToList();
        }

        public List<string> GetAvailableArmorFor(string characterClass)
        {
            return EquipmentInventory
                .Where(e => e.Equipment is Armor a && a.AllowedClasses.Contains(characterClass) && e.Quantity > 0)
                .Select(e => e.Equipment.Id)
                .ToList();
        }

        public int GetWeaponAttackBonus(string weaponId)
        {
            return Weapons.FirstOrDefault(w => w.Id == weaponId)?.AttackBonus ?? 0;
        }

        public int GetArmorDefenseBonus(string armorId)
        {
            return Armors.FirstOrDefault(a => a.Id == armorId)?.DefenseBonus ?? 0;
        }

        public int GetWeaponStock(string weaponId)
        {
            return EquipmentInventory.FirstOrDefault(e => e.Equipment.Id == weaponId && e.Equipment is Weapon)?.Quantity ?? 0;
        }

        public int GetArmorStock(string armorId)
        {
            return EquipmentInventory.FirstOrDefault(e => e.Equipment.Id == armorId && e.Equipment is Armor)?.Quantity ?? 0;
        }
    }
}
