using Jrpg.Core.Statuses;

namespace Jrpg.Data.Statuses;

/// <summary>Rules for a status an effect grants (ADR 0021); the effect's validator names where it sits.</summary>
internal static class StatusGrantValidator
{
    private const int WholeChance = 100;

    public static string? FindProblem(StatusGrant grant) =>
        grant.Turns < 1 ? $"effect.status.turns must be at least 1 (was {grant.Turns})."
        : grant.Power < 0 ? $"effect.status.power must be at least 0 (was {grant.Power})."
        : grant.Kind == StatusKind.Stun && grant.Power is < 1 or > WholeChance
            ? $"effect.status.power must be within 1..{WholeChance} for a stun, its chance to land (was {grant.Power})."
        : null;
}
