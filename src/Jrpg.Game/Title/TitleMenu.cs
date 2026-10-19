using System.Collections.Immutable;
using Jrpg.Game.Input;
using Jrpg.Game.Scenes;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Title;

/// <summary>The title screen's entries and which one is selected; what confirming each asks the flow for.</summary>
internal sealed class TitleMenu
{
    private readonly ListCursor _cursor;

    public TitleMenu()
    {
        _cursor = new ListCursor(Entries.Length, _ => true);
    }

    public static ImmutableArray<TitleEntry> Entries { get; } = [TitleEntry.NewGame, TitleEntry.Quit];

    public TitleEntry Selected => Entries[_cursor.Selected];

    /// <returns>The request of the entry confirmed this update, or null.</returns>
    public SceneRequest? Update(InputFrame input)
    {
        if (input.IsPressed(GameButton.Up))
        {
            _cursor.MoveUp();
        }

        if (input.IsPressed(GameButton.Down))
        {
            _cursor.MoveDown();
        }

        if (!input.IsPressed(GameButton.Confirm))
        {
            return null;
        }

        return Selected switch
        {
            TitleEntry.NewGame => new NewGameRequested(),
            TitleEntry.Quit => new QuitRequested(),
            _ => throw new InvalidOperationException($"The title has no request for {Selected}."),
        };
    }
}
