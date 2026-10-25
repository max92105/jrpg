using System.Collections.Immutable;
using Jrpg.Game.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Ui;

/// <summary>
/// A bar showing how full a value is: a dark track with the filled part in a gradient, which spans the filled part only,
/// so even a short bar shows the whole gradient.
/// </summary>
/// <param name="ui">Where the gradient's texture is made, once.</param>
/// <param name="fill">The gradient, such as <see cref="UiTheme.HpBar"/>.</param>
/// <param name="track">The color behind the filled part, such as <see cref="UiTheme.BarTrack"/>.</param>
/// <param name="fillTint">Multiplies the gradient: white keeps it, a color turns a grey one into that color.</param>
internal sealed class StatBar(UiTextures ui, ImmutableArray<GradientStop> fill, Color track, Color fillTint)
{
    /// <summary>A bar in its gradient's own colors.</summary>
    public StatBar(UiTextures ui, ImmutableArray<GradientStop> fill, Color track)
        : this(ui, fill, track, Color.White)
    {
    }

    /// <summary>A menu bar, on the menus' dark track.</summary>
    public StatBar(UiTextures ui, ImmutableArray<GradientStop> fill)
        : this(ui, fill, UiTheme.BarTrack)
    {
    }

    /// <summary>0 empty to 1 full.</summary>
    public float Fraction { get; private set; }

    /// <summary>Fills the bar to <paramref name="value"/> out of <paramref name="max"/>; empty when there is no max.</summary>
    public void Show(int value, int max) => Fraction = max > 0 ? Math.Clamp((float)value / max, 0, 1) : 0;

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the plain pixel and the gradient come from.</param>
    /// <param name="area">The whole bar, track included.</param>
    /// <param name="opacity">0 to 1, while the menu fades in.</param>
    public void Draw(SpriteBatch batch, TextureLibrary textures, Rectangle area, float opacity)
    {
        batch.Draw(textures.Pixel, area, track * opacity);
        // At least a pixel for anything above zero, so a member on 1 HP never looks like one on 0.
        var filledWidth = Fraction > 0 ? Math.Max(1, (int)MathF.Round(area.Width * Fraction)) : 0;
        if (filledWidth > 0)
        {
            batch.Draw(ui.Gradient(textures, fill), new Rectangle(area.X, area.Y, filledWidth, area.Height), fillTint * opacity);
        }
    }
}
