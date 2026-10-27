using FontStashSharp;
using Jrpg.Game.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Ui;

/// <summary>
/// Draws one dialog button, so every dialog's buttons look alike: the selected one in the shared selection look
/// with the pointer (decision 252), any other faintly outlined; its label centered.
/// </summary>
internal sealed class DialogButtonView
{
    /// <summary>A button's width, a placeholder.</summary>
    public const int Width = 260;

    /// <summary>A button's height, a placeholder.</summary>
    public const int Height = 50;

    private readonly UiTextures _ui;
    private readonly SpriteFontBase _font;

    /// <param name="ui">The shared selection look.</param>
    /// <param name="font">The labels' font, which they were measured with.</param>
    public DialogButtonView(UiTextures ui, SpriteFontBase font)
    {
        _ui = ui;
        _font = font;
    }

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the shared textures are made.</param>
    /// <param name="button">Where it goes, its label, and whether it's selected.</param>
    /// <param name="opacity">0 to 1, while the screen fades in.</param>
    public void Draw(SpriteBatch batch, TextureLibrary textures, Button button, float opacity)
    {
        var area = button.Area;
        if (button.IsSelected)
        {
            _ui.DrawSelectedRow(batch, textures, area, opacity);
            MenuPointer.Draw(batch, textures.Pixel, new Vector2(area.X, area.Center.Y), opacity);
        }
        else
        {
            Outline.Draw(batch, textures.Pixel, area, UiTheme.Stroke * opacity);
        }

        var labelPosition = new Vector2(area.Center.X - (button.Label.Width / 2), area.Y + ((area.Height - _font.LineHeight) / 2f));
        var color = button.IsSelected ? UiTheme.TextPrimary : UiTheme.TextSecondary;
        batch.DrawString(_font, button.Label.Text, labelPosition, color * opacity);
    }

    /// <summary>One button as drawn this frame.</summary>
    public readonly record struct Button(Rectangle Area, MeasuredText Label, bool IsSelected);
}
