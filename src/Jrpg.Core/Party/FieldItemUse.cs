using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Core.Stats;

namespace Jrpg.Core.Party;

/// <summary>
/// Uses a consumable from the menu, with the same target rules and effects as battle. As in the demo, every recruited
/// member can be a target, reserve included, and "all" items hit them all; "self" items only hit the user (decision 19).
/// </summary>
public sealed class FieldItemUse(PartyState party, Inventory inventory)
{
    /// <summary>
    /// Usable from the menu, owned, and aimed at someone right now: a revive with nobody fallen can't be used. Any
    /// member may be the user, so a "self" item is usable while anyone is standing. A rest item aims at nobody: it is
    /// usable while anyone has something to rest.
    /// </summary>
    public bool CanUse(ItemDefinition item) =>
        item.IsUsableFromMenu
        && item.Effect is { } effect
        && inventory.QuantityOf(item.Id) > 0
        && (effect.Kind == EffectKind.Rest
            ? party.Members.Any(member => member.HasAnythingToRest)
            : party.Members.Any(member => TargetRules.CanTarget(effect.Targeting, TargetRelation.Self, member.Vitals)));

    /// <summary>Uses a rest item: every recruited member rests, reserves too, as at an inn (decision 230).</summary>
    /// <exception cref="InvalidOperationException">
    /// The item isn't a rest item, or can't be used now (<see cref="CanUse"/>).
    /// </exception>
    public void RestParty(ItemDefinition item)
    {
        if (item.Effect?.Kind != EffectKind.Rest || !CanUse(item))
        {
            throw new InvalidOperationException($"{item.Name} can't rest the party now (not a rest item, not owned, or nobody needs it).");
        }

        inventory.TryRemove(item.Id, 1);
        foreach (var member in party.Members)
        {
            member.Rest();
        }
    }

    /// <summary>Who <paramref name="user"/> can use the item on, by the same rules <see cref="Use"/> checks.</summary>
    /// <param name="item">A consumable.</param>
    /// <param name="user">Whoever uses it; only matters for "self" items.</param>
    /// <exception cref="ArgumentException">The item has no effect.</exception>
    public TargetChoice<PartyMember> TargetChoiceFor(ItemDefinition item, PartyMember user)
    {
        var effect = item.Effect ?? throw new ArgumentException($"{item.Name} has no effect to aim.", nameof(item));
        var candidates = party.Members
            .Where(candidate => TargetRules.CanTarget(effect.Targeting, RelationOf(user, candidate), candidate.Vitals))
            .ToList();
        return new TargetChoice<PartyMember>(candidates, TargetRules.HitsEveryValidTarget(effect.Targeting));
    }

    /// <summary>
    /// Who the item can be used on from the menu, where the chosen member is also the user (question 91, placeholder): a
    /// "self" item can go to anyone standing, on themselves; for any other item the user changes nothing.
    /// </summary>
    /// <exception cref="ArgumentException">The item has no effect.</exception>
    public TargetChoice<PartyMember> TargetChoiceForAnyUser(ItemDefinition item)
    {
        if (item.Effect?.Targeting != Targeting.Self)
        {
            return TargetChoiceFor(item, party.Leader);
        }

        var selfUsers = party.Members.Where(member => TargetChoiceFor(item, member).Candidates.Contains(member)).ToList();
        return new TargetChoice<PartyMember>(selfUsers, TakesAll: false);
    }

    /// <summary>
    /// Uses the item on targets chosen from <see cref="TargetChoiceForAnyUser"/>: a single target is also the user, and the
    /// leader stands in for an item that takes everyone.
    /// </summary>
    /// <returns>The amount actually applied to each target, in target order.</returns>
    /// <exception cref="InvalidOperationException">The item can't be used now (<see cref="CanUse"/>).</exception>
    /// <exception cref="ArgumentException">The targets aren't legal for this item.</exception>
    public IReadOnlyList<int> UseOn(ItemDefinition item, IReadOnlyList<PartyMember> targets) =>
        Use(item, targets.Count == 1 ? targets[0] : party.Leader, targets);

    /// <param name="item">A consumable usable from the menu.</param>
    /// <param name="user">Whoever uses it; only matters for "self" items, and may be fallen.</param>
    /// <param name="targets">One target, or every valid target for an "all" item.</param>
    /// <returns>The amount actually applied to each target, in target order.</returns>
    /// <exception cref="InvalidOperationException">
    /// The item can't be used from the menu, isn't owned, or has nobody it can be used on (<see cref="CanUse"/>).
    /// </exception>
    /// <exception cref="ArgumentException">The user isn't recruited, or the targets aren't legal for this item.</exception>
    public IReadOnlyList<int> Use(ItemDefinition item, PartyMember user, IReadOnlyList<PartyMember> targets)
    {
        if (!CanUse(item) || item.Effect is not { } effect || effect.Kind == EffectKind.Rest)
        {
            throw new InvalidOperationException($"{item.Name} can't be used from the menu on chosen targets (not usable there, not owned, nobody it can be used on, or a rest item).");
        }

        var validTargets = TargetChoiceFor(item, user).Candidates.ToHashSet();
        if (!party.Members.Contains(user) || !TargetRules.IsLegalChoice(effect.Targeting, validTargets, targets))
        {
            throw new ArgumentException($"Illegal user or targets for {item.Name}.", nameof(targets));
        }

        inventory.TryRemove(item.Id, 1);
        return [.. targets.Select(target => Land(effect, target.Vitals))];
    }

    /// <summary>
    /// The HP <paramref name="target"/> would have after the item, without using it, so the menu can preview it. Who
    /// may take the item isn't checked: <see cref="TargetChoiceForAnyUser"/> says that.
    /// </summary>
    /// <exception cref="InvalidOperationException">The item has no effect, or is a rest item.</exception>
    public static int HpAfterUse(ItemDefinition item, PartyMember target)
    {
        if (item.Effect is not { } effect || effect.Kind == EffectKind.Rest)
        {
            throw new InvalidOperationException($"{item.Name} lands on no single target (no effect, or a rest item).");
        }

        var preview = new Vitals(target.Vitals.Hp, target.Vitals.MaxHp);
        Land(effect, preview);
        return preview.Hp;
    }

    // Use and HpAfterUse land the effect the same way, so a preview can't drift from the use.
    private static int Land(Effect effect, Vitals vitals) =>
        EffectResolver.Apply(effect, vitals, DamageFormula.NormalHit, Mitigation.None);

    private static TargetRelation RelationOf(PartyMember user, PartyMember candidate) =>
        candidate == user ? TargetRelation.Self : TargetRelation.Ally;
}
