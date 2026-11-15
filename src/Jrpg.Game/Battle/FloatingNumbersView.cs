using Jrpg.Core.Effects;
using Jrpg.Data.Art;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Battle;

/// <summary>
/// Draws the last action's numbers on their targets, digit by digit so each hops in turn
/// (<see cref="FloatingNumber"/>), from the damage digit strips (decision 653): each digit its strip's cell cut to its
/// ink, tinted by what the number reports (<see cref="FloatingNumber.ColorOf"/>), a critical from its own 1.4x strip at
/// 1:1. Each is centered on its target, its row resting below a member's feet or above an enemy's tag, with a smaller
/// "WEAK" or "RESISTED" cue in the body font still beside its row. The strips are measured on the first draw; each
/// turn's numbers are laid out once, when its playback starts.
/// </summary>
internal sealed class FloatingNumbersView
{
    // Placeholder look: under half the number's size, 8 px right of its row, level with its middle at rest.
    private const int CueTextPixels = 24;
    private const float CueGap = 8;

    private readonly BattleTurnSequence _sequence;
    private readonly BattlersView _battlers;
    private readonly EnemyTagView _tags;
    private readonly OutlinedFont _cueFont;
    private readonly MeasuredText _weakCue;
    private readonly MeasuredText _resistedCue;
    private DigitStripImage? _normal;
    private DigitStripImage? _critical;
    private ActionPlayback? _laidOutPlayback;
    private DigitRow[] _rows = [];

    /// <param name="sequence">Whose last action is playing.</param>
    /// <param name="battlers">Where each target stands.</param>
    /// <param name="tags">Where each enemy's name tag is, which its numbers keep clear of.</param>
    /// <param name="text">The cues' font and text.</param>
    public FloatingNumbersView(BattleTurnSequence sequence, BattlersView battlers, EnemyTagView tags, ScreenText text)
    {
        _sequence = sequence;
        _battlers = battlers;
        _tags = tags;
        _cueFont = new OutlinedFont(text.Fonts.FontAt(CueTextPixels));
        _weakCue = MeasuredText.Of(_cueFont.Font, text.Messages.Text(MessageIds.BattleWeak).ToUpperInvariant());
        _resistedCue = MeasuredText.Of(_cueFont.Font, text.Messages.Text(MessageIds.BattleResisted).ToUpperInvariant());
    }

    /// <exception cref="InvalidOperationException">
    /// A digit strip isn't ten cells of ink; the message names it.
    /// </exception>
    public void Draw(SpriteBatch batch, TextureLibrary textures)
    {
        if (_sequence.Playback is not { } playback)
        {
            return;
        }

        _normal ??= DigitStripImage.Of(textures, UiArtFiles.Fonts.DamageDigits);
        _critical ??= DigitStripImage.Of(textures, UiArtFiles.Fonts.CriticalDigits);
        if (!ReferenceEquals(playback, _laidOutPlayback))
        {
            _laidOutPlayback = playback;
            _rows = [.. playback.Numbers.Select(RowOf)];
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

    private DigitRow RowOf(FloatingNumber number) =>
        DigitRow.Of(number.Text, StripOf(number).Digits.Advances, FloatingNumber.DigitSpacing);

    private DigitStripImage StripOf(FloatingNumber number) =>
        number.Tone == FloatingNumberTone.Critical ? _critical! : _normal!;

    // Centered on the target; each digit's bottom sits on the rest line plus its offset.
    private void DrawNumber(SpriteBatch batch, FloatingNumber number, DigitRow row, TimeSpan sinceShown)
    {
        var strip = StripOf(number);
        var spot = _battlers.SpotOf(number.Target);
        var rowBottom = FloatingNumber.RowBottomOf(spot, _tags.TagAreaOf(number.Target), number.Text.Length);
        var restLine = rowBottom - (number.Stack * FloatingNumber.StackSpacing);
        var left = spot.Center.X - (row.Width / 2);
        var color = FloatingNumber.ColorOf(number.Tone);
        for (var digit = 0; digit < number.Text.Length; digit++)
        {
            var top = restLine + FloatingNumber.DigitOffsetAt(digit, sinceShown) - strip.Digits.Height;
            var source = strip.Digits.SourceOf(number.Text[digit] - '0');
            batch.Draw(strip.Texture, new Vector2(left + row.LeftOf(digit), top), source, color);
        }

        if (number.Affinity != HitAffinity.Neutral)
        {
            DrawCue(batch, number.Affinity, new Vector2(left + row.Width, restLine - (strip.Digits.Height / 2f)));
        }
    }

    // The cue starts CueGap right of the row's right end, centered on its middle at rest.
    private void DrawCue(SpriteBatch batch, HitAffinity affinity, Vector2 rowRight)
    {
        var (cue, color) = affinity == HitAffinity.Weak ? (_weakCue, UiTheme.Danger) : (_resistedCue, UiTheme.TextSecondary);
        _cueFont.DrawCentered(batch, cue, rowRight + new Vector2(CueGap + (cue.Width / 2), 0), (color, 1));
    }

    // A strip's texture with its digits measured once from the texture's own alpha.
    private sealed record DigitStripImage(Texture2D Texture, DigitStrip Digits)
    {
        public static DigitStripImage Of(TextureLibrary textures, string file)
        {
            var texture = textures.Get(ArtFolders.Fonts, file);
            var pixels = new Color[texture.Width * texture.Height];
            texture.GetData(pixels);
            var alpha = pixels.Select(pixel => pixel.A).ToArray();
            var digits = DigitStrip.Measure(alpha, new Point(texture.Width, texture.Height), file);
            return new DigitStripImage(texture, digits);
        }
    }
}
