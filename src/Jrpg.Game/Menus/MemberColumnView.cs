using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Menus;

/// <summary>
/// Draws the member column beside a member page (decision 255): a small diamond portrait per member in view, the shown
/// one in the shared selection look (decision 252), and a small mark above or below when members are out of view.
/// </summary>
internal sealed class MemberColumnView(UiTextures ui)
{
    /// <summary>The column's width in virtual pixels; the member page starts after it (placeholder).</summary>
    public const int Width = 80;

    // Placeholder layout: 56 px portraits 72 px apart, the shown one's band 64 px tall; 10 px scroll marks (the
    // sidebar's size) 8 px past the first and last portraits.
    private const int PortraitSize = 56;
    private const int Pitch = 72;
    private const int BandHeight = 64;
    private const int ScrollMarkDiagonal = 10;
    private const int ScrollMarkGap = 8;

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the portraits and the band come from.</param>
    /// <param name="place">The column, and where it goes.</param>
    /// <param name="opacity">The page's fade.</param>
    public void Draw(SpriteBatch batch, TextureLibrary textures, Place place, float opacity)
    {
        var (column, area, pointsAtShown) = place;
        var first = column.FirstVisible;
        var last = Math.Min(column.Members.Count, first + MemberColumn.VisibleCount);
        var shownCenter = CenterOf(area, column.ShownIndex - first);
        var band = new Rectangle(area.X, (int)shownCenter.Y - (BandHeight / 2), area.Width, BandHeight);
        ui.DrawSelectedRow(batch, textures, band, opacity);
        if (pointsAtShown)
        {
            MenuPointer.Draw(batch, textures.Pixel, new Vector2(band.X, band.Center.Y), opacity);
        }

        for (var index = first; index < last; index++)
        {
            var portrait = ui.DiamondPortrait(textures, column.Members[index].Definition.PortraitFile, PortraitSize);
            var center = CenterOf(area, index - first);
            batch.Draw(portrait, center - new Vector2(PortraitSize / 2f), Color.White * opacity);
        }

        DrawScrollMarks(batch, textures.Pixel, place, opacity);
    }

    private static Vector2 CenterOf(Rectangle area, int visibleRow) =>
        new(area.Center.X, area.Y + (Pitch / 2f) + (visibleRow * Pitch));

    // A mark above the first portrait when members are hidden above, one below the last when some are hidden below.
    private static void DrawScrollMarks(SpriteBatch batch, Texture2D pixel, Place place, float opacity)
    {
        var (column, area, _) = place;
        var color = UiTheme.TextSecondary * opacity;
        var reach = (PortraitSize / 2f) + ScrollMarkGap;
        if (column.FirstVisible > 0)
        {
            Diamond.WithDiagonal(CenterOf(area, 0) - new Vector2(0, reach), ScrollMarkDiagonal).Draw(batch, pixel, color);
        }

        if (column.FirstVisible + MemberColumn.VisibleCount < column.Members.Count)
        {
            Diamond.WithDiagonal(CenterOf(area, MemberColumn.VisibleCount - 1) + new Vector2(0, reach), ScrollMarkDiagonal).Draw(batch, pixel, color);
        }
    }

    /// <summary>What the column draws, and where.</summary>
    /// <param name="Column">The members, the shown one and the scroll.</param>
    /// <param name="Area">The column's strip, from the page area's top.</param>
    /// <param name="PointsAtShown">
    /// False on a page that points at a row of its own: the shown member keeps the band, and the page keeps the only
    /// pointer (decision 252).
    /// </param>
    public readonly record struct Place(MemberColumn Column, Rectangle Area, bool PointsAtShown);
}
