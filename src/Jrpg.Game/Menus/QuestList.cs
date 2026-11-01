using Jrpg.Core.Quests;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Menus;

/// <summary>
/// The Quests page's two lists (decision 390), the active and the completed quests in the journal's order as rows with
/// their fitted titles, each list with its own scrolling cursor and gliding highlight (decisions 252, 262), and which
/// of the two is shown. Shown again when the page opens, so drawing formats nothing.
/// </summary>
/// <param name="fitTitle">Fits a title to the list's column, once per row when the list is shown.</param>
internal sealed class QuestList(Func<string, string> fitTitle)
{
    private readonly Column[] _columns = [new Column(), new Column()];

    public QuestCategory Category { get; private set; }

    /// <summary>The shown category's quests with their fitted titles.</summary>
    public IReadOnlyList<QuestRow> Rows => Current.Rows;

    /// <summary>The shown category's cursor; null while it lists no quest.</summary>
    public ListCursor? Cursor => Current.Cursor;

    /// <summary>The shown category's highlight, gliding to its cursor's row.</summary>
    public SelectionMotion Selection => Current.Selection;

    /// <summary>The chosen quest of the shown category; null while it lists no quest.</summary>
    public QuestEntry? Selected => Current.Cursor is { } cursor ? Current.Rows[cursor.Selected].Entry : null;

    private Column Current => _columns[(int)Category];

    public void Advance(TimeSpan delta) => Current.Selection.Advance(delta);

    /// <summary>Lists both categories, the Active one shown, each cursor on its first quest.</summary>
    public void Show(IReadOnlyList<QuestEntry> active, IReadOnlyList<QuestEntry> completed)
    {
        Category = QuestCategory.Active;
        _columns[(int)QuestCategory.Active].Show(active, fitTitle);
        _columns[(int)QuestCategory.Completed].Show(completed, fitTitle);
    }

    /// <summary>Right: the Completed list; nothing past it (no wrap).</summary>
    public void ShowNext() => Category = QuestCategory.Completed;

    /// <summary>Left: back to the Active list, whose cursor stayed where it was.</summary>
    public void ShowPrevious() => Category = QuestCategory.Active;

    public void MoveUp() => Current.Move(cursor => cursor.MoveUp());

    public void MoveDown() => Current.Move(cursor => cursor.MoveDown());

    // One category's rows, cursor and highlight.
    private sealed class Column
    {
        public QuestRow[] Rows { get; private set; } = [];

        public ListCursor? Cursor { get; private set; }

        public SelectionMotion Selection { get; } = new();

        public void Show(IReadOnlyList<QuestEntry> entries, Func<string, string> fitTitle)
        {
            Rows = [.. entries.Select(entry => new QuestRow(entry, fitTitle(entry.Quest.Title)))];
            if (Rows.Length == 0)
            {
                Cursor = null;
                return;
            }

            Cursor = new ListCursor(Rows.Length, _ => true, QuestListView.VisibleRows);
            Selection.JumpTo(Cursor);
        }

        public void Move(Action<ListCursor> step)
        {
            if (Cursor is not { } cursor)
            {
                return;
            }

            step(cursor);
            Selection.Follow(cursor);
        }
    }
}
