namespace Jrpg.Core.Party;

/// <summary>Max HP, attack and defense: what a class gives at level 1, or what each level after adds.</summary>
public sealed record ClassStats(int MaxHp, int Attack, int Defense);
