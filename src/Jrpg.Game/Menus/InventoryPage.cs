using System.Collections.Immutable;
using System.Globalization;
using FontStashSharp;
using Jrpg.Core.Content;
using Jrpg.Core.Items;
using Jrpg.Core.Session;
using Jrpg.Game.Input;
using Jrpg.Game.Rendering;
using Jrpg.Game.Scenes;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Menus;

/// <summary>
/// The menu's Inventory page (legacy <c>InventoryView</c>): what the party owns, one category at a time, in pickup order,
/// with the selected item's description under the list. The tab buttons change the category (question 92). Read-only
/// until items can be used (P2b).
/// </summary>
internal sealed class InventoryPage : IMenuPage
{
    /// <summary>How many 44 px rows show at once.</summary>
    public const int VisibleRows = 10;

    // Legacy InventoryView and ListRow: a 28 x 20 margin, the Text.Heading title, column titles over a hairline, 44 px rows
    // 10 px in with a 40 px icon column (icons come in P1-2) and the quantity 16 px from the right; Text.Body 18 px.
    private const int Margin = 28;
    private const int TopMargin = 20;
    private const int HeadingPixels = 22;
    private const int BodyPixels = 18;
    private const int LabelPixels = 14;
    private const int ColumnTitlesTop = 48;
    private const int ColumnTitleLineGap = 20;
    private const int RowsTop = ColumnTitlesTop + 28;
    private const int RowHeight = 44;
    private const int RowInset = 10;
    private const int IconColumn = 40;
    private const int QuantityInset = 16;
    private const int DescriptionGap = 24;

    private static readonly ImmutableArray<InventoryCategory> _categories =
        [InventoryCategory.All, InventoryCategory.Consumables, InventoryCategory.Weapons, InventoryCategory.Armor];

    private readonly GameSession _session;
    private readonly ContentCatalog _catalog;
    private readonly UiTextures _ui;
    private readonly ListCursor _categoryCursor = new(_categories.Length, _ => true);
    private readonly SpriteFontBase _headingFont;
    private readonly SpriteFontBase _bodyFont;
    private readonly SpriteFontBase _labelFont;
    private readonly InventoryLabels _labels;
    private ListCursor? _rowCursor;
    private ItemRow[] _rows = [];

    /// <param name="text">Messages and fonts.</param>
    /// <param name="ui">The shared gradient textures.</param>
    /// <param name="session">Whose inventory is shown.</param>
    /// <param name="catalog">Item names and descriptions.</param>
    public InventoryPage(ScreenText text, UiTextures ui, GameSession session, ContentCatalog catalog)
    {
        _session = session;
        _catalog = catalog;
        _ui = ui;
        _headingFont = text.Fonts.FontAt(HeadingPixels);
        _bodyFont = text.Fonts.FontAt(BodyPixels);
        _labelFont = text.Fonts.FontAt(LabelPixels);
        _labels = InventoryLabels.From(text.Messages, _labelFont, _bodyFont);
        ShowCategory();
    }

    public string TitleMessageId => MessageIds.MenuInventory;

    public bool LeavesOnCancel => true;

    public InventoryCategory Category => _categories[_categoryCursor.Selected];

    /// <summary>The items on show, in order.</summary>
    public IReadOnlyList<string> ShownItemIds => [.. _rows.Select(row => row.ItemId)];

    /// <summary>The selected row's index; null when the category is empty.</summary>
    public int? SelectedRow => _rowCursor?.Selected;

    public SceneRequest? Update(InputFrame input)
    {
        if (input.IsPressed(GameButton.PreviousTab))
        {
            _categoryCursor.MoveUp();
            ShowCategory();
            return null;
        }

        if (input.IsPressed(GameButton.NextTab))
        {
            _categoryCursor.MoveDown();
            ShowCategory();
            return null;
        }

        if (input.IsPressed(GameButton.Up))
        {
            _rowCursor?.MoveUp();
        }

        if (input.IsPressed(GameButton.Down))
        {
            _rowCursor?.MoveDown();
        }

        return null;
    }

