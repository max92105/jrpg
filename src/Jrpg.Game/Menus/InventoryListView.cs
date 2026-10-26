using FontStashSharp;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Menus;

/// <summary>
/// Draws the Inventory page's list: the visible rows (icon, name, quantity), the selected row's glow and pointer, or a
/// centered line when the category holds nothing.
/// </summary>
internal sealed class InventoryListView
{
    /// <summary>How many rows show at once.</summary>
    public const int VisibleRows = 10;

    // Placeholder layout (decision 82): 52 px rows with a 32 px icon in a 48 px column and the quantity 16 px from the
    // right; the empty line 24 px below the top.
    private const int RowHeight = 52;
    private const int IconColumn = 48;
    private const int RowIconSize = 32;
    private const int EmptyLineTop = 24;

    /// <summary>The height of the visible rows, from the list's top.</summary>
    public const int Height = VisibleRows * RowHeight;

    /// <summary>A row's icon's gap to the list's left edge; the description under the list lines up with it.</summary>
    public const int RowInset = 10;

    /// <summary>Where the name column starts, from the list's left edge; its column title lines up with it.</summary>
    public const int NameLeft = RowInset + IconColumn;

    /// <summary>The quantity column's gap to the list's right edge; its column title lines up with it.</summary>
    public const int QuantityInset = 16;

    private readonly UiTextures _ui;
    private readonly SpriteFontBase _bodyFont;
    private readonly MeasuredText _empty;

    /// <param name="ui">The shared textures: icons, the selected row.</param>
    /// <param name="bodyFont">The page's body font, which its rows were measured with.</param>
    /// <param name="messages">The empty line's text.</param>
    public InventoryListView(UiTextures ui, SpriteFontBase bodyFont, UiMessages messages)
    {
        _ui = ui;
        _bodyFont = bodyFont;
        _empty = MeasuredText.Of(bodyFont, messages.Text(MessageIds.InventoryEmpty));
    }

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the plain pixel comes from.</param>
    /// <param name="place">The rows, the cursor on them, and where the list goes.</param>
    /// <param name="opacity">The page's fade.</param>
    public void Draw(SpriteBatch batch, TextureLibrary textures, Place place, float opacity)
    {
        var area = place.Area;
        if (place.Cursor is not { } cursor)
        {
            var emptyLeft = area.X + ((area.Width - _empty.Width) / 2f);
            batch.DrawString(_bodyFont, _empty.Text, new Vector2(emptyLeft, area.Y + EmptyLineTop), UiTheme.TextSecondary * opacity);
            return;
        }

        var first = cursor.FirstVisible;
        DrawSelection(batch, textures, new SelectedRow(RowArea(area, cursor.Selected - first), place.HasCursor), opacity);
        var last = Math.Min(place.Rows.Count, first + VisibleRows);
        for (var index = first; index < last; index++)
        {
            DrawRow(batch, textures, RowArea(area, index - first), place.Rows[index], opacity);
        }
    }

    private static Rectangle RowArea(Rectangle list, int visibleIndex) =>
        new(list.X, list.Y + (visibleIndex * RowHeight), list.Width, RowHeight);

    // The pointer marks the row while the list has the cursor; while the rail has it, the row stays chosen, dimmed.
    private void DrawSelection(SpriteBatch batch, TextureLibrary textures, SelectedRow selected, float opacity)
    {
        var glowOpacity = selected.HasCursor ? opacity : opacity * UiTheme.DimmedSelectionOpacity;
        _ui.DrawSelectedRow(batch, textures, selected.Area, glowOpacity);
        if (selected.HasCursor)
        {
            MenuPointer.Draw(batch, textures.Pixel, new Vector2(selected.Area.X, selected.Area.Center.Y), opacity);
        }
    }

    private void DrawRow(SpriteBatch batch, TextureLibrary textures, Rectangle row, InventoryRow item, float opacity)
    {
        var textColor = (item.IsUsable ? UiTheme.TextPrimary : UiTheme.TextDisabled) * opacity;
        var textTop = row.Y + ((RowHeight - _bodyFont.LineHeight) / 2f);
        var iconTop = row.Y + ((RowHeight - RowIconSize) / 2);
        batch.Draw(_ui.Icon(textures, item.IconFile, RowIconSize), new Vector2(row.X + RowInset, iconTop), Color.White * opacity);
        batch.DrawString(_bodyFont, item.Name, new Vector2(row.X + NameLeft, textTop), textColor);
        var quantityLeft = row.Right - QuantityInset - item.Quantity.Width;
        batch.DrawString(_bodyFont, item.Quantity.Text, new Vector2(quantityLeft, textTop), textColor);
    }

    /// <summary>What the list draws, and where.</summary>
    /// <param name="Rows">Every row of the category, built when it or the items change.</param>
    /// <param name="Cursor">The selected row and the first visible one; null when the category is empty.</param>
    /// <param name="HasCursor">Whether the list has the cursor rather than the category rail.</param>
    /// <param name="Area">The rows' column, from the first row's top.</param>
    public readonly record struct Place(IReadOnlyList<InventoryRow> Rows, ListCursor? Cursor, bool HasCursor, Rectangle Area);

    // The selected row's place, and whether the list has the cursor rather than the rail.
    private readonly record struct SelectedRow(Rectangle Area, bool HasCursor);
}
