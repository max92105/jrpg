using FontStashSharp;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Menus;

/// <summary>
/// Draws the game menu's tabs down the left: the plain tabs, the selected glow over them where the glide has it, then
/// every icon and label on top. Each tab sits at its own place in the slide-in, with its own fade; the glow takes the
/// selected tab's.
/// </summary>
internal sealed class MenuTabsView
{
    // From one tab's top to the next one's: the tab and a 10 px gap (placeholder).
    private const int Pitch = NavTabShape.Height + 10;

    // The selected tab's icon and label slide 14 px right, over a 2 px bright line along its bottom.
    private const int SelectedTabSlide = 14;
    private const int SelectionLineThickness = 2;

    // Each tab's icon, its 12 px mask at 2x (24 px), then the label 12 px after it (decision 254, placeholder).
    private const int IconScale = 2;
    private const int IconSize = TabGlyphShape.Size * IconScale;
    private const int IconGap = 12;

    private readonly UiTextures _ui;
    private readonly SpriteFontBase _font;
    private readonly IReadOnlyList<Tab> _tabs;
    private readonly int _top;

    /// <param name="ui">The tab shapes, the glow and the selection line.</param>
    /// <param name="font">The labels' font.</param>
    /// <param name="tabs">One per tab, top to bottom.</param>
    /// <param name="top">The first tab's top, in virtual pixels.</param>
    public MenuTabsView(UiTextures ui, SpriteFontBase font, IReadOnlyList<Tab> tabs, int top)
    {
        _ui = ui;
        _font = font;
        _tabs = tabs;
        _top = top;
    }

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the tab textures are made.</param>
    /// <param name="place">The selected tab, where its glow has glided to, and the slide-in.</param>
    public void Draw(SpriteBatch batch, TextureLibrary textures, Place place)
    {
        var motion = place.Motion;
        for (var index = 0; index < _tabs.Count; index++)
        {
            batch.Draw(_ui.NavTab(textures), new Vector2(TabLeft(motion, index), _top + (index * Pitch)), Color.White * motion.TabOpacity(index));
        }

        var selected = place.Selected;
        var tint = Color.White * motion.TabOpacity(selected);
        var glowLeft = TabLeft(motion, selected);
        var glowTop = _top + (int)Math.Round(place.GlidePosition * Pitch);
        batch.Draw(_ui.SelectedNavTab(textures), new Vector2(glowLeft, glowTop), tint);
        var line = new Rectangle(glowLeft, glowTop + NavTabShape.Height - SelectionLineThickness, NavTabShape.ShoulderPixels, SelectionLineThickness);
        batch.Draw(_ui.SelectionLine(textures), line, tint);

        for (var index = 0; index < _tabs.Count; index++)
        {
            DrawIconAndLabel(batch, textures, place, index);
        }
    }

    // The icon then the label, both sliding right on the selected tab.
    private void DrawIconAndLabel(SpriteBatch batch, TextureLibrary textures, Place place, int index)
    {
        var opacity = place.Motion.TabOpacity(index);
        var left = TabLeft(place.Motion, index) + UiTheme.MenuTextInset + (index == place.Selected ? SelectedTabSlide : 0);
        var tabTop = _top + (index * Pitch);
        var icon = new Rectangle(left, tabTop + ((NavTabShape.Height - IconSize) / 2), IconSize, IconSize);
        batch.Draw(_ui.TabIcon(textures, _tabs[index].Glyph), icon, Color.White * opacity);
        var labelTop = tabTop + ((NavTabShape.Height - _font.LineHeight) / 2f);
        batch.DrawString(_font, _tabs[index].Label, new Vector2(icon.Right + IconGap, labelTop), UiTheme.TextPrimary * opacity);
    }

    private static int TabLeft(MenuMotion motion, int index) => -(int)Math.Round(motion.TabOffset(index));

    /// <summary>What the tabs draw this frame.</summary>
    /// <param name="Motion">The slide-in: each tab's offset and fade.</param>
    /// <param name="Selected">The selected tab.</param>
    /// <param name="GlidePosition">Where the selected glow has glided to, in tabs.</param>
    public readonly record struct Place(MenuMotion Motion, int Selected, float GlidePosition);

    /// <summary>One tab's name and its icon.</summary>
    public readonly record struct Tab(string Label, TabGlyph Glyph);

}
