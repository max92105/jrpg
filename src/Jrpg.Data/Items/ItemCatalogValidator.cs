using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Data.Content;

namespace Jrpg.Data.Items;

internal static class ItemCatalogValidator
{
    public static void Validate(string fileName, IReadOnlyList<ItemDefinition> items, IReadOnlySet<string> classIds) =>
        new ContentListValidator<ItemDefinition>(fileName, "item", item => item.Id)
            .Validate(items, item => FindProblem(item, classIds));

    private static string? FindProblem(ItemDefinition item, IReadOnlySet<string> classIds)
    {
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

        return item.Equipment is null ? null : FindEquipmentProblem(item.Equipment, classIds);
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

    private static string? FindEquipmentProblem(EquipmentStats equipment, IReadOnlySet<string> classIds)
    {
        if (equipment.AttackBonus < 0)
        {
            return $"equipment.attackBonus must be at least 0 (was {equipment.AttackBonus}).";
        }

        if (equipment.DefenseBonus < 0)
        {
            return $"equipment.defenseBonus must be at least 0 (was {equipment.DefenseBonus}).";
        }

        if (equipment.AllowedClassIds.Any(string.IsNullOrWhiteSpace))
        {
            return "equipment.allowedClassIds must not contain a null or empty class id.";
        }

        var unknownClassId = equipment.AllowedClassIds.FirstOrDefault(id => !classIds.Contains(id));
        return unknownClassId is null ? null : $"equipment.allowedClassIds has unknown class id '{unknownClassId}'.";
    }
}
