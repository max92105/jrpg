using System.Collections.Immutable;
using Jrpg.Data.Art;
using Jrpg.Game.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Ui;

/// <summary>
/// The menus' code-drawn textures (shapes and gradients), each baked on first use and then reused. One instance
/// lives as long as the game, so reopening a menu bakes nothing.
/// </summary>
internal sealed class UiTextures
{
    // Wide enough that a gradient stretched over a long bar shows no steps.
    private const int GradientWidth = 256;

    // A quarter of the screen: the vignette changes slowly enough that stretching it shows no steps.
    private const int VignetteWidth = VirtualScreen.Width / 4;
    private const int VignetteHeight = VirtualScreen.Height / 4;

    /// <summary>The battle's top shade covers the top 320 pixels.</summary>
    public const int TopShadeHeight = 320;

    private Texture2D? _navTab;
    private Texture2D? _selectedNavTab;
    private Texture2D? _menuScrim;
    private Texture2D? _selectionLine;
    private Texture2D? _sectionLine;
    private Texture2D? _battleVignette;
    private Texture2D? _topShade;
    private Texture2D? _commandHeader;
    private Texture2D? _rewardBar;
    private Texture2D? _cardHeader;
    private readonly Dictionary<(string File, int Size), Texture2D> _diamondPortraits = [];
    private readonly Dictionary<(string Folder, string File, int Size), Texture2D> _shrunk = [];
    private readonly Dictionary<DiamondShape, Texture2D> _diamonds = [];
    private readonly Dictionary<Texture2D, Texture2D> _silhouettes = [];
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

    /// <summary>One row, stretched under a section title.</summary>
    public Texture2D SectionLine(TextureLibrary textures) =>
        _sectionLine ??= textures.Create(NavTabShape.Width, 1, GradientStrip.Colors(UiTheme.SectionLine, NavTabShape.Width));

    /// <summary>Darkens the battle's edges and corners; stretched over the whole screen.</summary>
    public Texture2D BattleVignette(TextureLibrary textures) =>
        _battleVignette ??= textures.Create(VignetteWidth, VignetteHeight, Vignette.Colors(UiTheme.BattleVignette, VignetteWidth, VignetteHeight));

    /// <summary>One column, stretched over the top of the battle, <see cref="TopShadeHeight"/> pixels down.</summary>
    public Texture2D TopShade(TextureLibrary textures) =>
        _topShade ??= textures.Create(1, TopShadeHeight, GradientStrip.Colors(UiTheme.BattleTopShade, TopShadeHeight));

    /// <summary>The battle command panel's header tab, in the selection gradient with its bright outline.</summary>
    public Texture2D CommandHeader(TextureLibrary textures) =>
        _commandHeader ??= textures.Create(
            CommandHeaderShape.Width,
            CommandHeaderShape.Height,
            CommandHeaderShape.Pixels(GradientStrip.Colors(UiTheme.Selection, CommandHeaderShape.Width), UiTheme.SelectionStroke));

    /// <summary>A victory reward row, pointed on the left and fading out to the right.</summary>
    public Texture2D RewardBar(TextureLibrary textures) =>
        _rewardBar ??= textures.Create(
            PointedBarShape.RewardBarWidth,
            PointedBarShape.RewardBarHeight,
            PointedBarShape.RewardBar(GradientStrip.Colors(UiTheme.RewardBar, PointedBarShape.RewardBarWidth)));

    /// <summary>A victory card's name tab, pointed at both ends, with a faint outline.</summary>
    public Texture2D CardHeader(TextureLibrary textures) =>
        _cardHeader ??= textures.Create(
            PointedBarShape.CardHeaderWidth,
            PointedBarShape.CardHeaderHeight,
            PointedBarShape.CardHeader(GradientStrip.Colors(UiTheme.CardHeader, PointedBarShape.CardHeaderWidth), UiTheme.Stroke));

    /// <summary>
    /// <paramref name="source"/>'s shape in plain white, made once per texture: drawn over a sprite, it lights it white, with
    /// no shader.
    /// </summary>
    public Texture2D WhiteSilhouette(TextureLibrary textures, Texture2D source)
    {
        if (!_silhouettes.TryGetValue(source, out var baked))
        {
            var pixels = new Color[source.Width * source.Height];
            source.GetData(pixels);
            for (var index = 0; index < pixels.Length; index++)
            {
                var alpha = pixels[index].A;
                pixels[index] = new Color(alpha, alpha, alpha, alpha);
            }

            baked = textures.Create(source.Width, source.Height, pixels);
            _silhouettes.Add(source, baked);
        }

        return baked;
    }

