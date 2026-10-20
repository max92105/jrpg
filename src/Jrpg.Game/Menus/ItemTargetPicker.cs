using Jrpg.Core.Effects;
using Jrpg.Core.Party;
using Jrpg.Game.Input;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Menus;

/// <summary>
/// Choosing who an item is used on (legacy use popup): every member is listed, only the legal ones can be chosen, and an
/// item that takes everyone selects them all and needs one Confirm.
/// </summary>
internal sealed class ItemTargetPicker
{
    private readonly TargetChoice<PartyMember> _choice;
    private readonly ListCursor _cursor;

    /// <param name="members">Everyone shown, in party order.</param>
    /// <param name="choice">Who the item can be used on; at least one.</param>
    public ItemTargetPicker(IReadOnlyList<PartyMember> members, TargetChoice<PartyMember> choice)
    {
        Members = members;
        _choice = choice;
        _cursor = new ListCursor(members.Count, index => choice.Candidates.Contains(members[index]));
    }

    public IReadOnlyList<PartyMember> Members { get; }

    public bool IsCandidate(PartyMember member) => _choice.Candidates.Contains(member);

    public bool IsSelected(PartyMember member) => _choice.TakesAll ? IsCandidate(member) : member == Members[_cursor.Selected];

    /// <returns>The targets confirmed this update, or null.</returns>
    public IReadOnlyList<PartyMember>? Update(InputFrame input)
    {
        if (_choice.TakesAll)
        {
            return input.IsPressed(GameButton.Confirm) ? _choice.Candidates : null;
        }

        if (input.IsPressed(GameButton.Up))
        {
            _cursor.MoveUp();
        }

        if (input.IsPressed(GameButton.Down))
        {
            _cursor.MoveDown();
        }

        return input.IsPressed(GameButton.Confirm) ? [Members[_cursor.Selected]] : null;
    }
}
