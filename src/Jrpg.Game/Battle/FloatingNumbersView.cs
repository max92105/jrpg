using Jrpg.Core.Effects;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Battle;

/// <summary>
/// Draws the last action's numbers over their targets (48 px, outlined): colored by what they report
/// (<see cref="FloatingNumber.ColorOf"/>), with a smaller "WEAK" or "RESISTED" cue beside a number its affinity marks.
/// Each turn's numbers are measured once, when its playback starts.
/// </summary>
internal sealed class FloatingNumbersView
{
    private const int TextPixels = 48;

    // Placeholder look: half the number's size, 8 px to its right, popping, rising and fading with it.
    private const int CueTextPixels = 24;
    private const float CueGap = 8;

    private readonly BattleTurnSequence _sequence;
    private readonly BattlersView _battlers;
    private readonly OutlinedFont _font;
    private readonly OutlinedFont _cueFont;
    private readonly MeasuredText _weakCue;
    private readonly MeasuredText _resistedCue;
    private ActionPlayback? _measuredPlayback;
    private MeasuredText[] _measured = [];

    /// <param name="sequence">Whose last action is playing.</param>
    /// <param name="battlers">Where each target's sprite is.</param>
    /// <param name="text">The numbers' and the cues' fonts, and the cues' text.</param>
    public FloatingNumbersView(BattleTurnSequence sequence, BattlersView battlers, ScreenText text)
    {
        _sequence = sequence;
        _battlers = battlers;
        _font = new OutlinedFont(text.Fonts.FontAt(TextPixels));
        _cueFont = new OutlinedFont(text.Fonts.FontAt(CueTextPixels));
        _weakCue = MeasuredText.Of(_cueFont.Font, text.Messages.Text(MessageIds.BattleWeak).ToUpperInvariant());
        _resistedCue = MeasuredText.Of(_cueFont.Font, text.Messages.Text(MessageIds.BattleResisted).ToUpperInvariant());
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
            if (number.Affinity != HitAffinity.Neutral)
            {
                DrawCue(batch, number.Affinity, center + new Vector2(_measured[index].Width * look.Scale / 2, 0), look);
            }
        }
    }

    // The cue starts CueGap right of the number's right edge, centered on the same line.
    private void DrawCue(SpriteBatch batch, HitAffinity affinity, Vector2 numberRight, FloatingNumberLook look)
    {
        var (cue, color) = affinity == HitAffinity.Weak ? (_weakCue, UiTheme.Danger) : (_resistedCue, UiTheme.TextSecondary);
        var center = numberRight + new Vector2((CueGap + (cue.Width / 2)) * look.Scale, 0);
        _cueFont.DrawCentered(batch, cue, center, (color * look.Opacity, look.Scale));
    }
}
