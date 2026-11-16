using Jrpg.Core.Statuses;

namespace Jrpg.Data.Statuses;

/// <summary>Rules for a status an effect grants (ADR 0021); the effect's validator names where it sits.</summary>
internal static class StatusGrantValidator
{
    private const int WholeChance = 100;

    /// <param name="grant">The grant.</param>
    /// <param name="index">Its place in the effect's statuses, which the messages name.</param>
    public static string? FindProblem(StatusGrant grant, int index) =>
        grant.Turns < 1 ? $"effect.statuses[{index}].turns must be at least 1 (was {grant.Turns})."
        : grant.Power < 0 ? $"effect.statuses[{index}].power must be at least 0 (was {grant.Power})."
        : grant.Chance is < 1 or > WholeChance
            ? $"effect.statuses[{index}].chance must be within 1..{WholeChance} (was {grant.Chance})."
        : null;
}
