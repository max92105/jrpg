using System.Collections.Immutable;
using Jrpg.Data.Art;
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
    // Wide enough that a gradient stretched over a long bar shows no steps.
    private const int GradientWidth = 256;

    private Texture2D? _navTab;
    private Texture2D? _selectedNavTab;
    private Texture2D? _menuScrim;
    private Texture2D? _selectionLine;
    private Texture2D? _selectionFill;
    private Texture2D? _sectionLine;
    private readonly Dictionary<(string File, int Size), Texture2D> _diamondPortraits = [];
    private readonly Dictionary<ImmutableArray<GradientStop>, Texture2D> _gradients = [];

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

    /// <summary>One row, stretched under a section title.</summary>
    public Texture2D SectionLine(TextureLibrary textures) =>
        _sectionLine ??= textures.Create(NavTabShape.Width, 1, GradientStrip.Colors(UiTheme.SectionLine, NavTabShape.Width));

    /// <summary>A portrait cut into the reference UI's diamond (<see cref="DiamondCrop"/>), once per file and size.</summary>
    /// <param name="textures">Where the portrait is read and the diamond made.</param>
    /// <param name="portraitFile">In the portraits folder.</param>
    /// <param name="size">The diamond's square side in pixels.</param>
    public Texture2D DiamondPortrait(TextureLibrary textures, string portraitFile, int size)
    {
        if (!_diamondPortraits.TryGetValue((portraitFile, size), out var baked))
        {
            var source = textures.Get(ArtFolders.Portraits, portraitFile);
            var pixels = new Color[source.Width * source.Height];
            source.GetData(pixels);
            baked = textures.Create(size, size, DiamondCrop.Bake(pixels, source.Width, source.Height, size));
            _diamondPortraits.Add((portraitFile, size), baked);
        }

        return baked;
    }

    /// <summary>
    /// A gradient as one row, stretched over whatever shows it, made once per gradient. The theme's gradients are
    /// single instances, so they work as keys.
    /// </summary>
    public Texture2D Gradient(TextureLibrary textures, ImmutableArray<GradientStop> stops)
    {
        if (!_gradients.TryGetValue(stops, out var texture))
        {
            texture = textures.Create(GradientWidth, 1, GradientStrip.Colors(stops, GradientWidth));
            _gradients.Add(stops, texture);
        }

        return texture;
    }

    private static Color[] SolidRow(Color color, int width)
    {
        var row = new Color[width];
        Array.Fill(row, color);
        return row;
    }
}
