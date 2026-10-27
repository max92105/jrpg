using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Menus;

/// <summary>
/// Draws the Inventory page's category rail: a darker column from the header to the footer with one icon tab per
/// category, the chosen one lit. The pointer marks it while the rail has the cursor; otherwise its band is dimmed.
/// </summary>
internal sealed class CategoryRailView(UiTextures ui)
{
    /// <summary>The rail's width in virtual pixels (placeholder).</summary>
    public const int Width = 72;

    // Placeholder layout: 12 px padding and 64 px tabs holding a 40 px icon, faint until selected; the selected tab
    // shows the shared selected-row band inside an 8 x 4 margin.
    private const int Padding = 12;
    private const int TabHeight = 64;
    private const int IconSize = 40;
    private const int BandInsetX = 8;
    private const int BandInsetY = 4;
    private const float UnselectedIconOpacity = 0.5f;

    private readonly ColumnPanelView _panel = new(UiTheme.SideColumn);

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the plain pixel comes from.</param>
    /// <param name="place">The rail to draw, and the column it fills.</param>
    /// <param name="opacity">The page's fade.</param>
    public void Draw(SpriteBatch batch, TextureLibrary textures, Place place, float opacity)
    {
        var rail = new Rectangle(place.Area.X, place.Area.Y, Width, place.Area.Height);
        _panel.Draw(batch, textures.Pixel, MenuLayout.FullHeight(rail), opacity);
        DrawSelection(batch, textures, place.Rail, new Rectangle(rail.X, rail.Y + Padding, Width, TabHeight), opacity);
        for (var index = 0; index < place.Rail.Tabs.Count; index++)
        {
            var tab = new Rectangle(rail.X, rail.Y + Padding + (index * TabHeight), Width, TabHeight);
            DrawTab(batch, textures, new TabPlace(place.Rail, tab, index), opacity);
        }
    }

    // The chosen tab's band glides between tabs (decision 262), inside an 8 x 4 margin. The pointer marks it while the
    // rail has the cursor; otherwise the band is dimmed.
    private void DrawSelection(SpriteBatch batch, TextureLibrary textures, CategoryRail rail, Rectangle firstTab, float opacity)
    {
        var selected = rail.Selection.RowIn(firstTab);
        var tab = selected.Area;
        var band = new Rectangle(tab.X + BandInsetX, tab.Y + BandInsetY, tab.Width - (2 * BandInsetX), tab.Height - (2 * BandInsetY));
        if (!rail.HasCursor)
        {
            ui.DrawSelectedRow(batch, textures, band, opacity * UiTheme.DimmedSelectionOpacity);
            return;
        }

        ui.DrawSelectedRow(batch, textures, band, opacity);
        MenuPointer.Draw(batch, textures.Pixel, selected.PointerTip, opacity);
    }

    private void DrawTab(SpriteBatch batch, TextureLibrary textures, TabPlace place, float opacity)
    {
        var (rail, tab, index) = place;
        var isSelected = index == rail.Selected;
        var icon = ui.Icon(textures, rail.Tabs[index].IconFile, IconSize);
        var corner = new Vector2(tab.Center.X - (IconSize / 2), tab.Center.Y - (IconSize / 2));
        batch.Draw(icon, corner, Color.White * (isSelected ? opacity : opacity * UnselectedIconOpacity));
    }

    /// <summary>Where the rail is drawn.</summary>
    /// <param name="Rail">Its tabs, the chosen one, and whether it has the cursor.</param>
    /// <param name="Area">The column it fills, from its left edge; only <see cref="Width"/> of it is used.</param>
    public readonly record struct Place(CategoryRail Rail, Rectangle Area);

    // One tab: which rail it is on, where, and its index.
    private readonly record struct TabPlace(CategoryRail Rail, Rectangle Tab, int Index);
}
