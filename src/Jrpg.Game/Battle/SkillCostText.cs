using Jrpg.Core.Resources;
using Jrpg.Core.Skills;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Battle;

/// <summary>
/// What a skill costs the actor, as its row shows it ("8 MP", "2 Mana"), and why Confirm can't use it ("Not enough
/// Mana."). A class resource cost wins over MP, which MP1 retires; names come from the actor's class (ADR 0019).
/// </summary>
internal sealed class SkillCostText(UiMessages messages, ResourceMeters actorResources)
{
    /// <summary>One cost per skill: its first resource cost, or else its MP. No skill pays two kinds yet.</summary>
    public string CostOf(SkillDefinition skill)
    {
        var cost = skill.ResourceChanges.FirstOrDefault(change => change.Amount < 0);
        if (cost is null)
        {
            return messages.Format(MessageIds.BattleSkillCost, MessageArgument.Number("cost", skill.MpCost));
        }

        var resource = actorResources.Definitions.First(definition => definition.Id == cost.ResourceId);
        return messages.Format(
            MessageIds.BattleSkillResourceCost,
            MessageArgument.Number("cost", -cost.Amount),
            new MessageArgument("resource", resource.Name));
    }

    /// <param name="lacking">The resource the actor is short of; null when only MP is short.</param>
    public string CannotAffordText(ResourceDefinition? lacking) => lacking is null
        ? messages.Text(MessageIds.BattleNotEnoughMp)
        : messages.Format(MessageIds.BattleNotEnoughResource, new MessageArgument("resource", lacking.Name));
}
