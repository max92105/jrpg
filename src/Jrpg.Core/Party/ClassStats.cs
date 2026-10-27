using Jrpg.Core.Stats;

namespace Jrpg.Core.Party;

/// <summary>
/// Max HP, attack, defense, speed, accuracy and the five primaries: what a class gives at level 1, or what each level
/// after adds. Speed only sets the rogue's readiness charges for now (question 213).
/// </summary>
public sealed record ClassStats(int MaxHp, int Attack, int Defense, int Speed)
{
    /// <summary>The five primaries (ADR 0026); none when content leaves them out.</summary>
    public PrimaryStats Primaries { get; init; } = PrimaryStats.None;

    /// <summary>Points added to the rules' base accuracy (decision 297).</summary>
    public int Accuracy { get; init; }
}
