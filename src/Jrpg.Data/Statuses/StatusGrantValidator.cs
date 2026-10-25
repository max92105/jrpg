using Jrpg.Core.Statuses;

namespace Jrpg.Data.Statuses;

/// <summary>Rules for a status an effect grants (ADR 0021); the effect's validator names where it sits.</summary>
internal static class StatusGrantValidator
{
    public static string? FindProblem(StatusGrant grant) =>
        grant.Turns < 1 ? $"effect.status.turns must be at least 1 (was {grant.Turns})."
        : grant.Power < 0 ? $"effect.status.power must be at least 0 (was {grant.Power})."
        : null;
}
