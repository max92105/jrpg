namespace Jrpg.Core.Party;

/// <summary>
/// Max HP, attack, defense and speed: what a class gives at level 1, or what each level after adds. Speed only sets the
/// rogue's readiness charges for now (question 213).
/// </summary>
public sealed record ClassStats(int MaxHp, int Attack, int Defense, int Speed);
