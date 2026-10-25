namespace Jrpg.Game.Battle;

/// <summary>What a beat of a turn's playback shows (design key decision 4).</summary>
internal enum PlaybackBeatKind
{
    /// <summary>The action's name, or what the actor does, on the banner.</summary>
    Banner,

    /// <summary>The effects landing together on their targets.</summary>
    Impact,

    /// <summary>Someone falls.</summary>
    Defeat,

    /// <summary>A combatant loses its turn to a stun, after the action's other beats.</summary>
    Skip,
}
