using Jrpg.Core.Maps;
using Jrpg.Data.Content;

namespace Jrpg.Data.Maps;

internal static class MapSettingsValidator
{
    /// <summary>Checks one settings file; <paramref name="fileName"/> is its path under the content folder, and the map id must equal its file name.</summary>
    public static void Validate(string fileName, MapSettings settings, IReadOnlySet<string> encounterIds)
    {
        var problem = FindIdentityProblem(fileName, settings) ?? FindEncounterProblem(settings.RandomEncounters, encounterIds);
        if (problem is not null)
        {
            throw new ContentValidationException($"{fileName}: {problem}");
        }
    }

    private static string? FindIdentityProblem(string fileName, MapSettings settings)
    {
        var expectedId = Path.GetFileNameWithoutExtension(fileName);
        if (settings.Id != expectedId)
        {
            return $"id must equal the file name '{expectedId}' (was '{settings.Id}').";
        }

        return string.IsNullOrWhiteSpace(settings.DisplayName) ? "displayName must not be empty." : null;
    }

    private static string? FindEncounterProblem(RandomEncounterTable? table, IReadOnlySet<string> encounterIds)
    {
        if (table is null)
        {
            return null;
        }

        // At least one grace step, because the developer wants no battle right after another (decision 26).
        var problem = RangeProblems.AtLeast("randomEncounters.oneInSteps", table.OneInSteps, minimum: 1)
            ?? RangeProblems.AtLeast("randomEncounters.graceSteps", table.GraceSteps, minimum: 1);
        if (problem is not null)
        {
            return problem;
        }

        if (table.EncounterIds.Count == 0)
        {
            return "randomEncounters.encounterIds must list at least one encounter.";
        }

        // The serializer doesn't apply nullable annotations to collection elements, so a stray null gets here.
        if (table.EncounterIds.Contains(null))
        {
            return "randomEncounters.encounterIds holds a null entry.";
        }

        var unknownId = table.EncounterIds.FirstOrDefault(id => !encounterIds.Contains(id));
        return unknownId is null ? null : $"randomEncounters.encounterIds has unknown encounter id '{unknownId}'.";
    }
}
