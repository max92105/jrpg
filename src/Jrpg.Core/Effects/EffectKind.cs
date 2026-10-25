namespace Jrpg.Core.Effects;

public enum EffectKind
{
    Damage,
    RestoreHp,
    Revive,

    /// <summary>Changes no HP: the effect only grants its status (ADR 0021).</summary>
    GrantStatus,

    /// <summary>Changes no HP: the user takes an extra turn after the target's next turn (decision 202).</summary>
    ReadyExtraTurn,
}
