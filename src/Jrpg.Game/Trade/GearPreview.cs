using Jrpg.Core.Party;
using Jrpg.Game.Input;
using Jrpg.Game.Menus;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Trade;

/// <summary>
/// What wearing the Shop's highlighted piece would change for one active member, whom Left and Right switch through the
/// party, the leader first (decision 383). Rebuilt only when the row or the member changes, never per frame.
/// </summary>
/// <param name="party">Whose active members are previewed.</param>
/// <param name="messages">The stat names the changes are written with.</param>
internal sealed class GearPreview(PartyState party, UiMessages messages)
{
    private readonly EquipmentComparison _comparison = new(messages);
    private int _memberIndex;
    private TradeRow? _previewedRow;
    private PartyMember? _previewedMember;

    /// <summary>The active member previewed.</summary>
    public PartyMember Member => party.ActiveMembers[_memberIndex];

    /// <summary>
    /// What wearing the followed row's piece would change for <see cref="Member"/>; null on other rows.
    /// </summary>
    public StatComparisonText? Changes => _comparison.Text;

    /// <summary>Left and Right switch the member, wrapping through the active party.</summary>
    public void SwitchMember(InputFrame input)
    {
        var members = party.ActiveMembers.Count;
        if (input.IsPressed(GameButton.Right))
        {
            _memberIndex = (_memberIndex + 1) % members;
        }

        if (input.IsPressed(GameButton.Left))
        {
            _memberIndex = (_memberIndex + members - 1) % members;
        }
    }

    /// <summary>Shows the changes for the highlighted row; a stack or a material has none.</summary>
    public void Follow(TradeRow? row)
    {
        var member = Member;
        if (ReferenceEquals(row, _previewedRow) && ReferenceEquals(member, _previewedMember))
        {
            return;
        }

        _previewedRow = row;
        _previewedMember = member;
        if (row?.Piece is { } piece)
        {
            _comparison.ShowPiece(member, piece);
        }
        else
        {
            _comparison.Clear();
        }
    }
}
