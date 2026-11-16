using System.Collections.Frozen;
using Jrpg.Core.Content;
using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Core.Skills;
using Jrpg.Core.Statuses;

namespace Jrpg.Data.Content;

/// <summary>
/// Points every content reference resolved at load (decision 789) at the catalog's records of the same id: a skill's or
/// an item's tiered effect and the statuses its effect grants. A record holds what it names, not an id, so an edited
/// effect or status reaches what names it only through here (decision 975); Data's checks and the editor's draft both
/// rebind with it.
/// </summary>
public static class CatalogReferences
{
    /// <summary>
    /// The catalog with its skills and items pointed at its own tiered effects and statuses; a skill or item already
    /// holding them stays the same instance.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// A skill or item names an id the catalog has no record of: steps undo in order, so a step removing a record is
    /// undone only after the steps naming it, and a page removes only the records it added, so a typed list never lacks
    /// one a skill names; this is a bug, never input.
    /// </exception>
    public static ContentCatalog Rebound(ContentCatalog catalog) =>
        catalog with
        {
            Skills = catalog.Skills.Values.Select(skill => Rebound(skill, catalog)).ToFrozenDictionary(skill => skill.Id),
            Items = catalog.Items.Values.Select(item => Rebound(item, catalog)).ToFrozenDictionary(item => item.Id),
        };

    private static SkillDefinition Rebound(SkillDefinition skill, ContentCatalog catalog)
    {
        var effect = Rebound(skill.Effect, catalog, $"skill '{skill.Id}'");
        return ReferenceEquals(effect, skill.Effect) ? skill : skill with { Effect = effect };
    }

    private static ItemDefinition Rebound(ItemDefinition item, ContentCatalog catalog)
    {
        if (item.Effect is not { } effect)
        {
            return item;
        }

        var rebound = Rebound(effect, catalog, $"item '{item.Id}'");
        return ReferenceEquals(rebound, effect) ? item : item with { Effect = rebound };
    }

    // Records compare by content, so only a reference check tells an old record from the catalog's equal one.
    private static Effect Rebound(Effect effect, ContentCatalog catalog, string owner)
    {
        var tier = effect.Tier is { } named ? RecordOf(catalog.TieredEffects, named.Id, owner, "effect") : null;
        var grants = effect.Statuses.Select(grant => Rebound(grant, catalog, owner)).ToList();
        return ReferenceEquals(tier, effect.Tier) && grants.SequenceEqual(effect.Statuses, ReferenceEqualityComparer.Instance)
            ? effect
            : effect with { Tier = tier, Statuses = grants };
    }

    private static StatusGrant Rebound(StatusGrant grant, ContentCatalog catalog, string owner)
    {
        var status = RecordOf(catalog.Statuses, grant.Status.Id, owner, "status");
        return ReferenceEquals(status, grant.Status) ? grant : grant with { Status = status };
    }

    private static T RecordOf<T>(IReadOnlyDictionary<string, T> records, string id, string owner, string recordName) =>
        records.TryGetValue(id, out var record)
            ? record
            : throw new InvalidOperationException($"{owner} names the {recordName} '{id}', which the catalog has no record of.");
}
