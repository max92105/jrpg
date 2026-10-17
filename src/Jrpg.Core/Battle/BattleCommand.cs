namespace Jrpg.Core.Battle;

/// <summary>What the current actor does on their turn. Passed to <see cref="BattleState.Execute"/>.</summary>
public abstract record BattleCommand
{
    // Only Core defines commands, so BattleState.Execute handles every one there is.
    private protected BattleCommand()
    {
    }
}
