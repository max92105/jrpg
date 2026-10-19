using JRPG.Core.Battles.Models;
using JRPG.Core.Systems.Equipments.Models;
using JRPG.Core.Systems;
using System.Linq;

public class EquipmentManager
{
    public EquipmentManager()
    {
        InitializeDefaults();
    }

    private void InitializeDefaults() { }

    public void Unequip(Character c, Equipment equipment)
    {
        if (equipment == null) return;

        if (equipment is Weapon && c.EquippedWeapon == equipment)
            c.EquippedWeapon = null;
        else if (equipment is Armor && c.EquippedArmor == equipment)
            c.EquippedArmor = null;

        var entry = GameSystems.Inventory.Items.FirstOrDefault(i => i.Item == equipment);
        entry?.EquippedTo.Remove(c);
    }

    public bool Equip(Character c, Equipment equipment)
    {
        if (!equipment.AllowedClasses.Contains(c.Class))
            return false;

        var entry = GameSystems.Inventory.Items.FirstOrDefault(i => i.Item == equipment);
        if (entry == null || entry.AvailableStock <= 0)
            return false;

        // Unequip old
        if (equipment is Weapon) Unequip(c, c.EquippedWeapon);
        else if (equipment is Armor) Unequip(c, c.EquippedArmor);

        // Equip new
        if (equipment is Weapon w) c.EquippedWeapon = w;
        else if (equipment is Armor a) c.EquippedArmor = a;

        if (!entry.EquippedTo.Contains(c))
            entry.EquippedTo.Add(c);

        return true;
    }
}