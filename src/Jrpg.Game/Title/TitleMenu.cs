using Jrpg.Game.Input;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Title;

/// <summary>
/// The title screen's entries and which one is selected (decision 156). Only entries that can be used are listed:
/// Continue and Load need a readable save, and Continue then comes first and starts selected (decision 177).
/// </summary>
internal sealed class TitleMenu
{
    private readonly ListCursor _cursor;

    /// <param name="hasReadableSave">Whether any save file can be loaded.</param>
    public TitleMenu(bool hasReadableSave)
    {
        Entries = hasReadableSave
            ? [TitleEntry.Continue, TitleEntry.NewGame, TitleEntry.Load, TitleEntry.Quit]
            : [TitleEntry.NewGame, TitleEntry.Quit];
        _cursor = new ListCursor(Entries.Count, _ => true);
    }

    /// <summary>The entries shown, top to bottom.</summary>
    public IReadOnlyList<TitleEntry> Entries { get; }

    public TitleEntry Selected => Entries[_cursor.Selected];

    /// <returns>The entry confirmed this update, or null.</returns>
    public TitleEntry? Update(InputFrame input)
    {
        if (input.IsPressed(GameButton.Up))
        {
            _cursor.MoveUp();
        }

        if (input.IsPressed(GameButton.Down))
        {
            _cursor.MoveDown();
        }

        return input.IsPressed(GameButton.Confirm) ? Selected : null;
    }
}
