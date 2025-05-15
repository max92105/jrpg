using JRPG.Core.Battles.Enums;
using JRPG.Core.Battles.Models;
using JRPG.Core.Systems.Inventories.Models;
using System.Collections.Generic;
using System.Linq;

namespace JRPG.Core.Systems.Inventories
{
    public class InventoryManager
    {
        private readonly List<InventoryItem> inventory = new List<InventoryItem>();
        private InventoryItem selectedItem;

        public IReadOnlyList<InventoryItem> Items => inventory;
        public InventoryItem SelectedItem => selectedItem;

        public InventoryManager() 
        {
            InitializeDefaultItems();
        }

        public List<InventoryItem> GetSerializableItems() => Items.ToList();

        public void LoadFromSavedData(List<InventoryItem> loadedItems)
        {
            inventory.Clear();
            inventory.AddRange(loadedItems);
        }

        public void InitializeDefaultItems()
        {
            inventory.Clear();

            inventory.Add(new InventoryItem 
            { 
                Name = "Potion",
                Type = Enums.ItemType.Potion,
                IconFile="Item.png", 
                Quantity = 5, 
                EffectValue = 15, 
                EffectType = EffectType.Heal, 
                Description = "Restores a small amount of HP.", 
                TargetType = TargetType.Ally 
            });
            
            inventory.Add(new InventoryItem 
            { 
                Name = "Hi-Potion",
                Type = Enums.ItemType.Potion,
                IconFile = "Item.png", 
                Quantity = 2, 
                EffectValue = 50, 
                EffectType = EffectType.Heal,
                Description = "Restores a medium amount of HP.", 
                TargetType = TargetType.Ally 
            });

            inventory.Add(new InventoryItem 
            { 
                Name = "Elixir",
                Type = Enums.ItemType.Potion,
                IconFile = "Item.png", 
                Quantity = 1, 
                EffectValue = 100, 
                EffectType = EffectType.Heal, 
                Description = "Fully restores HP and MP.", 
                TargetType = TargetType.Ally 
            });

            inventory.Add(new InventoryItem 
            { 
                Name = "Antidote",
                Type = Enums.ItemType.Potion,
                IconFile = "Item.png", 
                Quantity = 7, 
                EffectValue = 15, 
                EffectType = EffectType.Heal,
                Description = "Cures poison.", 
                TargetType = TargetType.Self 
            });

            inventory.Add(new InventoryItem 
            { 
                Name = "Bomb",
                Type = Enums.ItemType.Bomb,
                IconFile = "Attack.png", 
                Quantity = 3, 
                EffectValue = 15,
                EffectType = EffectType.Damage,
                Description = "Damage an enemy.", 
                TargetType = TargetType.Enemy 
            });

            inventory.Add(new InventoryItem 
            { 
                Name = "Bomb All",
                Type = Enums.ItemType.Bomb,
                IconFile = "Attack.png",
                Quantity = 3, 
                EffectValue = 30, 
                EffectType = EffectType.Damage, 
                Description = "Damage all enemy.", 
                TargetType = TargetType.AllEnemies 
            });

            inventory.Add(new InventoryItem 
            { 
                Name = "Revive Feather",
                Type = Enums.ItemType.Potion,
                IconFile = "Item.png", 
                Quantity = 3, 
                EffectValue = 15, 
                EffectType = EffectType.Heal, 
                Description = "Revives all fallen allies.", 
                TargetType = TargetType.AllAllies 
            });
        }

        public void SelectItem(InventoryItem item)
        {
            selectedItem = item;
        }

        public void UseSelectedItem(Character target)
        {
            if (selectedItem == null) return;

            selectedItem.Quantity--;
            if (selectedItem.Quantity <= 0)
            {
                inventory.Remove(selectedItem);
            }

            selectedItem = null;
        }

        public void ClearSelection()
        {
            selectedItem = null;
        }
    }
}

