namespace Jrpg.Core.Effects;

/// <summary>What a skill or consumable does, how strongly, and to whom.</summary>
public sealed record Effect(EffectKind Kind, int Power, Targeting Targeting);
