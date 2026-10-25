using Jrpg.Core.Resources;
using Jrpg.Core.Skills;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Battle;

/// <summary>
/// What a skill asks of the actor, as its row shows it ("2 Mana", "Faith 2", "Needs 4 Faith"), and why Confirm can't
/// use it ("Not enough Mana."). A cost wins over a requirement; names come from the actor's class (ADR 0019).
/// </summary>
internal sealed class SkillCostText(UiMessages messages, ResourceMeters actorResources)
{
    /// <summary>One line: the first resource cost, else the first requirement, else empty for a free skill.</summary>
    public string CostOf(SkillDefinition skill)
    {
        if (skill.ResourceChanges.FirstOrDefault(change => change.Amount < 0) is { } cost)
        {
            return ResourceLine(MessageIds.BattleSkillResourceCost, cost.ResourceId, -cost.Amount);
        }

        if (skill.Requirements.Count > 0)
        {
            var requirement = skill.Requirements[0];
            var isMet = actorResources.AmountOf(requirement.ResourceId) >= requirement.AtLeast;
            var messageId = isMet ? MessageIds.BattleSkillResourceLevel : MessageIds.BattleSkillResourceNeeded;
            return ResourceLine(messageId, requirement.ResourceId, requirement.AtLeast);
        }

        return string.Empty;
    }

    /// <param name="lacking">The resource the actor is short of.</param>
    public string CannotAffordText(ResourceDefinition lacking) =>
        messages.Format(MessageIds.BattleNotEnoughResource, new MessageArgument("resource", lacking.Name));

    // Every resource line has the placeholders {amount} and {resource}.
    private string ResourceLine(string messageId, string resourceId, int amount)
    {
        var resource = actorResources.Definitions.First(definition => definition.Id == resourceId);
        return messages.Format(messageId, MessageArgument.Number("amount", amount), new MessageArgument("resource", resource.Name));
    }
}
