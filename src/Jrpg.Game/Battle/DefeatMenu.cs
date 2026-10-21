using System.Collections.Immutable;
using Jrpg.Game.Input;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Battle;

/// <summary>
/// The defeat screen's choices and which one is selected (decision 65): Load last save first. Both loads need a readable
/// save; without one they stay listed, greyed, and the cursor starts on Main menu.
/// </summary>
internal sealed class DefeatMenu
{
    private readonly ListCursor _cursor;
    private readonly bool _hasReadableSave;

    /// <param name="hasReadableSave">Whether any save file can be loaded.</param>
    public DefeatMenu(bool hasReadableSave)
    {
        _hasReadableSave = hasReadableSave;
        _cursor = new ListCursor(Entries.Length, index => IsEnabled(Entries[index]));
    }

    public static ImmutableArray<DefeatEntry> Entries { get; } = [DefeatEntry.LoadLastSave, DefeatEntry.Load, DefeatEntry.MainMenu];

    public DefeatEntry Selected => Entries[_cursor.Selected];

    public bool IsEnabled(DefeatEntry entry) => _hasReadableSave || entry == DefeatEntry.MainMenu;

    /// <returns>The entry confirmed this update, or null.</returns>
    public DefeatEntry? Update(InputFrame input)
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
