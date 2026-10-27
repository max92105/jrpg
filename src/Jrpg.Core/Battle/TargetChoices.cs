using Jrpg.Core.Effects;

namespace Jrpg.Core.Battle;

/// <summary>
/// Who an actor may aim an effect at in one battle, and the check that a chosen set of targets is legal: the one rule
/// behind both the choices the battle offers and the check when a command is carried out, so the two can't drift.
/// </summary>
/// <param name="combatants">Both sides, in the battle's order, which the choices keep.</param>
/// <param name="formation">The party's line, along which an aura reaches (ADR 0023).</param>
internal sealed class TargetChoices(IReadOnlyList<Combatant> combatants, Formation formation)
{
    // An aura reaches the actor and the living allies within its range along the formation (ADR 0023, question 210).
    public TargetChoice<Combatant> ChoiceFor(Combatant actor, Effect effect) =>
        effect.Targeting == Targeting.AlliesInRange
            ? new([.. formation.WithinRange(actor, effect.Range).Where(ally => ally.IsAlive)], TakesAll: true)
            : ChoiceFor(actor, effect.Targeting);

    public TargetChoice<Combatant> ChoiceFor(Combatant actor, Targeting targeting) => new(
        [.. combatants.Where(candidate => TargetRules.CanTarget(targeting, RelationOf(actor, candidate), candidate.Vitals))],
        TargetRules.HitsEveryValidTarget(targeting));

    /// <exception cref="ArgumentException"><paramref name="targets"/> isn't a legal choice for the effect.</exception>
    public void ThrowIfIllegal(Combatant actor, Effect effect, IReadOnlyList<Combatant> targets)
    {
        var validTargets = ChoiceFor(actor, effect).Candidates.ToHashSet();
        if (!TargetRules.IsLegalChoice(effect.Targeting, validTargets, targets))
        {
            throw new ArgumentException(
                $"Illegal targets for a {effect.Targeting} effect from {actor.Name}: {string.Join(", ", targets.Select(target => target.Name))}.",
                nameof(targets));
        }
    }

    private static TargetRelation RelationOf(Combatant actor, Combatant candidate) =>
        candidate == actor ? TargetRelation.Self
        : candidate.Side == actor.Side ? TargetRelation.Ally
        : TargetRelation.Enemy;
}
