namespace Jrpg.Game.Battle;

/// <summary>Where the battle screen is in a turn (ADR 0013).</summary>
internal enum TurnStage
{
    /// <summary>A party actor steps out of the formation; an enemy pauses in place (decision 643).</summary>
    StepForward,

    /// <summary>A party member waits for the player's command.</summary>
    AwaitingCommand,

    /// <summary>The action is under way; Core executes it when this ends, at the impact (decision 644).</summary>
    WindingUp,

    /// <summary>The action has landed; the screen holds while it shows.</summary>
    Holding,

    /// <summary>A party actor steps back into the formation; an enemy pauses in place.</summary>
    StepBack,

    /// <summary>The battle has ended and its last actor is back in place.</summary>
    Over,
}
