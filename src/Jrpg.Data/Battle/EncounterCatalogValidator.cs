using Jrpg.Core.Battle;
using Jrpg.Data.Content;

namespace Jrpg.Data.Battle;

internal static class EncounterCatalogValidator
{
    public static void Validate(string fileName, IReadOnlyList<EncounterDefinition> encounters, IReadOnlySet<string> enemyIds) =>
        new ContentListValidator<EncounterDefinition>(fileName, "encounter", encounter => encounter.Id)
            .Validate(encounters, encounter => FindProblem(encounter, enemyIds));

    private static string? FindProblem(EncounterDefinition encounter, IReadOnlySet<string> enemyIds)
    {
        if (encounter.EnemyIds.Count == 0)
        {
            return "enemyIds must list at least one enemy.";
        }

        var unknownEnemyId = encounter.EnemyIds.FirstOrDefault(id => !enemyIds.Contains(id));
        return unknownEnemyId is null ? null : $"enemyIds has unknown enemy id '{unknownEnemyId}'.";
    }
}
