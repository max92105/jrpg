using Jrpg.Core.Party;

namespace Jrpg.Game.Menus;

/// <summary>
/// Where the Party page's cursor stands over the party's two columns: the active members, then the reserve. It starts on
/// the leader, the first active card.
/// </summary>
/// <param name="party">Whose columns; read on every move, so the cursor follows the party's changes.</param>
internal sealed class PartyCursor(PartyState party)
{
    public bool IsOnReserve { get; private set; }

    /// <summary>The selected card's index in its column.</summary>
    public int Row { get; private set; }

    public PartyMember Selected => Column[Row];

    private IReadOnlyList<PartyMember> Column => ColumnOf(IsOnReserve);

    /// <summary>Up or down in the column, wrapping at its ends.</summary>
    public void MoveInColumn(int step)
    {
        var count = Column.Count;
        Row = (Row + step + count) % count;
    }

    /// <summary>
    /// To the other column at the same row, or its last card when it's shorter (developer, 2026-10-20). Nothing happens
    /// when the other column is empty.
    /// </summary>
    public void SwitchColumn()
    {
        var other = ColumnOf(!IsOnReserve);
        if (other.Count == 0)
        {
            return;
        }

        IsOnReserve = !IsOnReserve;
        Row = Math.Min(Row, other.Count - 1);
    }

    /// <summary>
    /// After a member moved out of the cursor's column: the next card slides into the cursor's place, or the cursor steps
    /// back to the last one. The reserve can empty out (four recruits or fewer); the active party never does.
    /// </summary>
    public void KeepOnACard()
    {
        if (Column.Count == 0)
        {
            IsOnReserve = false;
        }

        Row = Math.Min(Row, Column.Count - 1);
    }

    /// <summary>
    /// While swapping places in the active column: up or down to the next member Core lets change place, so never onto
    /// the leader's. The moving member can change place, so there is always one to land on.
    /// </summary>
    public void MoveToNextPlaceToSwap(int step)
    {
        var active = party.ActiveMembers;
        do
        {
            Row = (Row + step + active.Count) % active.Count;
        }
        while (!party.CanChangePlace(active[Row]));
    }

    /// <summary>Back onto an active card, such as where a cancelled swap started.</summary>
    public void ReturnTo(int activeRow)
    {
        IsOnReserve = false;
        Row = activeRow;
    }

    /// <summary>Back to the leader (decision 109).</summary>
    public void Reset() => ReturnTo(0);

    private IReadOnlyList<PartyMember> ColumnOf(bool reserve) => reserve ? party.ReserveMembers : party.ActiveMembers;
}
