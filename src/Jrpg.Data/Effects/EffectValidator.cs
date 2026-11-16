using Jrpg.Core.Effects;
using Jrpg.Core.Stats;
using Jrpg.Core.Statuses;
using Jrpg.Data.Content;
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
        ?? (effect.Tier is { } tier ? FindTierProblem(effect, tier) : null)
        ?? FindStatusesProblem(effect.Statuses);

    // A tiered effect only sets a damage's or a heal's amount, and the effect repeats its record's kind and type, which
    // the record decides (ADR 0056); the effect's own power is left at 0 (decision 792).
    private static string? FindTierProblem(Effect effect, TieredEffect tier) =>
        effect.Kind is not (EffectKind.Damage or EffectKind.RestoreHp)
            ? $"effect.tier is only for a damage or restoreHp effect (was {tier.Id} on {NameOf(effect.Kind)})."
        : effect.Kind != tier.Kind
            ? $"effect.kind is {NameOf(effect.Kind)} but {tier.Id} is {NameOf(tier.Kind)}."
        : effect.DamageType != tier.DamageType
            ? $"effect.damageType is {NameOf(effect.DamageType)} but {tier.Id} is {NameOf(tier.DamageType)}."
        : effect.Power != 0 ? $"effect.power must be 0 beside a tiered effect, which sets the amount (was {effect.Power})."
        : null;

    private static string NameOf(EffectKind kind) => ContentJsonOptions.EnumName(kind);

    private static string NameOf(DamageType? type) => type is { } named ? ContentJsonOptions.EnumName(named) : "none";

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
        return effect.Kind == EffectKind.GrantStatus && effect.Statuses.Count == 0
            ? "effect.statuses must list at least one status for a grantStatus effect."
            : null;
    }

    // The serializer lets a null list element through; one status twice would refresh itself on landing (decision 809).
    private static string? FindStatusesProblem(IReadOnlyList<StatusGrant> grants)
    {
        if (grants.Any(grant => grant is null))
        {
            return "effect.statuses must not contain a null entry.";
        }

        var repeated = grants.GroupBy(grant => grant.Status.Id).FirstOrDefault(group => group.Count() > 1);
        if (repeated is not null)
        {
            return $"effect.statuses lists {repeated.Key} twice.";
        }

        return grants
            .Select((grant, index) => StatusGrantValidator.FindProblem(grant, index))
            .FirstOrDefault(problem => problem is not null);
    }
}
