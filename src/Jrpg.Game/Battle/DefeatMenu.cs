using Jrpg.Game.Input;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Battle;

/// <summary>
/// The defeat screen's choices and which one is selected (decision 65): Load last save first. Only choices that can
/// be used are listed (decision 175): both loads need a readable save, so without one Main menu is the only choice.
/// </summary>
internal sealed class DefeatMenu
{
    private readonly ListCursor _cursor;

    /// <param name="hasReadableSave">Whether any save file can be loaded.</param>
    public DefeatMenu(bool hasReadableSave)
    {
        Entries = hasReadableSave ? [DefeatEntry.LoadLastSave, DefeatEntry.Load, DefeatEntry.MainMenu] : [DefeatEntry.MainMenu];
        _cursor = new ListCursor(Entries.Count, _ => true);
    }

    /// <summary>The choices shown, top to bottom.</summary>
    public IReadOnlyList<DefeatEntry> Entries { get; }

    public DefeatEntry Selected => Entries[_cursor.Selected];

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
