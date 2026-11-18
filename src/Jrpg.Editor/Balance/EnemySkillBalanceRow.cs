namespace Jrpg.Editor.Balance;

/// <summary>
/// One skill an enemy's tactics or phases name in the balance table, with what it comes to at its power.
/// </summary>
/// <param name="EnemyName">The enemy's name.</param>
/// <param name="Power">The enemy's power, what its tiered skills multiply.</param>
/// <param name="SkillName">The skill's name.</param>
/// <param name="TierLine">"fire_1 x2" for a tier and its per-Power, "flat 15" for a raw power.</param>
/// <param name="Hits">How many times it lands on each target.</param>
/// <param name="IsHeal">Healing or revive, which no damage modifier changes (decision 786); the page mutes it.</param>
/// <param name="AmountText">"40", or "40 x 4 = 160" with hits.</param>
internal sealed record EnemySkillBalanceRow(
    string EnemyName,
    int Power,
    string SkillName,
    string TierLine,
    int Hits,
    bool IsHeal,
    string AmountText);
