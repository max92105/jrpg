using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Progression;
using Jrpg.Core.Randomness;
using Jrpg.Core.Skills;

namespace Jrpg.Core.Battle;

/// <summary>
/// A battle in progress. The only way to change it is <see cref="Execute"/>, one call per turn,
/// which returns what happened as battle events (ADR 0004).
/// </summary>
public sealed class BattleState
{
    // Shared by the attack Execute checks and the targets it offers, so the two can't drift.
    private const Targeting AttackTargeting = Targeting.SingleEnemy;

    private readonly TurnOrder _turnOrder;
    private readonly Inventory _inventory;
    private bool _isExperienceGranted;

    /// <param name="combatants">Both sides, each with at least one living combatant.</param>
    /// <param name="inventory">The party's items; using one in battle consumes a copy.</param>
    /// <param name="random">Decides the turn order.</param>
    /// <exception cref="ArgumentException">A side has no living combatant, or a combatant is listed twice.</exception>
    public BattleState(IReadOnlyList<Combatant> combatants, Inventory inventory, IRandom random)
    {
        Combatants = [.. combatants];
        if (Combatants.Distinct().Count() != Combatants.Count)
        {
            throw new ArgumentException("A combatant can only be in a battle once.", nameof(combatants));
        }

        if (LivingOn(BattleSide.Party).Count == 0 || LivingOn(BattleSide.Enemies).Count == 0)
        {
            throw new ArgumentException("Both sides need at least one living combatant.", nameof(combatants));
        }

        _inventory = inventory;
        _turnOrder = new TurnOrder(Combatants, random);
        Phase = TurnPhaseOf(CurrentActor);
    }

    public BattlePhase Phase { get; private set; }

    /// <summary>A battle between the active party, in party order, and <paramref name="enemies"/>, in encounter order.</summary>
    /// <exception cref="ArgumentException">Every active member has fallen, or there are no enemies.</exception>
    public static BattleState Start(PartyState party, IReadOnlyList<EnemyDefinition> enemies, Inventory inventory, IRandom random)
    {
        IReadOnlyList<Combatant> combatants =
        [
            .. party.ActiveMembers.Select(Combatant.FromPartyMember),
            .. enemies.Select(Combatant.FromEnemy),
        ];
        return new BattleState(combatants, inventory, random);
    }

    public bool IsOver => Phase is BattlePhase.Victory or BattlePhase.Defeat or BattlePhase.Escaped;

    public Combatant CurrentActor => _turnOrder.Current;

    public IReadOnlyList<Combatant> Combatants { get; }

    /// <summary>The <paramref name="count"/> turns after the current one, as the order stands now; a fall changes it.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is negative.</exception>
    public IReadOnlyList<Combatant> UpcomingActors(int count) => _turnOrder.Upcoming(count);

    public IReadOnlyList<Combatant> LivingOn(BattleSide side) =>
        [.. Combatants.Where(combatant => combatant.Side == side && combatant.IsAlive)];

    /// <summary>Who the current actor can attack: one living enemy, in formation order.</summary>
    public TargetChoice<Combatant> TargetChoiceForAttack() => ChoiceFor(AttackTargeting);

    /// <summary>Whether the current actor knows <paramref name="skill"/> and has the MP for it. The UI greys out the rest.</summary>
    public bool CanUse(SkillDefinition skill) => CurrentActor.Skills.Contains(skill) && CurrentActor.Vitals.Mp >= skill.MpCost;

    /// <summary>Whether the current actor can use <paramref name="item"/>: a party member, an owned copy, and an effect to apply.</summary>
    public bool CanUse(ItemDefinition item) =>
        CurrentActor.Side == BattleSide.Party && item.Effect is not null && _inventory.QuantityOf(item.Id) > 0;

    /// <summary>Gives the enemies' experience to the party members still standing (developer decision 13). Call once, after a victory.</summary>
    /// <returns>The total experience and one gain per living party member, with the level steps it produced.</returns>
    /// <exception cref="InvalidOperationException">The battle wasn't won, or the experience was already granted.</exception>
    public VictoryRewards GrantVictoryExperience(ExperienceTable table)
    {
        if (Phase != BattlePhase.Victory || _isExperienceGranted)
        {
            throw new InvalidOperationException($"Experience is granted once, after a victory (phase is {Phase}).");
        }

        _isExperienceGranted = true;
        var defeated = Combatants.Select(combatant => combatant.Enemy).OfType<EnemyDefinition>().ToList();
        var members = Combatants.Select(combatant => combatant.Member).OfType<PartyMember>().ToList();
        return new VictoryRewards(BattleRewards.TotalExperience(defeated), BattleRewards.GrantExperience(defeated, members, table));
    }

