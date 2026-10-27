using Jrpg.Core.Battle;
using Jrpg.Core.Stats;
using Jrpg.Data.Content;
using static Jrpg.Data.Content.RangeProblems;

namespace Jrpg.Data.Battle;

internal static class EnemyCatalogValidator
{
    public static void Validate(string fileName, IReadOnlyList<EnemyDefinition> enemies) =>
        new ContentListValidator<EnemyDefinition>(fileName, "enemy", enemy => enemy.Id)
            .Validate(enemies, FindProblem);

    private static string? FindProblem(EnemyDefinition enemy) =>
        string.IsNullOrWhiteSpace(enemy.Name)
            ? "name must not be empty."
            : AtLeast("maxHp", enemy.MaxHp, 1)
                ?? AtLeast("damage", enemy.Damage, 0)
                ?? AtLeast("experienceReward", enemy.ExperienceReward, 0)
                ?? FindResistanceProblem(enemy.Resistances);

    // Content within the floor and cap reads as it plays: past them the battle would clamp it silently (ADR 0027).
    private static string? FindResistanceProblem(Resistances resistances)
    {
        foreach (var type in Enum.GetValues<DamageType>())
        {
            var percent = StatFormula.WholePercentOf(resistances.Of(type));
            if (percent is < Resistances.FloorPercent or > Resistances.CapPercent)
            {
                return $"resistances.{ResistancesConverter.NameOf(type)} must be within {Resistances.FloorPercent}..{Resistances.CapPercent} (was {percent}).";
            }
        }

        return null;
    }
}
