using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Battle;

/// <summary>
/// Draws the impact's light where damage landed (decision 660, ADR 0049), each part in its own additive batch: the warm
/// glow on the floor at each hit target's feet, under the battlers; the battlefield's warm flash over the backdrop and
/// the battlers, under the HUD; and the warm halo on each hit target after the flash, over the impact strip. A heal, a
/// miss or a status alone lands no strip, so it lights nothing. The white wash on the battlers near the hit is
/// <see cref="BattlersView"/>'s.
/// </summary>
/// <param name="sequence">Whose last action is playing, and where its damage landed.</param>
/// <param name="battlers">Where each hit target stands and is drawn.</param>
/// <param name="ui">Where the soft disc is baked, once.</param>
internal sealed class ImpactLightView(BattleTurnSequence sequence, BattlersView battlers, UiTextures ui)
{
    // Placeholder sizes from the clip: a wide flat oval the target stands in, a disc a little wider than a goblin.
    private const int FloorGlowWidth = 600;
    private const int FloorGlowHeight = 200;
    private const int HaloSize = 280;

    private static readonly Rectangle _screen = new(0, 0, VirtualScreen.Width, VirtualScreen.Height);

    public void DrawUnderTheBattlers(SpriteBatch batch, TextureLibrary textures)
    {
        if (LitImpactNow() is not { Look.FloorGlow: > 0 } lit)
        {
            return;
        }

        BeginLight(batch);
        var glow = ui.RadialGlow(textures);
        for (var index = 0; index < lit.Playback.Strips.Count; index++)
        {
            var feet = battlers.SpotOf(lit.Playback.Strips[index].Target);
            var area = new Rectangle(
                feet.Center.X - (FloorGlowWidth / 2), feet.Bottom - (FloorGlowHeight / 2), FloorGlowWidth, FloorGlowHeight);
            batch.Draw(glow, area, UiTheme.ImpactFloor * lit.Look.FloorGlow);
        }

        EndLight(batch);
    }

    public void DrawOverTheBattlers(SpriteBatch batch, TextureLibrary textures)
    {
        if (LitImpactNow() is not { Look.SceneFlash: > 0 } lit)
        {
            return;
        }

        BeginLight(batch);
        batch.Draw(textures.Pixel, _screen, UiTheme.ImpactWarmth * lit.Look.SceneFlash);
        EndLight(batch);
    }

    public void DrawOverTheEffects(SpriteBatch batch, TextureLibrary textures)
    {
        if (LitImpactNow() is not { Look.Halo: > 0 } lit)
        {
            return;
        }

        BeginLight(batch);
        var glow = ui.RadialGlow(textures);
        for (var index = 0; index < lit.Playback.Strips.Count; index++)
        {
            var center = battlers.AreaOf(textures, lit.Playback.Strips[index].Target).Center;
            var area = new Rectangle(center.X - (HaloSize / 2), center.Y - (HaloSize / 2), HaloSize, HaloSize);
            batch.Draw(glow, area, UiTheme.ImpactHalo * lit.Look.Halo);
        }

        EndLight(batch);
    }

    // The playback whose damage landed somewhere, with its light now; null while nothing landed.
    private LitImpact? LitImpactNow() =>
        sequence.Playback is { Strips.Count: > 0 } playback ? new LitImpact(playback, ImpactLight.At(playback.Elapsed)) : null;

    private static void BeginLight(SpriteBatch batch)
    {
        batch.End();
        VirtualScreen.BeginLightBatch(batch);
    }

    private static void EndLight(SpriteBatch batch)
    {
        batch.End();
        VirtualScreen.BeginBatch(batch);
    }

    private readonly record struct LitImpact(ActionPlayback Playback, ImpactLightLook Look);
}
