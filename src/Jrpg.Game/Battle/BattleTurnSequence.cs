using Jrpg.Core.Battle;

namespace Jrpg.Game.Battle;

/// <summary>
/// Whose turn the battle screen shows and what happens next (ADR 0013). Enemy turns are resolved at once until playback
/// paces them (plan 2026-10-20-battle, B5a); a party turn waits for the player's command.
/// </summary>
internal sealed class BattleTurnSequence(BattleState battle, EnemyTactics tactics)
{
    public bool IsOver => battle.IsOver;

    public bool IsAwaitingCommand => battle.Phase == BattlePhase.PartyTurn;

    /// <summary>The combatant acting on screen.</summary>
    public Combatant Actor => battle.CurrentActor;

    /// <summary>Plays the enemy turns due now, each with the command Core's tactics choose.</summary>
    public void Update()
    {
        while (battle.Phase == BattlePhase.EnemyTurn)
        {
            battle.Execute(tactics.ChooseCommand(battle));
        }
    }

    /// <summary>The acting party member carries out <paramref name="command"/>.</summary>
    /// <exception cref="InvalidOperationException">No party member is waiting for a command.</exception>
    public void Submit(BattleCommand command)
    {
        if (!IsAwaitingCommand)
        {
            throw new InvalidOperationException($"No party member is waiting for a command ({battle.Phase}).");
        }

        battle.Execute(command);
    }
}
