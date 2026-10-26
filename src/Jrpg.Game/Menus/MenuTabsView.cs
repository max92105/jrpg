using FontStashSharp;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Menus;

/// <summary>
/// Draws the game menu's tabs down the left (decision 81): the plain backs, the selected tab's glow and bright line
/// where the selection glide has them, then every label on top, the selected one slid right. Each tab sits at its own
/// place in the menu's slide-in, with its own fade; the glow takes the selected tab's.
/// </summary>
internal sealed class MenuTabsView
{
    // From one tab's top to the next one's: the tab and a 10 px gap (placeholder, decision 82).
    private const int TabPitch = NavTabShape.Height + 10;

    // The selected tab's label slides 14 px right, over a 2 px bright line along its bottom.
    private const int SelectedLabelSlide = 14;
    private const int SelectionLineThickness = 2;

    private readonly UiTextures _ui;
    private readonly SpriteFontBase _font;
    private readonly IReadOnlyList<string> _labels;

    /// <param name="ui">The tab and gradient textures.</param>
    /// <param name="font">The labels' font.</param>
    /// <param name="labels">One per tab, top to bottom.</param>
    public MenuTabsView(UiTextures ui, SpriteFontBase font, IReadOnlyList<string> labels)
    {
        _ui = ui;
        _font = font;
        _labels = labels;
    }

    public void Draw(SpriteBatch batch, TextureLibrary textures, Place place)
    {
        for (var index = 0; index < _labels.Count; index++)
        {
            batch.Draw(_ui.NavTab(textures), new Vector2(TabLeft(place, index), place.Top + (index * TabPitch)), Color.White * place.Motion.TabOpacity(index));
        }

        var tint = Color.White * place.Motion.TabOpacity(place.Selected);
        var glowLeft = TabLeft(place, place.Selected);
        var glowTop = place.Top + (int)Math.Round(place.Glide.Position * TabPitch);
        batch.Draw(_ui.SelectedNavTab(textures), new Vector2(glowLeft, glowTop), tint);
        var line = new Rectangle(glowLeft, glowTop + NavTabShape.Height - SelectionLineThickness, NavTabShape.ShoulderPixels, SelectionLineThickness);
        batch.Draw(_ui.SelectionLine(textures), line, tint);

        for (var index = 0; index < _labels.Count; index++)
        {
            var labelLeft = TabLeft(place, index) + UiTheme.MenuTextInset + (index == place.Selected ? SelectedLabelSlide : 0);
            var labelTop = place.Top + (index * TabPitch) + ((NavTabShape.Height - _font.LineHeight) / 2f);
            batch.DrawString(_font, _labels[index], new Vector2(labelLeft, labelTop), UiTheme.TextPrimary * place.Motion.TabOpacity(index));
        }
    }

    private static int TabLeft(Place place, int index) => -(int)Math.Round(place.Motion.TabOffset(index));

    /// <summary>Where the tabs go and how they stand this frame.</summary>
    /// <param name="Top">The first tab's top.</param>
    /// <param name="Selected">The selected tab's index.</param>
    /// <param name="Glide">Where the selected tab's glow is, in tabs.</param>
    /// <param name="Motion">Each tab's slide-in offset and fade.</param>
    public readonly record struct Place(int Top, int Selected, SelectionGlide Glide, MenuMotion Motion);
}
