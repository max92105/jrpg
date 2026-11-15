using Jrpg.Core.Battle;
using Jrpg.Data.Art;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Battle;

/// <summary>
/// Draws the last action's effects on its targets: each strip frame by frame at its record's pace and its frame's own
/// size, centered on the target's drawn sprite, mirrored for a party member's attack (decisions 654, 655), its hit
/// frame on the impact: its lead starts during the wind-up on every target a damaging action aims at (decision 656).
/// Each placeholder cue's text shows on the same center (decision 101). Cue texts are measured once a turn.
/// </summary>
internal sealed class ImpactEffectsView
{
    private const int CueTextPixels = 24;

    private readonly BattleTurnSequence _sequence;
    private readonly BattlersView _battlers;
    private readonly IReadOnlyDictionary<string, EffectStripDefinition> _strips;
    private readonly Dictionary<string, EffectStrip> _paces = new(StringComparer.Ordinal);
    private readonly OutlinedFont _cueFont;
    private ActionPlayback? _measuredPlayback;
    private MeasuredText[] _cueTexts = [];

    /// <param name="sequence">Whose last action is playing.</param>
    /// <param name="battlers">Where each target's sprite is.</param>
    /// <param name="strips">The strips' records by file (ADR 0048), whose pace each plays at.</param>
    /// <param name="text">The cues' font.</param>
    public ImpactEffectsView(
        BattleTurnSequence sequence,
        BattlersView battlers,
        IReadOnlyDictionary<string, EffectStripDefinition> strips,
        ScreenText text)
    {
        _sequence = sequence;
        _battlers = battlers;
        _strips = strips;
        _cueFont = new OutlinedFont(text.Fonts.FontAt(CueTextPixels));
    }

    public void Draw(SpriteBatch batch, TextureLibrary textures)
    {
        DrawWindUpLead(batch, textures);
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
            var effect = playback.Strips[index];
            DrawStrip(batch, textures, (effect.File, effect.Target, effect.IsMirrored), playback.Elapsed);
        }

        for (var index = 0; index < _cueTexts.Length; index++)
        {
            var cue = playback.Cues[index];
            var opacity = PlaceholderCue.OpacityAt(playback.Elapsed - cue.ShowsAt);
            if (opacity > 0)
            {
                var center = _battlers.AreaOf(textures, cue.Target).Center.ToVector2();
                _cueFont.DrawCentered(batch, _cueTexts[index], center, (UiTheme.TextPrimary * opacity, 1));
            }
        }
    }

    // Decision 656: a damaging action's impact strip starts on every target it aims at before Core knows whether it
    // lands; the playback goes on from the impact only where damage landed, so a miss cuts the strip at its hit frame.
    private void DrawWindUpLead(SpriteBatch batch, TextureLibrary textures)
    {
        if (_sequence.Stage != TurnStage.WindingUp || _sequence.WindUp is not { ImpactFile: { } file } windUp)
        {
            return;
        }

        for (var index = 0; index < windUp.AimedTargets.Count; index++)
        {
            DrawStrip(batch, textures, (file, windUp.AimedTargets[index], windUp.IsMirrored), windUp.SinceImpact);
        }
    }

    private void DrawStrip(
        SpriteBatch batch, TextureLibrary textures, (string File, Combatant Target, bool IsMirrored) effect, TimeSpan sinceImpact)
    {
        var image = textures.Get(ArtFolders.Effects, effect.File);
        if (PaceOf(effect.File, image).FrameAtImpactTime(sinceImpact) is not { } frame)
        {
            return;
        }

        // A square frame at its own size, centered on the target's drawn sprite.
        var side = image.Height;
        var center = _battlers.AreaOf(textures, effect.Target).Center;
        var area = new Rectangle(center.X - (side / 2), center.Y - (side / 2), side, side);
        var source = new Rectangle(frame * side, 0, side, side);
        var flip = effect.IsMirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        batch.Draw(image, area, source, Color.White, 0, Vector2.Zero, flip, 0);
    }

    // Worked out once per strip image, not per frame: an unlisted strip's record is made on each lookup.
    private EffectStrip PaceOf(string file, Texture2D image)
    {
        if (!_paces.TryGetValue(file, out var strip))
        {
            strip = EffectStrip.Of(new Point(image.Width, image.Height), EffectStripDefinition.Of(file, _strips));
            _paces.Add(file, strip);
        }

        return strip;
    }
}
