using Jrpg.Core.Stats;

namespace Jrpg.Core.Items;

/// <summary>
/// One rolled line on a gear piece. Its stat is resolved from the affix pool when the piece is rolled or loaded, so the
/// piece's bonuses need no catalog afterwards (ADR 0029).
/// </summary>
/// <param name="AffixId">The affix in the pool it was rolled from.</param>
/// <param name="Stat">What it raises.</param>
/// <param name="Value">
/// In the stat's bonus unit, as an item's bonuses: whole points for a primary, max HP or speed, whole percents for the
/// rest (ADR 0026).
/// </param>
public sealed record RolledAffix(string AffixId, StatKind Stat, int Value);
