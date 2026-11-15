using Jrpg.Core.Effects;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Battle;

/// <summary>
/// Draws the last action's numbers on their targets, digit by digit so each hops in turn
/// (<see cref="FloatingNumber"/>), in the 54 px body font, outlined; a crit's bigger. Each is centered on its target,
/// its row resting below a member's feet or above an enemy's tag, colored by what it reports
/// (<see cref="FloatingNumber.ColorOf"/>), with a smaller "WEAK" or "RESISTED" cue still beside its row. Each turn's
/// numbers are laid out once, when its playback starts.
/// </summary>
internal sealed class FloatingNumbersView
{
    private const int TextPixels = 54;

    // Placeholder look: under half the number's size, 8 px right of its row, level with its middle at rest.
    private const int CueTextPixels = 24;
    private const float CueGap = 8;

    private readonly BattleTurnSequence _sequence;
    private readonly BattlersView _battlers;
    private readonly EnemyTagView _tags;
    private readonly OutlinedFont _font;
    private readonly OutlinedFont _cueFont;
    private readonly MeasuredText[] _digits;
    private readonly float[] _advances;
    private readonly MeasuredText _weakCue;
    private readonly MeasuredText _resistedCue;
    private ActionPlayback? _laidOutPlayback;
    private DigitRow[] _rows = [];

    /// <param name="sequence">Whose last action is playing.</param>
    /// <param name="battlers">Where each target stands.</param>
    /// <param name="tags">Where each enemy's name tag is, which its numbers keep clear of.</param>
    /// <param name="text">The numbers' and the cues' fonts, and the cues' text.</param>
    public FloatingNumbersView(BattleTurnSequence sequence, BattlersView battlers, EnemyTagView tags, ScreenText text)
    {
        _sequence = sequence;
        _battlers = battlers;
        _tags = tags;
        _font = new OutlinedFont(text.Fonts.FontAt(TextPixels));
        _cueFont = new OutlinedFont(text.Fonts.FontAt(CueTextPixels));
        _digits = [.. Enumerable.Range(0, 10).Select(digit => MeasuredText.Of(_font.Font, NumberText.Of(digit)))];
        _advances = [.. _digits.Select(digit => digit.Width)];
        _weakCue = MeasuredText.Of(_cueFont.Font, text.Messages.Text(MessageIds.BattleWeak).ToUpperInvariant());
        _resistedCue = MeasuredText.Of(_cueFont.Font, text.Messages.Text(MessageIds.BattleResisted).ToUpperInvariant());
    }

    public void Draw(SpriteBatch batch)
    {
        if (_sequence.Playback is not { } playback)
        {
            return;
        }

        if (!ReferenceEquals(playback, _laidOutPlayback))
        {
            _laidOutPlayback = playback;
            _rows = [.. playback.Numbers.Select(number => DigitRow.Of(number.Text, _advances, FloatingNumber.DigitSpacing))];
        }

        for (var index = 0; index < _rows.Length; index++)
        {
            var number = playback.Numbers[index];
            var sinceShown = playback.Elapsed - number.ShowsAt;
            if (FloatingNumber.IsShownAt(sinceShown))
            {
                DrawNumber(batch, number, _rows[index], sinceShown);
            }
        }
    }

    // Centered on the target; each digit's middle sits half a line above its bottom, the rest line plus its offset.
    private void DrawNumber(SpriteBatch batch, FloatingNumber number, DigitRow row, TimeSpan sinceShown)
    {
        var scale = FloatingNumber.SizeOf(number.Tone);
        var spot = _battlers.SpotOf(number.Target);
        var rowBottom = FloatingNumber.RowBottomOf(spot, _tags.TagAreaOf(number.Target), number.Text.Length);
        var restLine = rowBottom - (number.Stack * FloatingNumber.StackSpacing);
        var left = spot.Center.X - (row.Width * scale / 2);
        var halfLine = _font.Font.LineHeight * scale / 2;
        var color = FloatingNumber.ColorOf(number.Tone);
        for (var digit = 0; digit < number.Text.Length; digit++)
        {
            var glyph = _digits[number.Text[digit] - '0'];
            var x = left + ((row.LeftOf(digit) + (glyph.Width / 2)) * scale);
            var y = restLine + FloatingNumber.DigitOffsetAt(digit, sinceShown) - halfLine;
            _font.DrawCentered(batch, glyph, new Vector2(x, y), (color, scale));
        }

        if (number.Affinity != HitAffinity.Neutral)
        {
            DrawCue(batch, number.Affinity, new Vector2(left + (row.Width * scale), restLine - halfLine));
        }
    }

    // The cue starts CueGap right of the row's right end, centered on its middle at rest.
    private void DrawCue(SpriteBatch batch, HitAffinity affinity, Vector2 rowRight)
    {
        var (cue, color) = affinity == HitAffinity.Weak ? (_weakCue, UiTheme.Danger) : (_resistedCue, UiTheme.TextSecondary);
        _cueFont.DrawCentered(batch, cue, rowRight + new Vector2(CueGap + (cue.Width / 2), 0), (color, 1));
    }
}
