using Jrpg.Game.Rendering;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Menus;

/// <summary>
/// The game menu's regions on the virtual screen, so every page lays out from the same place. The page area's left edge
/// follows the tabs' right edge, which moves as the tabs fold (decision 286).
/// </summary>
internal static class MenuLayout
{
    /// <summary>The footer bar's height, which the hint is centered in.</summary>
    public const int FooterHeight = 72;

    /// <summary>Where the tabs and the pages start: 40 px under the header (placeholder, decision 82).</summary>
    public const int BodyTop = MenuHeader.Height + 40;

    /// <summary>
    /// The widest a page showing one column gets, about the reference's Status column at 1920, so the art shows beside
    /// it (decision 285, placeholder).
    /// </summary>
    public const int SingleColumnWidth = 1100;

    // Placeholder layout (decision 82): the page 24 px right of the tabs, 40 px from the screen's right, and 32 px
    // above the footer.
    private const int TabsGap = 24;
    private const int RightMargin = 40;
    private const int FooterGap = 32;

    /// <summary>The area the open page draws in, beside the tabs.</summary>
    /// <param name="tabsRight">The tabs' right edge in virtual pixels.</param>
    public static Rectangle PageArea(int tabsRight)
    {
        var left = tabsRight + TabsGap;
        return new Rectangle(left, BodyTop, VirtualScreen.Width - RightMargin - left, VirtualScreen.Height - FooterHeight - FooterGap - BodyTop);
    }

    /// <returns><paramref name="area"/>, at most <see cref="SingleColumnWidth"/> wide, its left edge kept.</returns>
    public static Rectangle SingleColumn(Rectangle area) => area with { Width = Math.Min(area.Width, SingleColumnWidth) };
}
