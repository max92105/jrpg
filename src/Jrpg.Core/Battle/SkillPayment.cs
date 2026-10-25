using Jrpg.Core.Resources;
using Jrpg.Core.Skills;

namespace Jrpg.Core.Battle;

/// <summary>What a skill asks of its user: MP, its resource requirements, and its resource costs (ADR 0019).</summary>
internal static class SkillPayment
{
    public static bool CanPay(Combatant actor, SkillDefinition skill) =>
        actor.Vitals.Mp >= skill.MpCost && LackingResourceOf(actor, skill) is null;

    /// <returns>
    /// The first resource the actor holds too little of: a requirement first, then a cost, both against the amount
    /// before the skill changes it; null when none is short.
    /// </returns>
    public static ResourceDefinition? LackingResourceOf(Combatant actor, SkillDefinition skill)
    {
        var shortId = skill.Requirements
            .Where(requirement => actor.Resources.AmountOf(requirement.ResourceId) < requirement.AtLeast)
            .Select(requirement => requirement.ResourceId)
            .Concat(skill.ResourceChanges
                .Where(change => change.Amount < 0 && actor.Resources.AmountOf(change.ResourceId) < -change.Amount)
                .Select(change => change.ResourceId))
            .FirstOrDefault();
        return shortId is null ? null : actor.Resources.Definitions.First(definition => definition.Id == shortId);
    }

    /// <summary>Spends the MP, then applies each resource change in order, reporting each one that moved.</summary>
    public static void Pay(Combatant actor, SkillDefinition skill, List<BattleEvent> events)
    {
        actor.Vitals.TrySpendMp(skill.MpCost);
        foreach (var change in skill.ResourceChanges)
        {
            actor.ChangeResource(change.ResourceId, change.Amount, events);
        }
    }
}
