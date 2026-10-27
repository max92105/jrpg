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
            ?? resources.Select(resource => FindDominanceProblem(resource, resources)).FirstOrDefault(problem => problem is not null)
            ?? resources.Select(resource => FindStrainProblem(resource, resources)).FirstOrDefault(problem => problem is not null);
    }

    private static string? FindProblem(ResourceDefinition resource)
    {
        var problem =
            string.IsNullOrWhiteSpace(resource.Name) ? "name must not be empty."
            : FindColorProblem(resource) ?? FindAmountProblem(resource) ?? FindRuleProblem(resource) ?? FindWillProblem(resource);
        return problem is null ? null : $"resource '{resource.Id}': {problem}";
    }

    private static string? FindColorProblem(ResourceDefinition resource) =>
        resource.Color is { Length: 7 } color && color[0] == '#' && color[1..].All(char.IsAsciiHexDigit)
            ? null
            : $"color must be # followed by six hex digits (was '{resource.Color}').";

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

    // A lasting resource's max lives on the member between battles; raising it with Will waits for W1b.
    private static string? FindWillProblem(ResourceDefinition resource) =>
        resource.WillScaling is not { } scaling ? null
        : scaling.WillPerPoint < 1 ? $"willScaling.willPerPoint must be at least 1 (was {scaling.WillPerPoint})."
        : resource.IsLasting ? "willScaling isn't for a lasting resource yet."
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

    // The source's missing points are what a member carries between battles, so only a lasting source has any. Its
    // target starts each battle afresh, so the lowering stays the same rather than growing.
    private static string? FindStrainProblem(ResourceDefinition resource, IReadOnlyList<ResourceDefinition> resources)
    {
        if (resource.Strain is not { } strain)
        {
            return null;
        }

        var problem =
            !resource.IsLasting ? "strain is only for a lasting resource."
            : !resources.Any(other => other.Id == strain.LowersStartOf && other != resource)
                ? $"strain.lowersStartOf must name another resource of the class (was '{strain.LowersStartOf}')."
            : resources.First(other => other.Id == strain.LowersStartOf).IsLasting
                ? $"strain.lowersStartOf can't name a lasting resource ('{strain.LowersStartOf}'), whose lowered start would carry on and drop again."
            : strain.PerMissingPoint < 1 ? $"strain.perMissingPoint must be at least 1 (was {strain.PerMissingPoint})."
            : null;
        return problem is null ? null : $"resource '{resource.Id}': {problem}";
    }

    // The speed gives both numbers, so a max or a start written beside it would be ignored.
    private static string? FindSpeedScalingProblem(ResourceDefinition resource, SpeedScaling scaling) =>
        resource.Max != 0 || resource.StartsAt != 0 ? "set either max and startsAt, or maxFromSpeed, not both."
        : resource.IsLasting ? "a lasting resource can't take its max from speed, which changes with level."
        : scaling.SpeedPerCharge < 1 ? $"maxFromSpeed.speedPerCharge must be at least 1 (was {scaling.SpeedPerCharge})."
        : null;
}
