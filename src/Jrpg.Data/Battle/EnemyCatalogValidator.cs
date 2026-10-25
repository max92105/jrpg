using Jrpg.Core.Battle;
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
                ?? AtLeast("attack", enemy.Attack, 0)
                ?? AtLeast("defense", enemy.Defense, 0)
                ?? AtLeast("experienceReward", enemy.ExperienceReward, 0);
}
