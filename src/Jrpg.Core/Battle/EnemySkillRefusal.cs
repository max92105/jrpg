namespace Jrpg.Core.Battle;

/// <summary>Why an enemy can't use a skill.</summary>
public enum EnemySkillRefusal
{
    /// <summary>It spends, gains or requires a class resource, which enemies don't hold (ADR 0019).</summary>
    UsesClassResource,

    /// <summary>It aims along the party's formation, which enemies don't stand in (ADR 0023).</summary>
    AimsAlongFormation,

    /// <summary>It places a readied turn, a rogue's resource mechanic.</summary>
    PlacesReadiedTurn,
}
