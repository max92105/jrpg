using Jrpg.Core.Resources;
using Jrpg.Core.Skills;

namespace Jrpg.Core.Battle;

/// <summary>What a skill costs its user: MP, and the negative amounts of its resource changes (ADR 0019).</summary>
internal static class SkillPayment
{
    public static bool CanPay(Combatant actor, SkillDefinition skill) =>
        actor.Vitals.Mp >= skill.MpCost && LackingResourceOf(actor, skill) is null;

    /// <returns>The first resource the skill costs more of than the actor has; null when none is short.</returns>
    public static ResourceDefinition? LackingResourceOf(Combatant actor, SkillDefinition skill)
    {
        var shortChange = skill.ResourceChanges.FirstOrDefault(
            change => change.Amount < 0 && actor.Resources.AmountOf(change.ResourceId) < -change.Amount);
        return shortChange is null
            ? null
            : actor.Resources.Definitions.First(definition => definition.Id == shortChange.ResourceId);
    }

    /// <summary>Spends the MP, then applies each resource change in order, reporting each one that moved.</summary>
    public static void Pay(Combatant actor, SkillDefinition skill, List<BattleEvent> events)
    {
        actor.Vitals.TrySpendMp(skill.MpCost);
        foreach (var change in skill.ResourceChanges)
        {
            var applied = actor.Resources.Change(change.ResourceId, change.Amount);
            if (applied != 0)
            {
                events.Add(new ResourceChanged(actor, change.ResourceId, applied));
            }
        }
    }
}
