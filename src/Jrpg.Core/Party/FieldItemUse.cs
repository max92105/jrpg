using Jrpg.Core.Effects;
using Jrpg.Core.Items;

namespace Jrpg.Core.Party;

/// <summary>
/// Uses a consumable from the menu, with the same target rules and effects as battle. As in the demo, every recruited
/// member can be a target, reserve included, and "all" items hit them all; "self" items only hit the user (decision 19).
/// </summary>
public sealed class FieldItemUse(PartyState party, Inventory inventory)
{
    public bool CanUse(ItemDefinition item) => item.IsUsableFromMenu && item.Effect is not null && inventory.QuantityOf(item.Id) > 0;

    /// <param name="item">A consumable usable from the menu.</param>
    /// <param name="user">Whoever uses it; only matters for "self" items, and may be fallen.</param>
    /// <param name="targets">One target, or every valid target for an "all" item.</param>
    /// <returns>The amount actually applied to each target, in target order.</returns>
    /// <exception cref="InvalidOperationException">The item can't be used from the menu or isn't owned.</exception>
    /// <exception cref="ArgumentException">The user isn't recruited, or the targets aren't legal for this item.</exception>
    public IReadOnlyList<int> Use(ItemDefinition item, PartyMember user, IReadOnlyList<PartyMember> targets)
    {
        if (!CanUse(item) || item.Effect is not { } effect)
        {
            throw new InvalidOperationException($"{item.Name} can't be used from the menu (not usable there, or not owned).");
        }

        var validTargets = party.Members
            .Where(candidate => TargetRules.CanTarget(effect.Targeting, RelationOf(user, candidate), candidate.Vitals))
            .ToHashSet();
        if (!party.Members.Contains(user) || !TargetRules.IsLegalChoice(effect.Targeting, validTargets, targets))
        {
            throw new ArgumentException($"Illegal user or targets for {item.Name}.", nameof(targets));
        }

        inventory.TryRemove(item.Id, 1);
        return [.. targets.Select(target => EffectResolver.Apply(effect, target.Vitals, target.TotalStats.Defense))];
    }

    private static TargetRelation RelationOf(PartyMember user, PartyMember candidate) =>
        candidate == user ? TargetRelation.Self : TargetRelation.Ally;
}
