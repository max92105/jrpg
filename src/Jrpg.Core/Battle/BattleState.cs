using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Progression;
using Jrpg.Core.Randomness;
using Jrpg.Core.Resources;
using Jrpg.Core.Skills;
using Jrpg.Core.Statuses;

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

    /// <param name="combatants">
    /// Both sides, with at least one living enemy. With nobody standing in the party, the battle is lost from the start
    /// (decision 97).
    /// </param>
    /// <param name="inventory">The party's items; using one in battle consumes a copy.</param>
    /// <param name="random">Decides the turn order.</param>
    /// <exception cref="ArgumentException">No enemy is alive, or a combatant is listed twice.</exception>
    public BattleState(IReadOnlyList<Combatant> combatants, Inventory inventory, IRandom random)
    {
        Combatants = [.. combatants];
        if (Combatants.Distinct().Count() != Combatants.Count)
        {
            throw new ArgumentException("A combatant can only be in a battle once.", nameof(combatants));
        }

        if (LivingOn(BattleSide.Enemies).Count == 0)
        {
            throw new ArgumentException("A battle needs at least one living enemy.", nameof(combatants));
        }

        Formation = new Formation([.. Combatants.Where(combatant => combatant.Side == BattleSide.Party)]);
        _inventory = inventory;
        _turnOrder = new TurnOrder(Combatants, random);
        Phase = LivingOn(BattleSide.Party).Count == 0 ? BattlePhase.Defeat : TurnPhaseOf(CurrentActor);
    }

    public BattlePhase Phase { get; private set; }

    /// <summary>The party's line in this battle, starting from the active order (ADR 0023).</summary>
    public Formation Formation { get; }

    /// <summary>A battle between the active party, in party order, and <paramref name="enemies"/>, in encounter order.</summary>
    /// <exception cref="ArgumentException">There are no enemies.</exception>
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

    /// <summary>Whom the actor can trade places with: the allies right beside them; none on an enemy's turn.</summary>
    public TargetChoice<Combatant> TargetChoiceForMove() =>
        new(CurrentActor.Side == BattleSide.Party ? Formation.Neighbors(CurrentActor) : [], TakesAll: false);

    /// <summary>Who the current actor can aim <paramref name="skill"/> at, in formation order.</summary>
    public TargetChoice<Combatant> TargetChoiceFor(SkillDefinition skill) => ChoiceFor(skill.Effect);

    /// <summary>Whether the actor can pay <paramref name="skill"/>: its resource requirements and costs.</summary>
    public bool CanAfford(SkillDefinition skill) => SkillPayment.CanPay(CurrentActor, skill);

    /// <returns>
    /// The first class resource the current actor has too little of for <paramref name="skill"/>; null exactly when
    /// <see cref="CanAfford"/> is true.
    /// </returns>
    public ResourceDefinition? LackingResourceFor(SkillDefinition skill) =>
        SkillPayment.LackingResourceOf(CurrentActor, skill);

    /// <summary>
    /// Whether the current actor knows <paramref name="skill"/>, can pay for it and has someone to aim it at. The UI
    /// greys out the rest.
    /// </summary>
    public bool CanUse(SkillDefinition skill) =>
        CurrentActor.Skills.Contains(skill) && CanAfford(skill) && TargetChoiceFor(skill).Candidates.Count > 0;

    /// <summary>Who the current actor can aim <paramref name="item"/> at, in formation order.</summary>
    /// <exception cref="ArgumentException">The item has no effect to aim.</exception>
    public TargetChoice<Combatant> TargetChoiceFor(ItemDefinition item) =>
        item.Effect is { } effect
            ? ChoiceFor(effect)
            : throw new ArgumentException($"{item.Name} has no effect to aim.", nameof(item));

    /// <summary>
    /// Whether the current actor can use <paramref name="item"/>: a party member, an owned copy, an effect to apply and
    /// someone to aim it at.
    /// </summary>
    public bool CanUse(ItemDefinition item) =>
        CurrentActor.Side == BattleSide.Party && item.Effect is { } effect && _inventory.QuantityOf(item.Id) > 0
        && ChoiceFor(effect).Candidates.Count > 0;

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
            case MoveCommand move:
                Move(actor, move.SwapWith, events);
                break;
            default:
                throw new ArgumentException($"Unknown command {command.GetType().Name}.", nameof(command));
        }

        EndTurn(actor, events);
        return events;
    }

    private static BattlePhase TurnPhaseOf(Combatant actor) =>
        actor.Side == BattleSide.Party ? BattlePhase.PartyTurn : BattlePhase.EnemyTurn;

    private static TargetRelation RelationOf(Combatant actor, Combatant candidate) =>
        candidate == actor ? TargetRelation.Self
        : candidate.Side == actor.Side ? TargetRelation.Ally
        : TargetRelation.Enemy;

    // An attack is a single-enemy damage effect whose power is the attacker's attack.
    private void Attack(Combatant actor, Combatant target, List<BattleEvent> events)
    {
        var effect = new Effect(EffectKind.Damage, actor.CurrentStats.Attack, AttackTargeting);
        ThrowIfIllegalTargets(effect, [target]);
        EffectLanding.Land(effect, [target], Combatants, events);
    }

    private void UseSkill(Combatant actor, SkillCommand command, List<BattleEvent> events)
    {
        if (!CanUse(command.Skill))
        {
            throw new InvalidOperationException($"{actor.Name} can't use {command.Skill.Name} (unknown skill, can't pay for it, or no target).");
        }

        ThrowIfIllegalTargets(command.Skill.Effect, command.Targets);
        SkillPayment.Pay(actor, command.Skill, events);
        EffectLanding.Land(command.Skill.Effect, command.Targets, Combatants, events);
    }

    private void UseItem(Combatant actor, ItemCommand command, List<BattleEvent> events)
    {
        if (!CanUse(command.Item) || command.Item.Effect is not { } effect)
        {
            throw new InvalidOperationException($"{actor.Name} can't use {command.Item.Name} (not owned, not usable, or not a party member).");
        }

        ThrowIfIllegalTargets(effect, command.Targets);
        _inventory.TryRemove(command.Item.Id, 1);
        EffectLanding.Land(effect, command.Targets, Combatants, events);
    }

    // The one rule for who the current actor may aim at, behind both the choices offered and the check in Execute.
    // An aura reaches the actor and the living allies within its range along the formation (ADR 0023, question 210).
    private TargetChoice<Combatant> ChoiceFor(Effect effect) =>
        effect.Targeting == Targeting.AlliesInRange
            ? new([.. Formation.WithinRange(CurrentActor, effect.Range).Where(ally => ally.IsAlive)], TakesAll: true)
            : ChoiceFor(effect.Targeting);

    private TargetChoice<Combatant> ChoiceFor(Targeting targeting) => new(
        [.. Combatants.Where(candidate => TargetRules.CanTarget(targeting, RelationOf(CurrentActor, candidate), candidate.Vitals))],
        TargetRules.HitsEveryValidTarget(targeting));

    private void ThrowIfIllegalTargets(Effect effect, IReadOnlyList<Combatant> targets)
    {
        var validTargets = ChoiceFor(effect).Candidates.ToHashSet();
        if (!TargetRules.IsLegalChoice(effect.Targeting, validTargets, targets))
        {
            throw new ArgumentException(
                $"Illegal targets for a {effect.Targeting} effect from {CurrentActor.Name}: {string.Join(", ", targets.Select(target => target.Name))}.",
                nameof(targets));
        }
    }

    // Placeholder (question 211): a Move costs the mover's turn, and the ally swapped with loses nothing.
    private void Move(Combatant actor, Combatant swapWith, List<BattleEvent> events)
    {
        if (actor.Side != BattleSide.Party)
        {
            throw new InvalidOperationException("Only the party has a formation to move in.");
        }

        Formation.Swap(actor, swapWith);
        events.Add(new PlacesSwapped(actor, swapWith));
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

    private void EndTurn(Combatant actor, List<BattleEvent> events)
    {
        if (IsOver)
        {
            return;
        }

        RunUpkeep(actor, events);
        SettleOutcome(events);
        if (!IsOver)
        {
            AdvanceToNextActor(events);
        }
    }

    private void RunUpkeep(Combatant holder, List<BattleEvent> events)
    {
        TurnUpkeep.ApplyPerTurnGains(holder, events);
        TurnUpkeep.CountDownStatuses(holder, Combatants, events);
    }

    // Defeat is checked first, as in the demo: if both sides fall in one turn, the party loses.
    private void SettleOutcome(List<BattleEvent> events)
    {
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
            return;
        }

        events.Add(new BattleEnded(Phase));
    }

    // A stunned combatant's turn is skipped inside the same Execute, so the screen never sees it as the actor. The
    // lost turn still runs its upkeep, so the stun counts down and the loop always ends (ADR 0021).
    private void AdvanceToNextActor(List<BattleEvent> events)
    {
        _turnOrder.Advance();
        while (CurrentActor.Statuses.Has(StatusKind.Stun))
        {
            var skipped = CurrentActor;
            events.Add(new TurnSkipped(skipped, StatusKind.Stun));
            RunUpkeep(skipped, events);
            SettleOutcome(events);
            if (IsOver)
            {
                return;
            }

            _turnOrder.Advance();
        }

        Phase = TurnPhaseOf(CurrentActor);
    }
}
