using Jrpg.Core.Party;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Menus;

/// <summary>
/// The Party page's text as last formatted, rebuilt when the party changes or a notice is shown, so drawing formats
/// nothing.
/// </summary>
/// <param name="Active">The active members, in order.</param>
/// <param name="Reserve">The reserve, in recruitment order.</param>
/// <param name="Cards">Every recruit's card.</param>
/// <param name="Count">"active / max", over the active column.</param>
/// <param name="Notice">Why the last Confirm changed nothing, or null.</param>
internal sealed record PartyColumns(
    IReadOnlyList<PartyMember> Active,
    IReadOnlyList<PartyMember> Reserve,
    IReadOnlyDictionary<PartyMember, MemberCardView.Card> Cards,
    MeasuredText Count,
    string? Notice);
