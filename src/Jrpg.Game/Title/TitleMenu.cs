using System.Collections.Immutable;
using Jrpg.Game.Input;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Title;

/// <summary>
/// The title screen's entries and which one is selected (decision 80). Continue and Load need a readable save; without
/// one they still show, greyed, and the cursor passes over them.
/// </summary>
internal sealed class TitleMenu
{
    private readonly ListCursor _cursor;
    private readonly bool _hasReadableSave;

    /// <param name="hasReadableSave">Whether any save file can be loaded.</param>
    public TitleMenu(bool hasReadableSave)
    {
        _hasReadableSave = hasReadableSave;
        _cursor = new ListCursor(Entries.Length, index => IsEnabled(Entries[index]));
    }

    public static ImmutableArray<TitleEntry> Entries { get; } = [TitleEntry.NewGame, TitleEntry.Continue, TitleEntry.Load, TitleEntry.Quit];

    public TitleEntry Selected => Entries[_cursor.Selected];

    public bool IsEnabled(TitleEntry entry) => _hasReadableSave || entry is not (TitleEntry.Continue or TitleEntry.Load);

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
