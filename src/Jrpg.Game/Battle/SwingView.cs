using Jrpg.Core.Battle;
using Jrpg.Data.Art;
using Jrpg.Game.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Battle;

/// <summary>
/// Draws the swing of an Attack's or an action's own strip in front of its actor (decisions 657, 708): the strip its
/// impact strip's record names, from <see cref="BattlePacing.SwingStart"/> into the wind-up to its last frame, past the
/// impact, at its own pace and size, its frame centered 60 px ahead of the attacker toward the other side, mirrored for
/// a party member.
/// </summary>
internal sealed class SwingView
{
    // How far ahead of the attacker's drawn center the swing's frame is centered (the clip's slash).
    private const int Reach = 60;

    private readonly BattleTurnSequence _sequence;
    private readonly BattlersView _battlers;
    private readonly IReadOnlyDictionary<string, EffectStripDefinition> _strips;
    private readonly Dictionary<string, EffectStrip> _paces = new(StringComparer.Ordinal);

    /// <param name="sequence">Whose action is winding up, and its swing.</param>
    /// <param name="battlers">Where the attacker is drawn.</param>
    /// <param name="strips">The strips' records by file (ADR 0048), whose pace the swing plays at.</param>
    public SwingView(
        BattleTurnSequence sequence, BattlersView battlers, IReadOnlyDictionary<string, EffectStripDefinition> strips)
    {
        _sequence = sequence;
        _battlers = battlers;
        _strips = strips;
    }

    public void Draw(SpriteBatch batch, TextureLibrary textures)
    {
        if (_sequence.WindUp is not { SwingFile: { } file } windUp || windUp.SinceSwing < TimeSpan.Zero)
        {
            return;
        }

        var image = textures.Get(ArtFolders.Effects, file);
        if (PaceOf(file, image).FrameAt(windUp.SinceSwing) is not { } frame)
        {
            return;
        }

        // A party member swings leftward at the enemies, an enemy rightward at the party.
        var center = _battlers.AreaOf(textures, windUp.Actor).Center;
        var ahead = windUp.IsMirrored ? -Reach : Reach;
        var side = image.Height;
        var area = new Rectangle(center.X + ahead - (side / 2), center.Y - (side / 2), side, side);
        var flip = windUp.IsMirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        batch.Draw(image, area, new Rectangle(frame * side, 0, side, side), Color.White, 0, Vector2.Zero, flip, 0);
    }

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
