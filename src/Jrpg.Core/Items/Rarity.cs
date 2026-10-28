namespace Jrpg.Core.Items;

/// <summary>
/// A gear piece's rarity, lowest first (decision 339). It sets how many affixes a dropped piece rolls (decision 340).
/// Unique is reserved for hand-made items and never rolls in the loot plan.
/// </summary>
public enum Rarity
{
    Common,
    Magic,
    Rare,
    Unique,
}
