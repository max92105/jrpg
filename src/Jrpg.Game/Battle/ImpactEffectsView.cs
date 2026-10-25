using Jrpg.Core.Battle;
using Jrpg.Data.Art;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Battle;

/// <summary>
/// Draws the last action's effects on its targets: each strip frame by frame, 96 px, 30 px below the target's top
/// (decision 57), and each placeholder cue's text in the same place (decision 101). Cue texts are measured once a turn.
/// </summary>
internal sealed class ImpactEffectsView
{
    private const int EffectSize = 96;
    private const int BelowTargetTop = 30;
    private const int CueTextPixels = 24;

    private readonly BattleTurnSequence _sequence;
    private readonly BattlersView _battlers;
    private readonly OutlinedFont _cueFont;
    private ActionPlayback? _measuredPlayback;
    private MeasuredText[] _cueTexts = [];

    /// <param name="sequence">Whose last action is playing.</param>
    /// <param name="battlers">Where each target's sprite is.</param>
    /// <param name="text">The cues' font.</param>
    public ImpactEffectsView(BattleTurnSequence sequence, BattlersView battlers, ScreenText text)
    {
        _sequence = sequence;
        _battlers = battlers;
        _cueFont = new OutlinedFont(text.Fonts.FontAt(CueTextPixels));
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
            _cueTexts = [.. playback.Cues.Select(cue => MeasuredText.Of(_cueFont.Font, cue.Text))];
        }

        for (var index = 0; index < playback.Strips.Count; index++)
        {
            DrawStrip(batch, textures, playback.Strips[index], playback.Elapsed);
        }

        for (var index = 0; index < _cueTexts.Length; index++)
        {
            var cue = playback.Cues[index];
            var opacity = PlaceholderCue.OpacityAt(playback.Elapsed - cue.ShowsAt);
            if (opacity > 0)
            {
                var area = EffectArea(textures, cue.Target);
                _cueFont.DrawCentered(batch, _cueTexts[index], area.Center.ToVector2(), (UiTheme.TextPrimary * opacity, 1));
            }
        }
    }

    private void DrawStrip(SpriteBatch batch, TextureLibrary textures, EffectOnTarget effect, TimeSpan elapsed)
    {
        var image = textures.Get(ArtFolders.Effects, effect.File);
        if (EffectStrip.Of(new Point(image.Width, image.Height)).FrameAt(elapsed) is not { } frame)
        {
            return;
        }

        var source = new Rectangle(frame * image.Height, 0, image.Height, image.Height);
        batch.Draw(image, EffectArea(textures, effect.Target), source, Color.White);
    }

    private Rectangle EffectArea(TextureLibrary textures, Combatant target)
    {
        var centerX = _battlers.SpotOf(target).Center.X;
        return new Rectangle(centerX - (EffectSize / 2), _battlers.TopOf(textures, target) + BelowTargetTop, EffectSize, EffectSize);
    }
}