    /// <summary>A diamond or diamond ring, baked once per shape.</summary>
    public Texture2D Diamond(TextureLibrary textures, DiamondShape shape)
    {
        if (!_diamonds.TryGetValue(shape, out var baked))
        {
            baked = textures.Create(shape.Size, shape.Size, shape.Pixels());
            _diamonds.Add(shape, baked);
        }

        return baked;
    }

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

    /// <summary>An icon shrunk smoothly from its 1024 px art (<see cref="SquareShrink"/>), once per file and size.</summary>
    /// <param name="textures">Where the icon is read and the small one made.</param>
    /// <param name="iconFile">In the icons folder.</param>
    /// <param name="size">The drawn square's side in pixels.</param>
    public Texture2D Icon(TextureLibrary textures, string iconFile, int size) => ShrunkOnce(textures, ArtFolders.Icons, iconFile, size);

    /// <summary>A painted battler shrunk smoothly (<see cref="SquareShrink"/>), once per file and size (decision 100).</summary>
    /// <param name="textures">Where the battler is read and the small one made.</param>
    /// <param name="spriteFile">In the battlers folder.</param>
    /// <param name="size">The drawn square's side in pixels.</param>
    public Texture2D Battler(TextureLibrary textures, string spriteFile, int size) =>
        ShrunkOnce(textures, ArtFolders.Battlers, spriteFile, size);

    /// <summary>A painted portrait resized smoothly (<see cref="SquareShrink"/>), once per file and size.</summary>
    /// <param name="textures">Where the portrait is read and the resized one made.</param>
    /// <param name="portraitFile">In the portraits folder.</param>
    /// <param name="size">The drawn square's side in pixels.</param>
    public Texture2D Portrait(TextureLibrary textures, string portraitFile, int size) =>
        ShrunkOnce(textures, ArtFolders.Portraits, portraitFile, size);

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

    /// <summary>
    /// The one selected-row look of every list (decision 252): the <see cref="UiTheme.SelectedRow"/> gradient between
    /// two bright hairlines. The list with the cursor adds <see cref="MenuPointer"/> at the row's left.
    /// </summary>
    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the gradients are made.</param>
    /// <param name="row">The row's area.</param>
    /// <param name="opacity">0 to 1, while the menu fades in.</param>
    public void DrawSelectedRow(SpriteBatch batch, TextureLibrary textures, Rectangle row, float opacity)
    {
        var tint = Color.White * opacity;
        batch.Draw(Gradient(textures, UiTheme.SelectedRow), row, tint);
        var line = Gradient(textures, UiTheme.HighlightLine);
        batch.Draw(line, new Rectangle(row.X, row.Y, row.Width, 1), tint);
        batch.Draw(line, new Rectangle(row.X, row.Bottom - 1, row.Width, 1), tint);
    }

    /// <summary>
    /// A page list's selected row, the same on every list (decision 262): the band where the highlight has glided to,
    /// and the pointer bobbing at its left.
    /// </summary>
    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the gradients and the plain pixel come from.</param>
    /// <param name="row">From the list's <see cref="SelectionMotion"/>.</param>
    /// <param name="opacity">0 to 1, while the menu fades in.</param>
    public void DrawGlidingSelection(SpriteBatch batch, TextureLibrary textures, GlidingRow row, float opacity)
    {
        DrawSelectedRow(batch, textures, row.Area, opacity);
        MenuPointer.Draw(batch, textures.Pixel, new Vector2(row.Area.X - row.PointerOffset, row.Area.Center.Y), opacity);
    }

    private Texture2D ShrunkOnce(TextureLibrary textures, string folder, string file, int size)
    {
        if (!_shrunk.TryGetValue((folder, file, size), out var baked))
        {
            var source = textures.Get(folder, file);
            var pixels = new Color[source.Width * source.Height];
            source.GetData(pixels);
            baked = textures.Create(size, size, SquareShrink.Bake(pixels, source.Width, source.Height, size));
            _shrunk.Add((folder, file, size), baked);
        }

        return baked;
    }

    private static Color[] SolidRow(Color color, int width)
    {
        var row = new Color[width];
        Array.Fill(row, color);
        return row;
    }
}
