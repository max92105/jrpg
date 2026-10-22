using Jrpg.Core.Party;

namespace Jrpg.Game.Menus;

/// <summary>One member's card in the menu's party sidebar, its texts formatted once.</summary>
internal sealed record SidebarCard
{
    public required PartyMember Member { get; init; }

    public required string Name { get; init; }

    /// <summary>Such as "Lv 3".</summary>
    public required string Level { get; init; }

    /// <summary>Such as "HP 45 / 60".</summary>
    public required string Hp { get; init; }

    /// <summary>Such as "MP 12 / 20".</summary>
    public required string Mp { get; init; }
}
