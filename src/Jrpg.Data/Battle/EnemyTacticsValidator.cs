using Jrpg.Core.Battle;
using Jrpg.Core.Skills;
using Jrpg.Data.Content;
using static Jrpg.Data.Content.RangeProblems;

namespace Jrpg.Data.Battle;

/// <summary>
/// An enemy's actions and phases: their thresholds, and every action against the skills (decisions 411, 412).
/// </summary>
internal static class EnemyTacticsValidator
{
    /// <param name="tactics">The actions, in content's order.</param>
    /// <param name="field">The list's name in the file, for messages.</param>
    /// <param name="skills">The skills an action may name.</param>
    /// <returns>The first problem, naming the field and the index; null when there is none.</returns>
    public static string? FindProblem(IReadOnlyList<EnemyAction> tactics, string field, IReadOnlyDictionary<string, SkillDefinition> skills)
    {
        for (var index = 0; index < tactics.Count; index++)
        {
            if (FindActionProblem(tactics[index], $"{field}[{index}]", skills) is { } problem)
            {
                return problem;
            }
        }

        return null;
    }

    /// <param name="phases">The phases, in content's order: strictly descending thresholds.</param>
    /// <param name="skills">The skills their actions may name.</param>
    /// <returns>The first problem, naming the phase by index; null when there is none.</returns>
    public static string? FindPhasesProblem(IReadOnlyList<EnemyPhase> phases, IReadOnlyDictionary<string, SkillDefinition> skills)
    {
        for (var index = 0; index < phases.Count; index++)
        {
            if (FindPhaseProblem(phases, index, skills) is { } problem)
            {
                return problem;
            }
        }

        return null;
    }

    private static string? FindPhaseProblem(IReadOnlyList<EnemyPhase> phases, int index, IReadOnlyDictionary<string, SkillDefinition> skills)
    {
        var field = $"phases[{index}]";
        EnemyPhase? phase = phases[index];
        if (phase is null)
        {
            return $"{field} must not be null.";
        }

        if (phase.Tactics.Count == 0)
        {
            return $"{field}.tactics must not be empty.";
        }

        return Within($"{field}.belowHpPercent", phase.BelowHpPercent, 1, 99)
            ?? (index > 0 && phase.BelowHpPercent >= phases[index - 1].BelowHpPercent
                ? $"{field}.belowHpPercent must be below the phase before it ({phases[index - 1].BelowHpPercent}, was {phase.BelowHpPercent})."
                : null)
            ?? FindProblem(phase.Tactics, $"{field}.tactics", skills);
    }

    // The strict serializer lets a null element through, so the entry's type here says it may be one.
    private static string? FindActionProblem(EnemyAction? action, string field, IReadOnlyDictionary<string, SkillDefinition> skills) =>
        action is null
            ? $"{field} must not be null."
            : AtLeast($"{field}.weight", action.Weight, 1)
                ?? FindConditionProblem(action.When, $"{field}.when")
                ?? FindSkillProblem(action.SkillId, field, skills)
                ?? FindTargetProblem(action, field, skills);

    // A percent of 0 or 100 would make an HP condition never or nearly always hold, so content says what it means.
    private static string? FindConditionProblem(TacticCondition? condition, string field)
    {
        if (condition is null)
        {
            return null;
        }

        return TacticConditionKinds.ShapeOf(condition.Kind).Number switch
        {
            TacticConditionNumber.Percent => Within($"{field}.percent", condition.Percent, 1, 99),
            TacticConditionNumber.None when condition.Percent != 0 =>
                $"{field}.percent must be left out of {ContentJsonOptions.EnumName(condition.Kind)} (was {condition.Percent}).",
            TacticConditionNumber.None => null,
            _ => throw new ArgumentOutOfRangeException(nameof(condition), condition.Kind, "Unknown tactic condition number."),
        };
    }

    // Runs after the skill's own check, so a skill id the action names is known by then.
    private static string? FindTargetProblem(
        EnemyAction action, string field, IReadOnlyDictionary<string, SkillDefinition> skills)
    {
        var skill = action.SkillId is { } skillId ? skills[skillId] : null;
        if (action.On is not { } target || TacticTargetFit.Fits(action, skill))
        {
            return null;
        }

        var label = action.SkillId ?? "attack";
        var named = $"{field} ({label}).on: {ContentJsonOptions.EnumName(target)}";
        return target == TacticTarget.Subject && action.When is null
            ? $"{named} needs a when that names someone."
            : $"{named} doesn't fit {label}, which aims at {ContentJsonOptions.EnumName(TacticTargetFit.AimOf(skill))}.";
    }

    private static string? FindSkillProblem(string? skillId, string field, IReadOnlyDictionary<string, SkillDefinition> skills)
    {
        if (skillId is null)
        {
            return null;
        }

        return skills.TryGetValue(skillId, out var skill)
            ? FindUsabilityProblem(skill, $"{field}.skillId '{skillId}'")
            : $"{field}.skillId has unknown skill id '{skillId}'.";
    }

    private static string? FindUsabilityProblem(SkillDefinition skill, string named) =>
        EnemySkillUse.RefusalOf(skill) switch
        {
            null => null,
            EnemySkillRefusal.UsesClassResource => $"{named} uses a class resource, and enemies have none.",
            EnemySkillRefusal.AimsAlongFormation => $"{named} aims along the party's formation, which enemies have none of.",
            EnemySkillRefusal.PlacesReadiedTurn => $"{named} places a readied turn, a rogue's resource mechanic.",
            { } refusal => throw new ArgumentOutOfRangeException(nameof(skill), refusal, "Unknown enemy skill refusal."),
        };
}
