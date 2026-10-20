using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Stats;

namespace Jrpg.Game.Menus;

/// <summary>A member's attack and defense now, and with a slot changed: another item in it, or nothing (Core's rule).</summary>
/// <param name="Current">With what they wear now.</param>
/// <param name="WithItem">With the item on instead.</param>
internal sealed record StatPreview(CombatStats Current, CombatStats WithItem)
{
    public int AttackChange => WithItem.Attack - Current.Attack;

    public int DefenseChange => WithItem.Defense - Current.Defense;

    /// <param name="member">Who would wear it.</param>
    /// <param name="item">Equipment for one of their slots.</param>
    public static StatPreview Of(PartyMember member, ItemDefinition item) => new(member.TotalStats, member.StatsWith(item));

    /// <summary>The change if what's worn in <paramref name="slot"/> were taken off.</summary>
    public static StatPreview OfRemoving(PartyMember member, EquipmentSlot slot) => new(member.TotalStats, member.StatsWithout(slot));
}
