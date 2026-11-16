using Jrpg.Core.Randomness;
using Jrpg.Core.Skills;

namespace Jrpg.Core.Battle;

/// <summary>
/// Chooses what an enemy does on its turn (ADR 0034); <see cref="EnemyTargeting"/> aims it. An eligible priority rule
/// is chosen first, the first in the list among several, without a draw (decision 841); otherwise it draws once for the
/// pick, only when more than one eligible action shares the weight. Then what the targeting draws. An enemy without
/// tactics, or with nothing eligible, attacks without a pick (decisions 403, 412), so it keeps its seeded results. A
/// rule with a target aims by it (decision 834) and is eligible only when it finds someone (decision 836); a once rule
/// isn't, after the enemy used its skill this battle (decision 842). Deciding whether a rule is eligible never draws.
/// </summary>
/// <param name="random">The battle's draws.</param>
/// <param name="skills">The content's skills, which an enemy's actions name by id.</param>
public sealed class EnemyTactics(IRandom random, IReadOnlyDictionary<string, SkillDefinition> skills)
{
    private readonly EnemyTargeting _targeting = new(random);

    /// <exception cref="InvalidOperationException">It isn't an enemy's turn.</exception>
    public BattleCommand ChooseCommand(BattleState battle)
    {
        if (battle.Phase != BattlePhase.EnemyTurn)
        {
            throw new InvalidOperationException($"Enemy tactics only choose on an enemy turn (phase is {battle.Phase}).");
        }

        var eligible = EligibleActions(battle);
        return eligible.Count == 0 ? Attack(battle) : CommandFor(Pick(eligible), battle);
    }

    private EnemyAction Pick(List<EnemyAction> eligible) =>
        eligible.FirstOrDefault(action => action.IsPriority)
        ?? eligible[WeightedPick.Pick([.. eligible.Select(action => action.Weight)], random)];

    private List<EnemyAction> EligibleActions(BattleState battle)
    {
        var actor = battle.CurrentActor;
        var tactics = actor.EnemyPhase?.Tactics ?? actor.Enemy?.Tactics ?? [];
        return [.. tactics.Where(action => IsEligible(action, battle))];
    }

    private bool IsEligible(EnemyAction action, BattleState battle) =>
        ConditionHolds(action, battle) && CanUseSkill(action, battle) && HasTarget(action, battle)
        && !IsUsedUp(action, battle.CurrentActor);

    private static bool ConditionHolds(EnemyAction action, BattleState battle) =>
        action.When is not { } condition || TacticConditions.Holds(condition, battle.CurrentActor, battle);

    private static bool IsUsedUp(EnemyAction action, Combatant actor) =>
        action is { IsOncePerBattle: true, SkillId: { } skillId } && actor.TacticMemory.HasUsed(skillId);

    private bool CanUseSkill(EnemyAction action, BattleState battle) =>
        action.SkillId is not { } skillId || battle.CanUse(skills[skillId]);

    private bool HasTarget(EnemyAction action, BattleState battle) =>
        action.On is null
        || TacticTargetCandidates.For(action, SkillOf(action), LegalTargetsOf(action, battle), battle).Count > 0;

    private BattleCommand CommandFor(EnemyAction action, BattleState battle)
    {
        if (action.On is { } target)
        {
            return CommandAimedBy(action, target, battle);
        }

        if (action.SkillId is not { } skillId)
        {
            return Attack(battle);
        }

        var skill = skills[skillId];
        return new SkillCommand(skill, _targeting.ForSkill(battle, skill, action.When));
    }

    private BattleCommand CommandAimedBy(EnemyAction action, TacticTarget target, BattleState battle)
    {
        var skill = SkillOf(action);
        var legal = LegalTargetsOf(action, battle);
        var candidates = TacticTargetCandidates.For(action, skill, legal, battle);
        var chosen = _targeting.ForRule(battle.CurrentActor, target, candidates, legal);
        return skill is null ? new AttackCommand(chosen) : new SkillCommand(skill, [chosen]);
    }

    private SkillDefinition? SkillOf(EnemyAction action) => action.SkillId is { } skillId ? skills[skillId] : null;

    private IReadOnlyList<Combatant> LegalTargetsOf(EnemyAction action, BattleState battle) =>
        action.SkillId is { } skillId
            ? battle.TargetChoiceFor(skills[skillId]).Candidates
            : battle.TargetChoiceForAttack().Candidates;

    private AttackCommand Attack(BattleState battle) => new(_targeting.ForAttack(battle));
}
