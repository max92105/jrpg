using Jrpg.Game.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Ui;

/// <summary>
/// The menus' code-drawn textures (legacy shapes and gradients), each baked on first use and then reused. One instance
/// lives as long as the game, so reopening a menu bakes nothing.
/// </summary>
internal sealed class UiTextures
{
    private Texture2D? _navTab;
    private Texture2D? _selectedNavTab;
    private Texture2D? _menuScrim;
    private Texture2D? _selectionLine;
    private Texture2D? _selectionFill;

    /// <summary>A tab that isn't selected: the arrow, dark and faintly outlined.</summary>
    public Texture2D NavTab(TextureLibrary textures) =>
        _navTab ??= textures.Create(NavTabShape.Width, NavTabShape.Height, NavTabShape.Pixels(SolidRow(UiTheme.NavFill, NavTabShape.Width), UiTheme.NavStroke));

    /// <summary>The selected tab: the arrow in the blue selection gradient, brightly outlined.</summary>
    public Texture2D SelectedNavTab(TextureLibrary textures) =>
        _selectedNavTab ??= textures.Create(
            NavTabShape.Width,
            NavTabShape.Height,
            NavTabShape.Pixels(GradientStrip.Colors(UiTheme.Selection, NavTabShape.Width), UiTheme.SelectionStroke));

    /// <summary>One row, stretched over the whole screen.</summary>
    public Texture2D MenuScrim(TextureLibrary textures) =>
        _menuScrim ??= textures.Create(VirtualScreen.Width, 1, GradientStrip.Colors(UiTheme.MenuScrim, VirtualScreen.Width));

    /// <summary>One row, as wide as the tab up to its point; stretched to the line's thickness.</summary>
    public Texture2D SelectionLine(TextureLibrary textures) =>
        _selectionLine ??= textures.Create(NavTabShape.ShoulderPixels, 1, GradientStrip.Colors(UiTheme.SelectionLine, NavTabShape.ShoulderPixels));

    /// <summary>The blue selection gradient as one row, stretched over a selected button.</summary>
    public Texture2D SelectionFill(TextureLibrary textures) =>
        _selectionFill ??= textures.Create(NavTabShape.Width, 1, GradientStrip.Colors(UiTheme.Selection, NavTabShape.Width));

    private static Color[] SolidRow(Color color, int width)
    {
        var row = new Color[width];
        Array.Fill(row, color);
        return row;
    }
}
