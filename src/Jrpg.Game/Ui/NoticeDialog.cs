using Jrpg.Game.Input;

namespace Jrpg.Game.Ui;

/// <summary>
/// A notice the player closes with its one OK (decision 268), such as why a Confirm changed nothing. It holds no state:
/// whoever shows it drops it when <see cref="IsClosedBy"/> says so.
/// </summary>
/// <param name="Title">Over the window's separator, already in the player's language.</param>
/// <param name="Message">One line under it, already in the player's language.</param>
internal sealed record NoticeDialog(string Title, string Message)
{
    /// <summary>Confirm presses OK; Cancel closes it as well, as nothing else is on offer.</summary>
    public static bool IsClosedBy(InputFrame input) => input.IsPressed(GameButton.Confirm) || input.IsPressed(GameButton.Cancel);
}
