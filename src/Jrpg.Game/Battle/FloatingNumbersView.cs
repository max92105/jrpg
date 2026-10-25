using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Battle;

/// <summary>
/// Draws the last action's numbers over their targets (40 px, outlined): colored by what they report
/// (<see cref="FloatingNumber.ColorOf"/>). Each turn's numbers are measured once, when its playback starts.
/// </summary>
internal sealed class FloatingNumbersView
{
    private const int TextPixels = 40;

    private readonly BattleTurnSequence _sequence;
    private readonly BattlersView _battlers;
    private readonly OutlinedFont _font;
    private ActionPlayback? _measuredPlayback;
    private MeasuredText[] _measured = [];

    /// <param name="sequence">Whose last action is playing.</param>
    /// <param name="battlers">Where each target's sprite is.</param>
    /// <param name="text">The numbers' font.</param>
    public FloatingNumbersView(BattleTurnSequence sequence, BattlersView battlers, ScreenText text)
    {
        _sequence = sequence;
        _battlers = battlers;
        _font = new OutlinedFont(text.Fonts.FontAt(TextPixels));
    }

    public void Draw(SpriteBatch batch, TextureLibrary textures)
    {
        if (_sequence.Playback is not { } playback)
        {
            return;
        }

        if (!ReferenceEquals(playback, _measuredPlayback))
        {
            _measuredPlayback = playback;
            _measured = [.. playback.Numbers.Select(number => MeasuredText.Of(_font.Font, number.Text))];
        }

        for (var index = 0; index < _measured.Length; index++)
        {
            var number = playback.Numbers[index];
            var sinceShown = playback.Elapsed - number.ShowsAt;
            var look = FloatingNumber.At(sinceShown);
            if (sinceShown < TimeSpan.Zero || look.Opacity <= 0)
            {
                continue;
            }

            var center = new Vector2(_battlers.SpotOf(number.Target).Center.X, _battlers.TopOf(textures, number.Target) - look.Rise - (number.Stack * FloatingNumber.StackSpacing));
            _font.DrawCentered(batch, _measured[index], center, (FloatingNumber.ColorOf(number.Tone) * look.Opacity, look.Scale));
        }
    }
}
