using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Data.Content;
using Jrpg.Data.Effects;

namespace Jrpg.Data.Items;

internal static class ItemCatalogValidator
{
    public static void Validate(string fileName, IReadOnlyList<ItemDefinition> items) =>
        new ContentListValidator<ItemDefinition>(fileName, "item", item => item.Id)
            .Validate(items, item => FindProblem(item));

    private static string? FindProblem(ItemDefinition item)
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

        return item.Equipment is null ? null : FindEquipmentProblem(item.Equipment);
    }

    private static string? FindEffectProblem(Effect effect, bool isUsableFromMenu)
    {
        var problem = EffectValidator.FindProblem(effect);
        if (problem is not null)
        {
            return problem;
        }

        // An extra turn belongs to a battle's turn order, and the menu has no other combatant to aim at.
        if (effect.Kind == EffectKind.ReadyExtraTurn || effect.Targeting == Targeting.OtherCombatant)
        {
            return "effect.kind readyExtraTurn and effect.targeting otherCombatant are for skills only; an item has no turn order.";
        }

        // A rest is the party's, at an inn or from the menu: battle refuses it, and it takes everyone.
        if (effect.Kind == EffectKind.Rest && (!isUsableFromMenu || effect.Targeting != Targeting.AllAllies))
        {
            return "effect.kind rest needs isUsableFromMenu true and effect.targeting allAllies; a rest is used from the menu on the whole party.";
        }

        // An item can be used from the menu, where there is no formation to measure a range along.
        if (effect.Targeting == Targeting.AlliesInRange)
        {
            return "effect.targeting alliesInRange is for skills only; an item has no battle formation to reach along.";
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

        return equipment.DefenseBonus < 0 ? $"equipment.defenseBonus must be at least 0 (was {equipment.DefenseBonus})." : null;
    }
}
