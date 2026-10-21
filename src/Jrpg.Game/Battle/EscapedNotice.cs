using Jrpg.Game.Input;
using Jrpg.Game.Scenes;

namespace Jrpg.Game.Battle;

/// <summary>"Escaped" over the battle after a successful Run (legacy <c>ShowEscaped</c>): it leaves by itself, or on Confirm.</summary>
internal sealed class EscapedNotice
{
    // Legacy EscapedDisplayTime.
    private static readonly TimeSpan _displayTime = TimeSpan.FromSeconds(1.5);

    private TimeSpan _elapsed;
    private bool _hasLeft;

    /// <returns><see cref="BattleFinished"/> once, when the display time is up or on Confirm; null otherwise.</returns>
    public SceneRequest? Update(TimeSpan delta, InputFrame input)
    {
        if (_hasLeft)
        {
            return null;
        }

        _elapsed += delta;
        if (_elapsed < _displayTime && !input.IsPressed(GameButton.Confirm))
        {
            return null;
        }

        _hasLeft = true;
        return new BattleFinished();
    }
}
