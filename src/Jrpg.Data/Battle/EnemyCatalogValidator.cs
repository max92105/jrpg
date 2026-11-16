using Jrpg.Core.Battle;
using Jrpg.Core.Maps;
using Jrpg.Core.Party;
using Jrpg.Core.Skills;
using Jrpg.Core.Stats;
using Jrpg.Data.Content;
using Jrpg.Data.Maps;
using static Jrpg.Data.Content.RangeProblems;

namespace Jrpg.Data.Battle;

internal static class EnemyCatalogValidator
{
    // Twice a sure hit: enough to beat any dodge under the cap and still mean something, nothing more (decision 299).
    private const int MostAccuracy = 200;
    private const int WholeChance = 100;

    // A bound against overflow: Power times a record's per-Power stays far within an int (decision 818).
    private const int MostPower = 10_000;

    /// <param name="fileName">The file, for messages.</param>
    /// <param name="enemies">The deserialized list.</param>
    /// <param name="references">What an enemy may name.</param>
    public static void Validate(string fileName, IReadOnlyList<EnemyDefinition> enemies, EnemyReferences references) =>
        new ContentListValidator<EnemyDefinition>(fileName, "enemy", enemy => enemy.Id)
            .Validate(enemies, enemy => FindProblem(enemy)
                ?? FindDropTableProblem(enemy, references.DropTableIds)
                ?? FindIdleAnimationProblem(enemy.IdleAnimation)
                ?? EnemyTacticsValidator.FindProblem(enemy.Tactics, "tactics", references.Skills)
                ?? EnemyTacticsValidator.FindPhasesProblem(enemy.Phases, references.Skills)
                ?? FindPowerProblem(enemy, references.Skills));

    // A tiered skill multiplies its user's Power, so an enemy using one with none would deal nothing (decision 787).
    // Checked after the tactics, which refuse a null action or an unknown skill first.
    private static string? FindPowerProblem(EnemyDefinition enemy, IReadOnlyDictionary<string, SkillDefinition> skills)
    {
        if (Within("power", enemy.Power, 0, MostPower) is { } range)
        {
            return range;
        }

        // The first in its tactics, then its phases', so the message names the same skill every load.
        var tiered = enemy.Power > 0
            ? null
            : enemy.Tactics.Concat(enemy.Phases.SelectMany(phase => phase.Tactics))
                .Select(action => action.SkillId)
                .FirstOrDefault(id => id is not null && skills.TryGetValue(id, out var skill) && skill.Effect.Tier is not null);
        return tiered is null ? null : $"power must be at least 1: its skill '{tiered}' names a tiered effect (was 0).";
    }

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
                ?? FindGoldProblem(enemy.Gold)
                ?? FindResistanceProblem(enemy.Resistances);

    // ADR 0047: whether the strip splits into its frames is the startup art check's (ArtReferenceCheck).
    private static string? FindIdleAnimationProblem(TileAnimation? idle) =>
        idle is null ? null : AnimationProblems.Find("idleAnimation", idle);

    // Both ends within the purse (ADR 0031); the max is checked from the min, so a max below it is named.
    private static string? FindGoldProblem(GoldRange? gold) =>
        gold is null ? null : Within("gold.min", gold.Min, 0, Purse.MaxGold) ?? Within("gold.max", gold.Max, gold.Min, Purse.MaxGold);

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
