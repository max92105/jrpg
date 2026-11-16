using Jrpg.Core.Content;

namespace Jrpg.Editor.Skills;

/// <summary>One resource the Skills page's cost boxes offer (decision 979).</summary>
/// <param name="Id">The resource's id, which a cost row names.</param>
/// <param name="Label">"Mana (mage, warlock)": the resource's name and every class having it.</param>
internal sealed record ResourceChoice(string Id, string Label)
{
    /// <returns>
    /// Every class's resources, the classes by id (the catalog keeps no file order) and each class's in its own order,
    /// each resource once, named as the first class having it names it; then each resource a skill names that no class
    /// has, by id, labelled "(no class)". A box over a value its list lacks keeps showing the previously chosen
    /// skill's, and Data lets a skill no class learns name any resource.
    /// </returns>
    public static IReadOnlyList<ResourceChoice> AllIn(ContentCatalog content)
    {
        var owned = content.Classes.Values
            .OrderBy(owner => owner.Id, StringComparer.Ordinal)
            .SelectMany(owner => owner.Resources.Select(resource => (Owner: owner.Id, Resource: resource)))
            .GroupBy(held => held.Resource.Id, StringComparer.Ordinal)
            .Select(group => new ResourceChoice(
                group.Key,
                $"{group.First().Resource.Name} ({string.Join(", ", group.Select(held => held.Owner))})"))
            .ToList();
        var unowned = content.Skills.Values
            .SelectMany(skill => skill.ResourceChanges.Select(change => change.ResourceId)
                .Concat(skill.Requirements.Select(requirement => requirement.ResourceId)))
            .Where(id => !owned.Exists(choice => choice.Id == id))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .Select(id => new ResourceChoice(id, $"{id} (no class)"));
        return [.. owned, .. unowned];
    }
}