    /// <summary>The current actor carries out <paramref name="command"/>, then the turn ends.</summary>
    /// <returns>What happened, in order, starting with <see cref="ActionStarted"/>.</returns>
    /// <exception cref="InvalidOperationException">The battle is over, or the current actor can't give this command.</exception>
    /// <exception cref="ArgumentException">The command's targets are not legal targets.</exception>
    public IReadOnlyList<BattleEvent> Execute(BattleCommand command)
    {
        if (IsOver)
        {
            throw new InvalidOperationException($"The battle is over ({Phase}).");
        }

        var actor = CurrentActor;
        var events = new List<BattleEvent> { new ActionStarted(actor, command) };
        switch (command)
        {
            case AttackCommand attack:
                Attack(actor, attack.Target, events);
                break;
            case SkillCommand skill:
                UseSkill(actor, skill, events);
                break;
            case ItemCommand item:
                UseItem(actor, item, events);
                break;
            case DefendCommand:
                break;
            case FleeCommand:
                Flee(actor, events);
                break;
            default:
                throw new ArgumentException($"Unknown command {command.GetType().Name}.", nameof(command));
        }

        EndTurn(events);
        return events;
    }

    private static BattlePhase TurnPhaseOf(Combatant actor) =>
        actor.Side == BattleSide.Party ? BattlePhase.PartyTurn : BattlePhase.EnemyTurn;

    private static TargetRelation RelationOf(Combatant actor, Combatant candidate) =>
        candidate == actor ? TargetRelation.Self
        : candidate.Side == actor.Side ? TargetRelation.Ally
        : TargetRelation.Enemy;

    private static void ApplyToTargets(Effect effect, IReadOnlyList<Combatant> targets, List<BattleEvent> events)
    {
        foreach (var target in targets)
        {
            var wasAlive = target.IsAlive;
            events.Add(new EffectApplied(target, effect.Kind, EffectResolver.Apply(effect, target.Vitals, target.Stats.Defense)));
            if (wasAlive && !target.IsAlive)
            {
                events.Add(new CombatantDefeated(target));
            }
        }
    }

    // An attack is a single-enemy damage effect whose power is the attacker's attack.
    private void Attack(Combatant actor, Combatant target, List<BattleEvent> events)
    {
        var effect = new Effect(EffectKind.Damage, actor.Stats.Attack, AttackTargeting);
        ThrowIfIllegalTargets(effect, [target]);
        ApplyToTargets(effect, [target], events);
    }

    private void UseSkill(Combatant actor, SkillCommand command, List<BattleEvent> events)
    {
        if (!CanUse(command.Skill))
        {
            throw new InvalidOperationException($"{actor.Name} can't use {command.Skill.Name} (unknown skill or not enough MP).");
        }

        ThrowIfIllegalTargets(command.Skill.Effect, command.Targets);
        actor.Vitals.TrySpendMp(command.Skill.MpCost);
        ApplyToTargets(command.Skill.Effect, command.Targets, events);
    }

    private void UseItem(Combatant actor, ItemCommand command, List<BattleEvent> events)
    {
        if (!CanUse(command.Item) || command.Item.Effect is not { } effect)
        {
            throw new InvalidOperationException($"{actor.Name} can't use {command.Item.Name} (not owned, not usable, or not a party member).");
        }

        ThrowIfIllegalTargets(effect, command.Targets);
        _inventory.TryRemove(command.Item.Id, 1);
        ApplyToTargets(effect, command.Targets, events);
    }

    // The one rule for who the current actor may aim at, behind both the choices offered and the check in Execute.
    private TargetChoice<Combatant> ChoiceFor(Targeting targeting) => new(
        [.. Combatants.Where(candidate => TargetRules.CanTarget(targeting, RelationOf(CurrentActor, candidate), candidate.Vitals))],
        TargetRules.HitsEveryValidTarget(targeting));

    private void ThrowIfIllegalTargets(Effect effect, IReadOnlyList<Combatant> targets)
    {
        var validTargets = ChoiceFor(effect.Targeting).Candidates.ToHashSet();
        if (!TargetRules.IsLegalChoice(effect.Targeting, validTargets, targets))
        {
            throw new ArgumentException(
                $"Illegal targets for a {effect.Targeting} effect from {CurrentActor.Name}: {string.Join(", ", targets.Select(target => target.Name))}.",
                nameof(targets));
        }
    }

    private void Flee(Combatant actor, List<BattleEvent> events)
    {
        if (actor.Side != BattleSide.Party)
        {
            throw new InvalidOperationException("Only the party can flee.");
        }

        Phase = BattlePhase.Escaped;
        events.Add(new BattleEnded(Phase));
    }

    private void EndTurn(List<BattleEvent> events)
    {
        if (IsOver)
        {
            return;
        }

        // Defeat is checked first, as in the demo: if both sides fall in one turn, the party loses.
        if (LivingOn(BattleSide.Party).Count == 0)
        {
            Phase = BattlePhase.Defeat;
        }
        else if (LivingOn(BattleSide.Enemies).Count == 0)
        {
            Phase = BattlePhase.Victory;
        }
        else
        {
            _turnOrder.Advance();
            Phase = TurnPhaseOf(CurrentActor);
            return;
        }

        events.Add(new BattleEnded(Phase));
    }
}
