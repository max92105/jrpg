using Jrpg.Game.Input;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Saving;

/// <summary>The save files as a list to choose from, scrolling a window of them with the cursor.</summary>
internal sealed class SaveSlotList
{
    /// <summary>How many 112 px rows fit on a menu page.</summary>
    public const int VisibleRows = 6;

    private readonly ListCursor _cursor;

    /// <param name="entries">One per file, in order; at least one.</param>
    public SaveSlotList(IReadOnlyList<SaveSlotEntry> entries)
    {
        Entries = entries;
        _cursor = new ListCursor(entries.Count, _ => true, VisibleRows);
    }

    /// <summary>A new list after each read of the files, so a view can tell when to rebuild its text.</summary>
    public IReadOnlyList<SaveSlotEntry> Entries { get; private set; }

    public int Selected => _cursor.Selected;

    public int FirstVisible => _cursor.FirstVisible;

    /// <returns>The entry confirmed this update, or null.</returns>
    public SaveSlotEntry? Update(InputFrame input)
    {
        if (input.IsPressed(GameButton.Up))
        {
            _cursor.MoveUp();
        }

        if (input.IsPressed(GameButton.Down))
        {
            _cursor.MoveDown();
        }

        return input.IsPressed(GameButton.Confirm) ? Entries[Selected] : null;
    }

    /// <summary>Shows the files as just read again; the cursor stays where it was.</summary>
    /// <param name="entries">The same files, in the same order.</param>
    public void Refresh(IReadOnlyList<SaveSlotEntry> entries) => Entries = entries;
}
