using Jrpg.Core.Stats;

namespace Jrpg.Core.Items;

/// <summary>
/// One line on a gear piece: rolled, or fixed on a unique item (ADR 0058). Its stat is resolved from the affix pool
/// when the piece is rolled or loaded, or when a unique's record loads, so the piece's bonuses need no catalog
/// afterwards (ADR 0029).
/// </summary>
/// <param name="AffixId">The pool affix it was rolled from, or the one a unique names.</param>
/// <param name="Stat">What it raises.</param>
/// <param name="Value">
/// In the stat's bonus unit, as an item's bonuses: whole points for a primary, max HP or speed, whole percents for the
/// rest (ADR 0026).
/// </param>
public sealed record RolledAffix(string AffixId, StatKind Stat, int Value);
