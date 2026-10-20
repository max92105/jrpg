namespace Jrpg.Game.Battle;

/// <summary>Where the battle screen is in a turn (ADR 0013).</summary>
internal enum TurnStage
{
    /// <summary>The actor steps out of the formation.</summary>
    StepForward,

    /// <summary>A party member waits for the player's command.</summary>
    AwaitingCommand,

    /// <summary>The action has landed; the screen holds while it shows.</summary>
    Holding,

    /// <summary>The actor steps back into the formation.</summary>
    StepBack,

    /// <summary>The battle has ended and its last actor is back in place.</summary>
    Over,
}
