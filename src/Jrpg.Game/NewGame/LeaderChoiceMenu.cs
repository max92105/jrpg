using Jrpg.Core.Content;
using Jrpg.Game.Input;
using Jrpg.Game.Ui;

namespace Jrpg.Game.NewGame;

/// <summary>The leaders a new game offers, in content order, and which one the cursor is on (decision 157).</summary>
internal sealed class LeaderChoiceMenu
{
    private readonly ListCursor _cursor;

    /// <param name="choices">At least one, as Data validates.</param>
    public LeaderChoiceMenu(IReadOnlyList<LeaderChoice> choices)
    {
        Choices = choices;
        _cursor = new ListCursor(choices.Count, _ => true);
    }

    public IReadOnlyList<LeaderChoice> Choices { get; }

    public LeaderChoice Selected => Choices[_cursor.Selected];

    /// <returns>The choice confirmed this update, or null.</returns>
    public LeaderChoice? Update(InputFrame input)
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
