using Jrpg.Core.Quests;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Menus;

/// <summary>
/// The Quests page's listed quests in the journal's order, with their scrolling cursor and gliding highlight (decisions
/// 252, 262). Shown again when the page opens, so drawing formats nothing.
/// </summary>
internal sealed class QuestList
{
    private QuestEntry[] _entries = [];

    public IReadOnlyList<QuestEntry> Entries => _entries;

    /// <summary>The cursor over the entries; null while no quest is listed.</summary>
    public ListCursor? Cursor { get; private set; }

    /// <summary>The highlight gliding to the cursor's row.</summary>
    public SelectionMotion Selection { get; } = new();

    /// <summary>The chosen quest; null while no quest is listed.</summary>
    public QuestEntry? Selected => Cursor is null ? null : _entries[Cursor.Selected];

    public void Advance(TimeSpan delta) => Selection.Advance(delta);

    /// <summary>Lists <paramref name="entries"/>, the cursor on the first, with the highlight jumped there.</summary>
    public void Show(IReadOnlyList<QuestEntry> entries)
    {
        _entries = [.. entries];
        if (_entries.Length == 0)
        {
            Cursor = null;
            return;
        }

        Cursor = new ListCursor(_entries.Length, _ => true, QuestsPage.VisibleRows);
        Selection.JumpTo(Cursor);
    }

    public void MoveUp() => Move(cursor => cursor.MoveUp());

    public void MoveDown() => Move(cursor => cursor.MoveDown());

    private void Move(Action<ListCursor> step)
    {
        if (Cursor is not { } cursor)
        {
            return;
        }

        step(cursor);
        Selection.Follow(cursor);
    }
}
