using Jrpg.Core.Randomness;
using Jrpg.Core.Skills;

namespace Jrpg.Core.Battle;

/// <summary>
/// Chooses what an enemy does on its turn (ADR 0034); <see cref="EnemyTargeting"/> aims it. It draws once for the pick,
/// only when more than one eligible action shares the weight, then what the targeting draws. An enemy without tactics,
/// or with nothing eligible, attacks without a pick (decisions 403, 412), so it keeps its seeded results.
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
        return eligible.Count == 0
            ? Attack(battle)
            : CommandFor(eligible[WeightedPick.Pick([.. eligible.Select(action => action.Weight)], random)], battle);
    }

    private List<EnemyAction> EligibleActions(BattleState battle)
    {
        var actor = battle.CurrentActor;
        var tactics = actor.EnemyPhase?.Tactics ?? actor.Enemy?.Tactics ?? [];
        return [.. tactics.Where(action => IsEligible(action, battle))];
    }

    private bool IsEligible(EnemyAction action, BattleState battle) =>
        (action.When is not { } condition || TacticConditions.Holds(condition, battle.CurrentActor, battle))
        && (action.SkillId is not { } skillId || battle.CanUse(skills[skillId]));

    private BattleCommand CommandFor(EnemyAction action, BattleState battle)
    {
        if (action.SkillId is not { } skillId)
        {
            return Attack(battle);
        }

        var skill = skills[skillId];
        return new SkillCommand(skill, _targeting.ForSkill(battle, skill, action.When));
    }

    private AttackCommand Attack(BattleState battle) => new(_targeting.ForAttack(battle));
}
