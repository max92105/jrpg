using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Battle;

/// <summary>
/// Draws the impact's light where damage landed (decision 660, ADR 0049): the battlefield's warm flash, additive over
/// the backdrop and the battlers, under the numbers and the HUD. A heal, a miss or a status alone lands no strip, so it
/// lights nothing. The white wash on the battlers near the hit is <see cref="BattlersView"/>'s.
/// </summary>
/// <param name="sequence">Whose last action is playing, and where its damage landed.</param>
internal sealed class ImpactLightView(BattleTurnSequence sequence)
{
    private static readonly Rectangle _screen = new(0, 0, VirtualScreen.Width, VirtualScreen.Height);

    public void DrawOverTheBattlers(SpriteBatch batch, TextureLibrary textures)
    {
        if (sequence.Playback is not { Strips.Count: > 0 } playback)
        {
            return;
        }

        var flash = ImpactLight.At(playback.Elapsed).SceneFlash;
        if (flash <= 0)
        {
            return;
        }

        batch.End();
        VirtualScreen.BeginLightBatch(batch);
        batch.Draw(textures.Pixel, _screen, UiTheme.ImpactWarmth * flash);
        batch.End();
        VirtualScreen.BeginBatch(batch);
    }
}
