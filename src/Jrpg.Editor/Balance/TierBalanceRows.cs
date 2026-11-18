using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Core.Effects;
using Jrpg.Core.Stats;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Balance;

/// <summary>
/// The balance table's tier sheet (decisions 704, 992): each <c>effects.json</c> record at the typed Powers, Power x
/// <c>perPower</c> x (1 + modifiers), read through the same Core rule as the skills' amounts (decision 993).
/// </summary>
internal static class TierBalanceRows
{
    /// <returns>The records in id order (the catalog keeps no file order).</returns>
    /// <param name="content">The catalog whose tiered effects the rows read.</param>
    /// <param name="powers">The table's Powers in the boxes' order, each from 0.</param>
    /// <param name="modifierPermille">
    /// The table's damage modifier, which Core adds to damage and never to healing.
    /// </param>
    public static IReadOnlyList<TierBalanceRow> Of(ContentCatalog content, IReadOnlyList<int> powers, int modifierPermille) =>
    [
        .. content.TieredEffects.Values
            .OrderBy(tier => tier.Id, StringComparer.Ordinal)
            .Select(tier => RowOf(tier, powers, modifierPermille)),
    ];

    private static TierBalanceRow RowOf(TieredEffect tier, IReadOnlyList<int> powers, int modifierPermille)
    {
        // A skill naming the record: its amount reads neither the targeting nor the raw power.
        var effect = new Effect(tier.Kind, 0, Targeting.SingleEnemy, DamageType: tier.DamageType) { Tier = tier };
        return new TierBalanceRow(
            tier.Id,
            KindLineOf(tier),
            tier.PerPower,
            [
                .. powers.Select(power => EffectAmounts.PerHit(
                    effect,
                    new CombatStats(0) { Power = power, DamageModifierPermille = modifierPermille })),
            ],
            tier.Kind != EffectKind.Damage);
    }

    // Spelled as the content files spell the values, as the Effects page's Tier box does.
    private static string KindLineOf(TieredEffect tier) =>
        tier.DamageType is { } type
            ? $"{ContentSpelling.Of(tier.Kind)}, {ContentSpelling.Of(type)}"
            : ContentSpelling.Of(tier.Kind);
}
