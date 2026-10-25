using Jrpg.Core.Effects;
using Jrpg.Data.Statuses;

namespace Jrpg.Data.Effects;

/// <summary>Rules every effect must follow, whether a skill or an item carries it.</summary>
internal static class EffectValidator
{
    public static string? FindProblem(Effect effect)
    {
        if (effect.Power < 0)
        {
            return $"effect.power must be at least 0 (was {effect.Power}).";
        }

        // Reviving with 0 HP would leave the target fallen, and Vitals.Revive rejects it.
        if (effect.Kind == EffectKind.Revive && effect.Power < 1)
        {
            return $"effect.power must be at least 1 for a revive effect (was {effect.Power}).";
        }

        // A grantStatus effect changes nothing else, so without a status it would do nothing.
        if (effect.Kind == EffectKind.GrantStatus && effect.Status is null)
        {
            return "effect.status is required for a grantStatus effect.";
        }

        return effect.Status is { } grant ? StatusGrantValidator.FindProblem(grant) : null;
    }
}
