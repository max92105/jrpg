using Jrpg.Core.Battle;
using Jrpg.Core.Stats;
using Jrpg.Data.Content;
using static Jrpg.Data.Content.RangeProblems;

namespace Jrpg.Data.Battle;

internal static class EnemyCatalogValidator
{
    // Twice a sure hit: enough to beat any dodge under the cap and still mean something, nothing more (decision 299).
    private const int MostAccuracy = 200;
    private const int WholeChance = 100;

    /// <param name="fileName">The file, for messages.</param>
    /// <param name="enemies">The deserialized list.</param>
    /// <param name="dropTableIds">
    /// The drop tables an enemy may name; several enemies may share one (decision 343).
    /// </param>
    public static void Validate(string fileName, IReadOnlyList<EnemyDefinition> enemies, IReadOnlySet<string> dropTableIds) =>
        new ContentListValidator<EnemyDefinition>(fileName, "enemy", enemy => enemy.Id)
            .Validate(enemies, enemy => FindProblem(enemy) ?? FindDropTableProblem(enemy, dropTableIds));

    private static string? FindDropTableProblem(EnemyDefinition enemy, IReadOnlySet<string> dropTableIds) =>
        enemy.DropTableId is { } id && !dropTableIds.Contains(id) ? $"dropTableId has unknown drop table id '{id}'." : null;

    private static string? FindProblem(EnemyDefinition enemy) =>
        string.IsNullOrWhiteSpace(enemy.Name)
            ? "name must not be empty."
            : AtLeast("maxHp", enemy.MaxHp, 1)
                ?? AtLeast("damage", enemy.Damage, 0)
                ?? Within("accuracy", enemy.Accuracy, 0, MostAccuracy)
                ?? Within("dodge", enemy.Dodge, 0, StatFormula.DodgeCapPercent)
                ?? Within("critChance", enemy.CritChance, 0, WholeChance)
                ?? Within("crowdControlResistance", enemy.CrowdControlResistance, 0, WholeChance)
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
