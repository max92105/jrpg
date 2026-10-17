using Jrpg.Core.Effects;

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
        return effect.Kind == EffectKind.Revive && effect.Power < 1
            ? $"effect.power must be at least 1 for a revive effect (was {effect.Power})."
            : null;
    }
}
