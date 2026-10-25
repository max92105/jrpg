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

        return resources.Select(FindProblem).FirstOrDefault(problem => problem is not null);
    }

    private static string? FindProblem(ResourceDefinition resource)
    {
        var problem =
            string.IsNullOrWhiteSpace(resource.Name) ? "name must not be empty."
            : resource.Max < 1 ? $"max must be at least 1 (was {resource.Max})."
            : resource.StartsAt < 0 || resource.StartsAt > resource.Max
                ? $"startsAt must be within 0..{resource.Max} (was {resource.StartsAt})."
            : null;
        return problem is null ? null : $"resource '{resource.Id}': {problem}";
    }
}
