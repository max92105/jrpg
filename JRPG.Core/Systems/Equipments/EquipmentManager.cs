using JRPG.Core.Battles.Models;
using JRPG.Core.Systems.Equipments.Models;
using JRPG.Core.Systems;

public class EquipmentManager
{
    public EquipmentManager()
    {
        InitializeDefaults();
    }

    private void InitializeDefaults()
    {
    }

    public void UnequipWeapon(Character c)
    {
        if (c.EquippedWeapon != null)
        {
            GameSystems.Inventory.Add(c.EquippedWeapon);
            c.EquippedWeapon = null;
        }
    }

    public void UnequipArmor(Character c)
    {
        if (c.EquippedArmor != null)
        {
            GameSystems.Inventory.Add(c.EquippedArmor);
            c.EquippedArmor = null;
        }
    }

    public bool Equip(Character c, Equipment equipment)
    {
        if (!equipment.AllowedClasses.Contains(c.Class))
            return false;

        if (!GameSystems.Inventory.HasItem(equipment.Id))
            return false;

        if (equipment is Weapon w)
        {
            UnequipWeapon(c);
            c.EquippedWeapon = w;
        }
        else if (equipment is Armor a)
        {
            UnequipArmor(c);
            c.EquippedArmor = a;
        }

        GameSystems.Inventory.Remove(equipment.Id, 1);
        return true;
    }
}