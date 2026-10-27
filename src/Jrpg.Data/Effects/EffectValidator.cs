using Jrpg.Core.Effects;
using Jrpg.Data.Statuses;

namespace Jrpg.Data.Effects;

/// <summary>Rules every effect must follow, whether a skill or an item carries it.</summary>
internal static class EffectValidator
{
    public static string? FindProblem(Effect effect) =>
        FindPowerProblem(effect)
        ?? FindRangeProblem(effect)
        ?? FindHitsProblem(effect)
        ?? FindKindProblem(effect)
        ?? FindDamageTypeProblem(effect)
        ?? (effect.Status is { } grant ? StatusGrantValidator.FindProblem(grant) : null);

    private static string? FindPowerProblem(Effect effect)
    {
        if (effect.Power < 0)
        {
            return $"effect.power must be at least 0 (was {effect.Power}).";
        }

        // Reviving with 0 HP would leave the target fallen, and Vitals.Revive rejects it.
        return effect.Kind == EffectKind.Revive && effect.Power < 1
            ? $"effect.power must be at least 1 for a revive effect (was {effect.Power})."
            : null;
    }

    private static string? FindRangeProblem(Effect effect)
    {
        if (effect.Range < 0)
        {
            return $"effect.range must be at least 0 (was {effect.Range}).";
        }

        // Only an aura reaches along the formation, so a range on any other targeting would be ignored.
        return effect.Range > 0 && effect.Targeting != Targeting.AlliesInRange
            ? $"effect.range is only for an alliesInRange effect (was {effect.Range} on {effect.Targeting})."
            : null;
    }

    // Only damage is resolved per hit; a heal or a status landing again would just repeat itself.
    private static string? FindHitsProblem(Effect effect) =>
        effect.Hits < 1 ? $"effect.hits must be at least 1 (was {effect.Hits})."
        : effect.Hits > 1 && effect.Kind != EffectKind.Damage ? $"effect.hits above 1 is only for a damage effect (was {effect.Hits} on {effect.Kind})."
        : null;

    // Only a hit meets a resistance (ADR 0027), so every damage effect says what it hits as and nothing else does.
    private static string? FindDamageTypeProblem(Effect effect) =>
        effect.Kind == EffectKind.Damage && effect.DamageType is null ? "effect.damageType is required for a damage effect."
        : effect.Kind != EffectKind.Damage && effect.DamageType is { } type ? $"effect.damageType is only for a damage effect (was {type} on {effect.Kind})."
        : null;

    private static string? FindKindProblem(Effect effect)
    {
        // Ready places its user's extra turn after someone else's, so it can only be aimed at another combatant.
        if (effect.Kind == EffectKind.ReadyExtraTurn && effect.Targeting != Targeting.OtherCombatant)
        {
            return $"effect.targeting must be otherCombatant for a readyExtraTurn effect (was {effect.Targeting}).";
        }

        // A grantStatus effect changes nothing else, so without a status it would do nothing.
        return effect.Kind == EffectKind.GrantStatus && effect.Status is null
            ? "effect.status is required for a grantStatus effect."
            : null;
    }
}
