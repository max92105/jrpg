using FontStashSharp;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Menus;

/// <summary>
/// Draws the game menu's tabs down the left: the plain tabs, the selected glow over them where the glide has it, then
/// every label on top. Each tab sits at its own place in the slide-in, with its own fade; the glow takes the selected
/// tab's.
/// </summary>
internal sealed class MenuTabsView
{
    // From one tab's top to the next one's: the tab and a 10 px gap (placeholder).
    private const int Pitch = NavTabShape.Height + 10;

    // The selected tab's label slides 14 px right, over a 2 px bright line along its bottom.
    private const int SelectedLabelSlide = 14;
    private const int SelectionLineThickness = 2;

    private readonly UiTextures _ui;
    private readonly SpriteFontBase _font;
    private readonly string[] _labels;
    private readonly int _top;

    /// <param name="ui">The tab shapes, the glow and the selection line.</param>
    /// <param name="font">The labels' font.</param>
    /// <param name="labels">One per tab, top to bottom.</param>
    /// <param name="top">The first tab's top, in virtual pixels.</param>
    public MenuTabsView(UiTextures ui, SpriteFontBase font, string[] labels, int top)
    {
        _ui = ui;
        _font = font;
        _labels = labels;
        _top = top;
    }

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the tab textures are made.</param>
    /// <param name="place">The selected tab, where its glow has glided to, and the slide-in.</param>
    public void Draw(SpriteBatch batch, TextureLibrary textures, Place place)
    {
        var motion = place.Motion;
        for (var index = 0; index < _labels.Length; index++)
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

        for (var index = 0; index < _labels.Length; index++)
        {
            var labelLeft = TabLeft(motion, index) + UiTheme.MenuTextInset + (index == selected ? SelectedLabelSlide : 0);
            var labelTop = _top + (index * Pitch) + ((NavTabShape.Height - _font.LineHeight) / 2f);
            batch.DrawString(_font, _labels[index], new Vector2(labelLeft, labelTop), UiTheme.TextPrimary * motion.TabOpacity(index));
        }
    }

    private static int TabLeft(MenuMotion motion, int index) => -(int)Math.Round(motion.TabOffset(index));

    /// <summary>What the tabs draw this frame.</summary>
    /// <param name="Motion">The slide-in: each tab's offset and fade.</param>
    /// <param name="Selected">The selected tab.</param>
    /// <param name="GlidePosition">Where the selected glow has glided to, in tabs.</param>
    public readonly record struct Place(MenuMotion Motion, int Selected, float GlidePosition);
}
