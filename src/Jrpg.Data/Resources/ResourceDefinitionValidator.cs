using Jrpg.Core.Resources;

namespace Jrpg.Data.Resources;

/// <summary>Checks one class's battle resources (ADR 0019); the class validator names the class.</summary>
internal static class ResourceDefinitionValidator
{
    /// <returns>The first problem, or null when every resource is valid.</returns>
    public static string? FindProblem(IReadOnlyList<ResourceDefinition> resources)
    {
        // The JSON reader lets a null list element through.
        if (resources.Any(resource => resource is null))
        {
            return "resources must not contain a null entry.";
        }

        if (resources.Any(resource => string.IsNullOrWhiteSpace(resource.Id)))
        {
            return "resources must not contain an empty id.";
        }

        var repeatedId = resources.GroupBy(resource => resource.Id).FirstOrDefault(group => group.Count() > 1)?.Key;
        if (repeatedId is not null)
        {
            return $"resources lists '{repeatedId}' more than once.";
        }

        return resources.Select(FindProblem).FirstOrDefault(problem => problem is not null)
            ?? resources.Select(resource => FindDominanceProblem(resource, resources)).FirstOrDefault(problem => problem is not null);
    }

    private static string? FindProblem(ResourceDefinition resource)
    {
        var problem =
            string.IsNullOrWhiteSpace(resource.Name) ? "name must not be empty."
            : FindAmountProblem(resource) ?? FindRuleProblem(resource);
        return problem is null ? null : $"resource '{resource.Id}': {problem}";
    }

    // Either a fixed max and start, or both from the holder's speed.
    private static string? FindAmountProblem(ResourceDefinition resource) =>
        resource.MaxFromSpeed is { } scaling ? FindSpeedScalingProblem(resource, scaling)
        : resource.Max < 1 ? $"max must be at least 1 (was {resource.Max})."
        : resource.StartsAt < 0 || resource.StartsAt > resource.Max
            ? $"startsAt must be within 0..{resource.Max} (was {resource.StartsAt})."
        : null;

    private static string? FindRuleProblem(ResourceDefinition resource) =>
        resource.Gains.PerHeavyHit != 0 && resource.Gains.HeavyHitPercent is < 1 or > 100
            ? $"gains.heavyHitPercent must be within 1..100 when perHeavyHit is set (was {resource.Gains.HeavyHitPercent})."
        : resource.Backfire is { } backfire
            && (backfire.ChancePerPointPercent is < 1 or > 100 || backfire.DamagePercentOfMaxHp is < 1 or > 100)
            ? $"backfire percents must be within 1..100 (were {backfire.ChancePerPointPercent} and {backfire.DamagePercentOfMaxHp})."
        : null;

    // Dominance is judged between two forces, so each must name the other.
    private static string? FindDominanceProblem(ResourceDefinition resource, IReadOnlyList<ResourceDefinition> resources)
    {
        if (resource.Dominance is not { } rule)
        {
            return null;
        }

        var rival = resources.FirstOrDefault(other => other.Id == rule.Over && other != resource);
        var problem =
            rival is null ? $"dominance.over must name another resource of the class (was '{rule.Over}')."
            : rival.Dominance?.Over != resource.Id ? $"dominance.over names '{rule.Over}', whose dominance doesn't name '{resource.Id}' back."
            : rule.BonusPercent < 0 ? $"dominance.bonusPercent must be at least 0 (was {rule.BonusPercent})."
            : null;
        return problem is null ? null : $"resource '{resource.Id}': {problem}";
    }

    // The speed gives both numbers, so a max or a start written beside it would be ignored.
    private static string? FindSpeedScalingProblem(ResourceDefinition resource, SpeedScaling scaling) =>
        resource.Max != 0 || resource.StartsAt != 0 ? "set either max and startsAt, or maxFromSpeed, not both."
        : scaling.SpeedPerCharge < 1 ? $"maxFromSpeed.speedPerCharge must be at least 1 (was {scaling.SpeedPerCharge})."
        : null;
}
