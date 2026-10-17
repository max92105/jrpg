using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Data.Content;

namespace Jrpg.Data.Items;

internal static class ItemCatalogValidator
{
    public static void Validate(string fileName, IReadOnlyList<ItemDefinition> items)
    {
        var seenIds = new HashSet<string>();
        for (var index = 0; index < items.Count; index++)
        {
            // The serializer doesn't apply nullable annotations to collection elements, so a stray null gets here.
            var item = items[index];
            if (item is null)
            {
                throw new ContentValidationException($"{fileName}: entry {index} is null instead of an item.");
            }

            if (!seenIds.Add(item.Id))
            {
                throw new ContentValidationException($"{fileName}: item id '{item.Id}' is used more than once.");
            }

            var problem = FindProblem(item);
            if (problem is not null)
            {
                throw new ContentValidationException($"{fileName}: item '{item.Id}': {problem}");
            }
        }
    }

    private static string? FindProblem(ItemDefinition item)
    {
        if (string.IsNullOrWhiteSpace(item.Id))
        {
            return "id must not be empty.";
        }

        if (string.IsNullOrWhiteSpace(item.Name))
        {
            return "name must not be empty.";
        }

        if (item.MaxStack < 1)
        {
            return $"maxStack must be at least 1 (was {item.MaxStack}).";
        }

        if (item.Effect is not null && item.Equipment is not null)
        {
            return "an item has either an effect or equipment, not both.";
        }

        if (item.Effect is not null)
        {
            return FindEffectProblem(item.Effect, item.IsUsableFromMenu);
        }

        return item.Equipment is null ? null : FindEquipmentProblem(item.Equipment);
    }

    private static string? FindEffectProblem(Effect effect, bool isUsableFromMenu)
    {
        if (effect.Power < 0)
        {
            return $"effect.power must be at least 0 (was {effect.Power}).";
        }

        var targetsEnemies = effect.Targeting is Targeting.SingleEnemy or Targeting.AllEnemies;
        return isUsableFromMenu && targetsEnemies
            ? $"isUsableFromMenu is true but effect.targeting is {effect.Targeting}; there are no enemies outside battle."
            : null;
    }

    private static string? FindEquipmentProblem(EquipmentStats equipment)
    {
        if (equipment.AttackBonus < 0)
        {
            return $"equipment.attackBonus must be at least 0 (was {equipment.AttackBonus}).";
        }

        if (equipment.DefenseBonus < 0)
        {
            return $"equipment.defenseBonus must be at least 0 (was {equipment.DefenseBonus}).";
        }

        return equipment.AllowedClassIds.Any(string.IsNullOrWhiteSpace)
            ? "equipment.allowedClassIds must not contain a null or empty class id."
            : null;
    }
}
