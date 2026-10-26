using Jrpg.Core.Party;

namespace Jrpg.Game.Menus;

/// <summary>A menu page that shows one party member at a time, which the party sidebar marks (decision 146).</summary>
internal interface IMemberPage : IMenuPage
{
    /// <summary>The members Left and Right go through, in order: the active party, then the reserve.</summary>
    IReadOnlyList<PartyMember> Members { get; }

    /// <summary>The member the page shows now; Left and Right change it.</summary>
    PartyMember Shown { get; }

    /// <summary>Whether the page points at a row of its own; the sidebar then only lights the shown member.</summary>
    bool HasOwnPointer { get; }
}
