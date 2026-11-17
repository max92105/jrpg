using Jrpg.Core.Content;
using Jrpg.Core.Effects;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Effects;

/// <summary>
/// One entry of a Tier box (ADR 0056): a tiered effect record with its line, or none for a flat power.
/// </summary>
/// <param name="Id">The record's id; empty for none.</param>
/// <param name="Line">"fire_1: damage, fire, x2", "heal_2: restoreHp, x4", or "(none: a flat power)".</param>
/// <param name="Tier">The record; null for none.</param>
internal sealed record TierChoice(string Id, string Line, TieredEffect? Tier)
{
    /// <returns>
    /// None first, then the content's records by id (the catalog keeps no file order). A skill names only a record the
    /// content holds, Data refusing an unknown one at load, so a box over these never misses its value.
    /// </returns>
    public static IReadOnlyList<TierChoice> AllIn(ContentCatalog content) =>
    [
        new("", "(none: a flat power)", null),
        .. content.TieredEffects.Values
            .OrderBy(tier => tier.Id, StringComparer.Ordinal)
            .Select(tier => new TierChoice(tier.Id, LineOf(tier), tier)),
    ];

    // Spelled as the content files spell the values, which the Effects page's records show.
    private static string LineOf(TieredEffect tier) =>
        tier.DamageType is { } type
            ? $"{tier.Id}: {ContentSpelling.Of(tier.Kind)}, {ContentSpelling.Of(type)}, x{tier.PerPower}"
            : $"{tier.Id}: {ContentSpelling.Of(tier.Kind)}, x{tier.PerPower}";
}
