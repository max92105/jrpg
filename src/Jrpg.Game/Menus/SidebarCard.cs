using Jrpg.Core.Party;

namespace Jrpg.Game.Menus;

/// <summary>
/// One member's card in the menu's party sidebar, as what it shows that can change, and whether the member is in the
/// reserve rather than the active party. A new record means the card needs formatting anew; a member's max HP and MP
/// are fixed (<c>Vitals</c> is built once from the definition), so they aren't watched.
/// </summary>
internal sealed record SidebarCard(PartyMember Member, int Level, int Hp, int Mp, bool IsReserve)
{
    public static SidebarCard Of(PartyMember member, bool isReserve) => new(member, member.Level, member.Vitals.Hp, member.Vitals.Mp, isReserve);
}
