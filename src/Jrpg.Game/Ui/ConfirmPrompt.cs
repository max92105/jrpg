using Jrpg.Game.Input;

namespace Jrpg.Game.Ui;

/// <summary>
/// A yes or no question: Yes above No, and No selected at first so a stray Confirm changes nothing. Up or Left selects
/// Yes, Down or Right selects No (Left and Right from when they sat side by side); Cancel answers No.
/// </summary>
/// <param name="question">What is asked, already in the player's language.</param>
internal sealed class ConfirmPrompt(string question)
{
    public string Question => question;

    public bool IsYesSelected { get; private set; }

    /// <returns>The answer on the update the player gives it; null while they choose.</returns>
    public ConfirmAnswer? Update(InputFrame input)
    {
        if (input.IsPressed(GameButton.Cancel))
        {
            return ConfirmAnswer.No;
        }

        if (input.IsPressed(GameButton.Up) || input.IsPressed(GameButton.Left))
        {
            IsYesSelected = true;
        }

        if (input.IsPressed(GameButton.Down) || input.IsPressed(GameButton.Right))
        {
            IsYesSelected = false;
        }

        if (!input.IsPressed(GameButton.Confirm))
        {
            return null;
        }

        return IsYesSelected ? ConfirmAnswer.Yes : ConfirmAnswer.No;
    }
}
