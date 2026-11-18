namespace Jrpg.Editor.Balance;

/// <summary>One skill of one class in the balance table, with what it comes to at each of the table's levels.</summary>
/// <param name="ClassName">The class's name.</param>
/// <param name="SkillName">The skill's name.</param>
/// <param name="TierLine">"fire_1 x2" for a tier and its per-Power, "flat 15" for a raw power.</param>
/// <param name="Hits">How many times it lands on each target.</param>
/// <param name="IsHeal">Healing or revive, which no damage modifier changes (decision 786); the page mutes it.</param>
/// <param name="AmountTexts">
/// One per level in the boxes' order: "24", "28 x 4 = 112" with hits, or "-" below the level the class learns it at.
/// </param>
internal sealed record SkillBalanceRow(
    string ClassName,
    string SkillName,
    string TierLine,
    int Hits,
    bool IsHeal,
    IReadOnlyList<string> AmountTexts);
