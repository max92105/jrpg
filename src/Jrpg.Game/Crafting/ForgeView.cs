using FontStashSharp;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Crafting;

/// <summary>
/// Draws the Forge screen: a translucent dark layer over the map, which shows through it, and the title. A placeholder
/// look until the plan's F1d.
/// </summary>
internal sealed class ForgeView
{
    // The menu header's title size and side margin, so the Forge's title reads as a screen's.
    private const int TitleTextPixels = 38;
    private const int TitleMargin = 48;

    private readonly SpriteFontBase _titleFont;
    private readonly string _title;

    public ForgeView(ScreenText text)
    {
        _titleFont = text.Fonts.FontAt(TitleTextPixels);
        _title = text.Messages.Text(MessageIds.ForgeTitle);
    }

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the plain pixel comes from.</param>
    public void Draw(SpriteBatch batch, TextureLibrary textures)
    {
        batch.Draw(textures.Pixel, new Rectangle(0, 0, VirtualScreen.Width, VirtualScreen.Height), UiTheme.Scrim);
        batch.DrawString(_titleFont, _title, new Vector2(TitleMargin, TitleMargin), UiTheme.TextPrimary);
    }
}
