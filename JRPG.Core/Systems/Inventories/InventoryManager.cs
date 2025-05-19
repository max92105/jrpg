using JRPG.Core.Battles.Enums;
using JRPG.Core.Battles.Models;
using JRPG.Core.Systems.Inventories.Models;
using System.Collections.Generic;
using System.Linq;
using JRPG.Core.Systems.Inventories.Enums;
using JRPG.Core.Systems.Equipments.Models;
using JRPG.Core.Systems.SaveGames.Models;
using System.Windows.Controls;

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

        public List<SavedInventoryEntry> GetSerializableItems()
        {
            return inventory
                .Select(i => new SavedInventoryEntry
                {
                    ItemId = i.Item.Id,
                    Quantity = i.Quantity
                })
                .ToList();
        }

        public void LoadFromSavedData(List<SavedInventoryEntry> savedItems)
        {
            inventory.Clear();

            foreach (var entry in savedItems)
            {
                var item = CreateItemById(entry.ItemId);
                if (item == null) continue;

                var inventoryItem = new InventoryItem
                {
                    Item = item,
                    Quantity = entry.Quantity
                };

                foreach (var charId in entry.EquippedToCharacterIds)
                {
                    var character = GameSystems.Party.GetCharacterById(charId);
                    if (character == null) continue;

                    inventoryItem.EquippedTo.Add(character);

                    if (item is Weapon weapon)
                        character.EquippedWeapon = weapon;
                    else if (item is Armor armor)
                        character.EquippedArmor = armor;
                }

                inventory.Add(inventoryItem);
            }
        }

        private Item CreateItemById(string id)
        {
            switch (id)
            {
                case "potion":
                    return new Consumable
                    {
                        Id = "potion",
                        Name = "Potion",
                        IconFile = "Consumable.png",
                        Description = "Restores a small amount of HP.",
                        EffectValue = 15,
                        EffectType = EffectType.Heal,
                        TargetType = TargetType.Ally
                    };

                case "hipotion":
                    return new Consumable
                    {
                        Id = "hipotion",
                        Name = "Hi-Potion",
                        IconFile = "Consumable.png",
                        Description = "Restores a medium amount of HP.",
                        EffectValue = 50,
                        EffectType = EffectType.Heal,
                        TargetType = TargetType.Ally
                    };

                case "elixir":
                    return new Consumable
                    {
                        Id = "elixir",
                        Name = "Elixir",
                        IconFile = "Consumable.png",
                        Description = "Fully restores HP and MP.",
                        EffectValue = 100,
                        EffectType = EffectType.Heal,
                        TargetType = TargetType.Ally
                    };

                case "antidote":
                    return new Consumable
                    {
                        Id = "antidote",
                        Name = "Antidote",
                        IconFile = "Consumable.png",
                        Description = "Cures poison.",
                        EffectValue = 15,
                        EffectType = EffectType.Heal,
                        TargetType = TargetType.Self
                    };

                case "bomb":
                    return new Consumable
                    {
                        Id = "bomb",
                        Name = "Bomb",
                        IconFile = "Consumable.png",
                        Description = "Damage an enemy.",
                        EffectValue = 15,
                        EffectType = EffectType.Damage,
                        TargetType = TargetType.Enemy,
                        CanBeUsedFromInventory = false
                    };

                case "bomball":
                    return new Consumable
                    {
                        Id = "bomball",
                        Name = "Bomb All",
                        IconFile = "Consumable.png",
                        Description = "Damage all enemies.",
                        EffectValue = 30,
                        EffectType = EffectType.Damage,
                        TargetType = TargetType.AllEnemies,
                        CanBeUsedFromInventory = false
                    };

                case "revivefeather":
                    return new Consumable
                    {
                        Id = "revivefeather",
                        Name = "Revive Feather",
                        IconFile = "Consumable.png",
                        Description = "Revives all fallen allies.",
                        EffectValue = 15,
                        EffectType = EffectType.Heal,
                        TargetType = TargetType.AllAllies
                    };

                case "BronzeSword":
                    return new Weapon
                    {
                        Id = "BronzeSword",
                        Name = "Bronze Sword",
                        IconFile = "Weapon.png",
                        AttackBonus = 5,
                        AllowedClasses = new List<string> { "Warrior", "Knight", "Thief" },
                        Description = "Sword made of bronze"
                    };

                case "IronSword":
                    return new Weapon
                    {
                        Id = "IronSword",
                        Name = "Iron Sword",
                        IconFile = "Weapon.png",
                        AttackBonus = 8,
                        AllowedClasses = new List<string> { "Warrior", "Knight" },
                        Description = "Sword made of iron"
                    };

                case "MagicStaff":
                    return new Weapon
                    {
                        Id = "MagicStaff",
                        Name = "Magic Staff",
                        IconFile = "Weapon.png",
                        AttackBonus = 6,
                        AllowedClasses = new List<string> { "Mage", "Healer" }
                    };

                case "LeatherArmor":
                    return new Armor
                    {
                        Id = "LeatherArmor",
                        Name = "Leather Armor",
                        IconFile = "Armor.png",
                        DefenseBonus = 3,
                        AllowedClasses = new List<string> { "Warrior", "Knight", "Thief", "Ranger" }
                    };

                case "ChainMail":
                    return new Armor
                    {
                        Id = "ChainMail",
                        Name = "Chain Mail",
                        IconFile = "Armor.png",
                        DefenseBonus = 6,
                        AllowedClasses = new List<string> { "Warrior", "Knight" }
                    };

                case "WizardRobe":
                    return new Armor
                    {
                        Id = "WizardRobe",
                        Name = "Wizard Robe",
                        IconFile = "Armor.png",
                        DefenseBonus = 4,
                        AllowedClasses = new List<string> { "Mage", "Healer" }
                    };

                default:
                    return null;
            }
        }

        public void InitializeDefaultItems()
        {
            inventory.Clear();

            Add(CreateItemById("potion"), 5);
            Add(CreateItemById("hipotion"), 2);
            Add(CreateItemById("elixir"), 1);
            Add(CreateItemById("antidote"), 7);
            Add(CreateItemById("bomb"), 3);
            Add(CreateItemById("bomball"), 3);
            Add(CreateItemById("revivefeather"), 3);

            Add(CreateItemById("BronzeSword"), 2);
            Add(CreateItemById("IronSword"), 1);
            Add(CreateItemById("MagicStaff"), 3);

            Add(CreateItemById("LeatherArmor"), 2);
            Add(CreateItemById("ChainMail"), 3);
            Add(CreateItemById("WizardRobe"), 1);
        }

        public void Add(Item item, int amount = 1)
        {
            if (item == null || amount <= 0)
                return;

            var entry = inventory.FirstOrDefault(i => i.Item.Id == item.Id);

            if (entry != null && entry.IsStackable)
            {
                entry.Quantity += amount;
            }
            else if (entry == null)
            {
                inventory.Add(new InventoryItem
                {
                    Item = item,
                    Quantity = amount
                });
            }
            else if (!entry.IsStackable)
            {
                // Add multiple unstackable copies separately
                for (int i = 0; i < amount; i++)
                {
                    inventory.Add(new InventoryItem
                    {
                        Item = item,
                        Quantity = 1
                    });
                }
            }
        }

        public bool Remove(Item item, int quantity = 1)
        {
            return Remove(item?.Id, quantity);
        }

        public bool Remove(string itemId, int quantity = 1)
        {
            var entry = inventory.FirstOrDefault(i => i.Item.Id == itemId);
            if (entry == null || entry.Quantity < quantity)
                return false;

            entry.Quantity -= quantity;
            if (entry.Quantity <= 0)
                inventory.Remove(entry);

            return true;
        }

        public bool HasItem(string itemId)
        {
            return inventory.Any(i => i.Item.Id == itemId && i.Quantity > 0);
        }

        public int GetQuantity(string itemId)
        {
            return inventory.FirstOrDefault(i => i.Item.Id == itemId)?.Quantity ?? 0;
        }

        public void SelectItem(InventoryItem item)
        {
            selectedItem = item;
        }

        public List<ActionResult> UseSelectedItem(Character user, List<BattleActor> targets)
        {
            var item = selectedItem?.Item as Consumable;
            if (item == null)
            {
                return null;
            }

            var consumable = item;

            var result = new List<ActionResult>();

            foreach (var target in targets)
            {
                if (consumable.EffectType == EffectType.Heal)
                {
                    target.Heal(consumable.EffectValue);
                }
                else if (consumable.EffectType == EffectType.Damage)
                {
                    target.TakeDamage(consumable.EffectValue);
                }

                result.Add(new ActionResult(target, consumable.EffectValue, consumable.EffectType));
            }

            selectedItem.Quantity--;

            if (selectedItem.Quantity <= 0)
                inventory.Remove(selectedItem);

            selectedItem = null;

            return result;
        }

        public void ClearSelection()
        {
            selectedItem = null;
        }
    }
}
