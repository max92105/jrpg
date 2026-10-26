using Jrpg.Core.Effects;
using Jrpg.Core.Party;
using Jrpg.Game.Input;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Menus;

/// <summary>
/// Choosing who an item is used on, among the party sidebar's cards (decision 267): every member is listed, only the
/// legal ones can be chosen, and an item that takes everyone selects them all and needs one Confirm.
/// </summary>
internal sealed class ItemTargetPicker
{
    private readonly TargetChoice<PartyMember> _choice;
    private readonly ListCursor _cursor;

    /// <param name="members">Everyone shown, in the sidebar's order: the active party, then the reserve.</param>
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

    /// <summary>The member the pointer is on; null when the item takes everyone, which needs no pointer.</summary>
    public PartyMember? Pointed => _choice.TakesAll ? null : Members[_cursor.Selected];

    /// <summary>Points at <paramref name="member"/> if they can take the item; otherwise the pointer stays.</summary>
    public void PointAt(PartyMember member)
    {
        if (!IsCandidate(member))
        {
            return;
        }

        for (var index = 0; index < Members.Count; index++)
        {
            if (Members[index] == member)
            {
                _cursor.Select(index);
                return;
            }
        }
    }

    /// <returns>The targets confirmed this update, or null.</returns>
    public IReadOnlyList<PartyMember>? Update(InputFrame input)
    {
        if (_choice.TakesAll)
        {
            return input.IsPressed(GameButton.Confirm) ? _choice.Candidates : null;
        }

        // The cards are one column, so only Up and Down move; Left and Right do nothing.
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
