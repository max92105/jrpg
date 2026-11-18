namespace Jrpg.Editor.Balance;

/// <summary>
/// One tiered effect record in the balance table's tier sheet, with one hit's amount at each typed Power.
/// </summary>
/// <param name="TierId">The record's id, such as <c>fire_1</c>.</param>
/// <param name="KindLine">"damage, fire" for a damage record and its type, "restoreHp" for a heal.</param>
/// <param name="PerPower">What each point of Power gives.</param>
/// <param name="Amounts">One per Power in the boxes' order, the typed modifier raising damage only.</param>
/// <param name="IsHeal">
/// A heal, which no damage modifier changes (decision 786); the page mutes it (decision 994).
/// </param>
internal sealed record TierBalanceRow(
    string TierId,
    string KindLine,
    int PerPower,
    IReadOnlyList<int> Amounts,
    bool IsHeal);