    public void Draw(SpriteBatch batch, TextureLibrary textures, Rectangle area, float opacity)
    {
        var left = area.X + Margin;
        var top = area.Y + TopMargin;
        var width = area.Right - Margin - left;
        batch.DrawString(_headingFont, _labels.Categories[_categoryCursor.Selected], new Vector2(left, top), UiTheme.TextPrimary * opacity);
        DrawColumnTitles(batch, textures, new Rectangle(left, top + ColumnTitlesTop, width, 0), opacity);

        var rowsTop = top + RowsTop;
        if (_rowCursor is null)
        {
            var emptyLeft = left + ((width - _labels.EmptyWidth) / 2f);
            batch.DrawString(_bodyFont, _labels.Empty, new Vector2(emptyLeft, rowsTop + DescriptionGap), UiTheme.TextSecondary * opacity);
            return;
        }

        var last = Math.Min(_rows.Length, _rowCursor.FirstVisible + VisibleRows);
        for (var index = _rowCursor.FirstVisible; index < last; index++)
        {
            var row = new Rectangle(left, rowsTop + ((index - _rowCursor.FirstVisible) * RowHeight), width, RowHeight);
            DrawRow(batch, textures, row, index, opacity);
        }

        var descriptionTop = rowsTop + (VisibleRows * RowHeight) + DescriptionGap;
        batch.DrawString(_bodyFont, _rows[_rowCursor.Selected].Description, new Vector2(left + RowInset, descriptionTop), UiTheme.TextSecondary * opacity);
    }

    // Built when the category changes, so drawing formats nothing; the cursor goes back to the first row.
    private void ShowCategory()
    {
        var shown = InventoryFilter.Select(Category, _session.Inventory.Entries, _catalog.Items);
        _rows = [.. shown.Select(RowOf)];
        _rowCursor = _rows.Length == 0 ? null : new ListCursor(_rows.Length, _ => true, VisibleRows);
    }

    private ItemRow RowOf(InventoryEntry entry)
    {
        var item = _catalog.Items[entry.ItemId];
        var quantity = entry.Quantity.ToString(CultureInfo.InvariantCulture);
        return new ItemRow(item.Id, item.Name, quantity, _bodyFont.MeasureString(quantity).X, item.Description);
    }

    // Legacy SectionHeaderStyle over the rows' columns: Name after the icon column, Qty at the right.
    private void DrawColumnTitles(SpriteBatch batch, TextureLibrary textures, Rectangle section, float opacity)
    {
        var color = UiTheme.TextSecondary * opacity;
        batch.DrawString(_labelFont, _labels.Name, new Vector2(section.X + RowInset + IconColumn, section.Y), color);
        var quantityLeft = section.Right - QuantityInset - _labels.QuantityWidth;
        batch.DrawString(_labelFont, _labels.Quantity, new Vector2(quantityLeft, section.Y), color);
        var line = new Rectangle(section.X, section.Y + ColumnTitleLineGap, section.Width, 1);
        batch.Draw(_ui.SectionLine(textures), line, Color.White * opacity);
    }

    private void DrawRow(SpriteBatch batch, TextureLibrary textures, Rectangle row, int index, float opacity)
    {
        if (index == _rowCursor?.Selected)
        {
            batch.Draw(_ui.Gradient(textures, UiTheme.SelectedRow), row, Color.White * opacity);
            var line = _ui.Gradient(textures, UiTheme.HighlightLine);
            batch.Draw(line, new Rectangle(row.X, row.Y, row.Width, 1), Color.White * opacity);
            batch.Draw(line, new Rectangle(row.X, row.Bottom - 1, row.Width, 1), Color.White * opacity);
        }

        var item = _rows[index];
        var textTop = row.Y + ((RowHeight - _bodyFont.LineHeight) / 2f);
        batch.DrawString(_bodyFont, item.Name, new Vector2(row.X + RowInset + IconColumn, textTop), UiTheme.TextPrimary * opacity);
        var quantityLeft = row.Right - QuantityInset - item.QuantityWidth;
        batch.DrawString(_bodyFont, item.Quantity, new Vector2(quantityLeft, textTop), UiTheme.TextPrimary * opacity);
    }

    // One owned item's row text.
    private sealed record ItemRow(string ItemId, string Name, string Quantity, float QuantityWidth, string Description);

    // The page's fixed text; Categories is in rail order.
    private sealed record InventoryLabels(string[] Categories, string Name, string Quantity, float QuantityWidth, string Empty, float EmptyWidth)
    {
        public static InventoryLabels From(UiMessages messages, SpriteFontBase labelFont, SpriteFontBase bodyFont)
        {
            var quantity = messages.Text(MessageIds.InventoryColumnQuantity);
            var empty = messages.Text(MessageIds.InventoryEmpty);
            return new InventoryLabels(
                [
                    messages.Text(MessageIds.InventoryAll),
                    messages.Text(MessageIds.InventoryConsumables),
                    messages.Text(MessageIds.InventoryWeapons),
                    messages.Text(MessageIds.InventoryArmor),
                ],
                messages.Text(MessageIds.InventoryColumnName),
                quantity,
                labelFont.MeasureString(quantity).X,
                empty,
                bodyFont.MeasureString(empty).X);
        }
    }
}
