using Jrpg.Game.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Ui;

/// <summary>
/// A row of small diamond pips, like the reference UI's BP: a lit diamond in its color per point, a dim dot for each
/// point missing, and a white flash over the pips that just changed.
/// </summary>
/// <param name="ui">Where the pip diamonds are baked, once per shape.</param>
internal sealed class PipRow(UiTextures ui)
{
    // Placeholders on the 1920 x 1080 virtual screen: 9 px pips, 4 px apart; a missing point is a half-size dot.
    public const int PipSize = 9;
    private const int PipGap = 4;
    private const float DimReach = 0.5f;
    private const float DimOpacity = 0.6f;

    private static readonly DiamondShape _lit = new(PipSize, 0, 1, Color.White, Color.White);
    private static readonly DiamondShape _dim = new(PipSize, 0, DimReach, Color.White, Color.White);

    public static int WidthOf(int count) => count <= 0 ? 0 : (count * PipSize) + ((count - 1) * PipGap);

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the baked diamonds are kept.</param>
    /// <param name="topLeft">The first pip's top left.</param>
    /// <param name="look">How many pips, how many lit, in what color, and what flashes.</param>
    public void Draw(SpriteBatch batch, TextureLibrary textures, Point topLeft, PipRowLook look)
    {
        var lit = ui.Diamond(textures, _lit);
        var dim = ui.Diamond(textures, _dim);
        for (var index = 0; index < look.Count; index++)
        {
            var pip = new Rectangle(topLeft.X + (index * (PipSize + PipGap)), topLeft.Y, PipSize, PipSize);
            var isLit = index < look.Lit;
            batch.Draw(isLit ? lit : dim, pip, isLit ? look.Color * look.Opacity : UiTheme.TextSecondary * (DimOpacity * look.Opacity));
            if (look.Flash.Covers(index))
            {
                batch.Draw(lit, pip, Color.White * (look.Flash.Share * look.Opacity));
            }
        }
    }
}
