using Jrpg.Core.Effects;

namespace Jrpg.Core.Battle;

/// <summary>
/// Writes one <see cref="BattleState.Execute"/> call's events into the enemies' tactic memories: the one place the
/// memory is written, after every event of the call, as phases are entered (ADR 0034, decision 846), so every path that
/// hits is seen. An enemy that acts forgets its reactions first, as its turn answered them; a stunned one never acts,
/// so it keeps them (decision 844). An enemy that uses a skill notes it for its once-per-battle rules (decision 842).
/// Then each damage hit a foe's action landed on an enemy notes it was hit, critically hit and hit on a weakness as it
/// was, by the actor (decision 845). A miss lands nothing and a status tick is no <see cref="EffectApplied"/>, so
/// neither counts; a blocked hit still lands, so it does.
/// </summary>
internal static class TacticMemoryNotes
{
    /// <param name="events">The call's events, its <see cref="ActionStarted"/> first.</param>
    public static void Note(IReadOnlyList<BattleEvent> events)
    {
        var started = (ActionStarted)events[0];
        var actor = started.Actor;
        if (actor.Side == BattleSide.Enemies)
        {
            NoteOwnTurn(actor.TacticMemory, started.Command);
        }

        var hitsByAFoe = events.OfType<EffectApplied>().Where(applied =>
            applied is { Kind: EffectKind.Damage, Target.Side: BattleSide.Enemies } && applied.Target.Side != actor.Side);
        foreach (var hit in hitsByAFoe)
        {
            NoteHit(hit, actor);
        }
    }

    private static void NoteOwnTurn(TacticMemory memory, BattleCommand command)
    {
        memory.ForgetReactions();
        if (command is SkillCommand used)
        {
            memory.NoteUsed(used.Skill.Id);
        }
    }

    private static void NoteHit(EffectApplied hit, Combatant actor)
    {
        var memory = hit.Target.TacticMemory;
        memory.NoteReaction(TacticConditionKind.WasHit, actor);
        if (hit.IsCritical)
        {
            memory.NoteReaction(TacticConditionKind.WasCriticallyHit, actor);
        }

        if (hit.Affinity == HitAffinity.Weak)
        {
            memory.NoteReaction(TacticConditionKind.WasHitOnWeakness, actor);
        }
    }
}
