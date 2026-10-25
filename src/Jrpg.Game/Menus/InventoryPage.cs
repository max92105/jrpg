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
/// The menu's Inventory page: what the party owns, one category at a time, in pickup order, with the selected item's
/// description under the list. Left puts the cursor on the category rail, where Up and Down choose the category
/// (question 152); the tab buttons change it from either side (question 92). Confirm on a usable item uses it.
/// </summary>
internal sealed class InventoryPage : IMenuPage
{
    /// <summary>How many 44 px rows show at once.</summary>
    public const int VisibleRows = 10;

    // Placeholder layout (decision 82): a 28 x 20 margin, a heading title, column titles over a hairline, 44 px rows
    // 10 px in with a 26 px icon in a 40 px column and the quantity 16 px from the right; 18 px body text.
    private const int Margin = 28;
    private const int TopMargin = 20;
    private const int HeadingPixels = 22;
    private const int BodyPixels = 18;
    private const int LabelPixels = 14;
    private const int ColumnTitlesTop = 48;
    private const int RowsTop = ColumnTitlesTop + ColumnTitles.Height;
    private const int RowHeight = 44;
    private const int RowInset = 10;
    private const int IconColumn = 40;
    private const int QuantityInset = 16;
    private const int DescriptionGap = 24;
    private const int RowIconSize = 26;

    // The list starts 12 px right of the category rail (placeholder).
    private const int RailGap = 12;

    private readonly GameSession _session;
    private readonly ContentCatalog _catalog;
    private readonly UiTextures _ui;
    private readonly CategoryRail _rail = new();
    private readonly CategoryRailView _railView;
    private readonly SpriteFontBase _headingFont;
    private readonly SpriteFontBase _bodyFont;
    private readonly ColumnTitles _columnTitles;
    private readonly InventoryLabels _labels;
    private readonly string[] _categoryNames;
    private readonly string _listHint;
    private readonly string _railHint;
    private readonly ItemUseDialog _itemUse;
    private ListCursor? _rowCursor;
    private ItemRow[] _rows = [];

    // What the last item used did; shown in the description's place until the player moves on.
    private string? _notice;

    /// <param name="text">Messages and fonts.</param>
    /// <param name="ui">The shared gradient textures.</param>
    /// <param name="session">Whose inventory is shown.</param>
    /// <param name="catalog">Item names and descriptions.</param>
    public InventoryPage(ScreenText text, UiTextures ui, GameSession session, ContentCatalog catalog)
    {
        _listHint = text.Messages.Text(MessageIds.MenuHintInventory);
        _railHint = text.Messages.Text(MessageIds.MenuHintInventoryCategories);
        _session = session;
        _catalog = catalog;
        _ui = ui;
        _railView = new CategoryRailView(ui);
        _headingFont = text.Fonts.FontAt(HeadingPixels);
        _bodyFont = text.Fonts.FontAt(BodyPixels);
        var labelFont = text.Fonts.FontAt(LabelPixels);
        _columnTitles = new ColumnTitles(
            labelFont,
            ui,
            [
                ColumnTitle.FromLeft(MeasuredText.Of(labelFont, text.Messages.Text(MessageIds.InventoryColumnName)), RowInset + IconColumn),
                ColumnTitle.FromRight(MeasuredText.Of(labelFont, text.Messages.Text(MessageIds.InventoryColumnQuantity)), QuantityInset),
            ]);
        _labels = InventoryLabels.From(text.Messages, _bodyFont);
        _categoryNames = [.. _rail.Tabs.Select(tab => text.Messages.Text(tab.LabelId))];
        _itemUse = new ItemUseDialog(session, text, ui, catalog);
        ShowRowsFrom(0);
    }

    public string TitleMessageId => MessageIds.MenuInventory;

    public string Hint => _rail.HasCursor ? _railHint : _listHint;

    public bool ShowsPartySidebar => true;

    /// <summary>False while choosing who to use an item on: Cancel closes that instead.</summary>
    public bool LeavesOnCancel => !_itemUse.IsOpen;

    public InventoryCategory Category => _rail.Category;

    /// <summary>Whether the cursor is on the category rail rather than on the list.</summary>
    public bool IsChoosingCategory => _rail.HasCursor;

    /// <summary>The items on show, in order.</summary>
    public IReadOnlyList<string> ShownItemIds => [.. _rows.Select(row => row.ItemId)];

    /// <summary>The selected row's index; null when the category is empty.</summary>
    public int? SelectedRow => _rowCursor?.Selected;

    /// <summary>The last use's result, until the player moves on; null otherwise.</summary>
    public string? Notice => _notice;

    /// <summary>Whether the player is choosing who to use an item on.</summary>
    public bool IsChoosingTarget => _itemUse.IsOpen;

    public void Open()
    {
        _notice = null;
        _rail.Reset();
        ShowRowsFrom(0);
    }

    public SceneRequest? Update(InputFrame input)
    {
        if (_itemUse.IsOpen)
        {
            if (_itemUse.Update(input) is { } result)
            {
                _notice = result;
                ShowRowsFrom(_rowCursor?.Selected ?? 0);
            }

            return null;
        }

        if (MovesACursor(input))
        {
            _notice = null;
        }

        // The rail answers first, so Confirm there only goes back to the list and never uses an item.
        if (_rail.HasCursor)
        {
            MoveOnRail(input);
            return null;
        }

        if (input.IsPressed(GameButton.Confirm))
        {
            OpenItemUse();
            return null;
        }

        MoveThroughList(input);
        return null;
    }

    public void Draw(SpriteBatch batch, TextureLibrary textures, Rectangle area, float opacity)
    {
        DrawList(batch, textures, area, opacity);
        _itemUse.Draw(batch, textures, opacity);
    }

    private void OpenItemUse()
    {
        if (_rowCursor is null || !_rows[_rowCursor.Selected].IsUsable)
        {
            return;
        }

        if (_itemUse.UseOrPickTarget(_catalog.Items[_rows[_rowCursor.Selected].ItemId]) is { } result)
        {
            _notice = result;
            ShowRowsFrom(_rowCursor.Selected);
        }
    }

    // The tab buttons change the category from either side; on the rail, Up and Down do too.
    private bool TryChangeCategory(InputFrame input)
    {
        if (input.IsPressed(GameButton.PreviousTab) || (_rail.HasCursor && input.IsPressed(GameButton.Up)))
        {
            _rail.MoveUp();
        }
        else if (input.IsPressed(GameButton.NextTab) || (_rail.HasCursor && input.IsPressed(GameButton.Down)))
        {
            _rail.MoveDown();
        }
        else
        {
            return false;
        }

        ShowRowsFrom(0);
        return true;
    }

    private static bool MovesACursor(InputFrame input) =>
        input.IsPressed(GameButton.PreviousTab) || input.IsPressed(GameButton.NextTab) || input.IsPressed(GameButton.Up) || input.IsPressed(GameButton.Down);

    // Up and Down move the category on the rail; Right or Confirm go back to the list's first row.
    private void MoveOnRail(InputFrame input)
    {
        if (TryChangeCategory(input))
        {
            return;
        }

        if (input.IsPressed(GameButton.Right) || input.IsPressed(GameButton.Confirm))
        {
            _rail.ReturnCursor();
            ShowRowsFrom(0);
        }
    }

    private void MoveThroughList(InputFrame input)
    {
        if (TryChangeCategory(input))
        {
            return;
        }

        if (input.IsPressed(GameButton.Left))
        {
            _rail.TakeCursor();
            return;
        }

        if (input.IsPressed(GameButton.Up))
        {
            _rowCursor?.MoveUp();
        }

        if (input.IsPressed(GameButton.Down))
        {
            _rowCursor?.MoveDown();
        }
    }

    private void DrawList(SpriteBatch batch, TextureLibrary textures, Rectangle area, float opacity)
    {
        _railView.Draw(batch, textures, new CategoryRailView.Place(_rail, area), opacity);
        var left = area.X + CategoryRailView.Width + RailGap + Margin;
        var top = area.Y + TopMargin;
        var width = area.Right - Margin - left;
        batch.DrawString(_headingFont, _categoryNames[_rail.Selected], new Vector2(left, top), UiTheme.TextPrimary * opacity);
        _columnTitles.Draw(batch, textures, new Rectangle(left, top + ColumnTitlesTop, width, 0), opacity);

        var rowsTop = top + RowsTop;
        var descriptionPosition = new Vector2(left + RowInset, rowsTop + (VisibleRows * RowHeight) + DescriptionGap);
        if (_notice is not null)
        {
            batch.DrawString(_bodyFont, _notice, descriptionPosition, UiTheme.TextPrimary * opacity);
        }

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

        if (_notice is null)
        {
            batch.DrawString(_bodyFont, _rows[_rowCursor.Selected].Description, descriptionPosition, UiTheme.TextSecondary * opacity);
        }
    }

    // Built when the category or the items change, so drawing formats nothing. The cursor stays on the same row, or the
    // last one once the list got shorter.
    private void ShowRowsFrom(int selectedRow)
    {
        var shown = InventoryFilter.Select(Category, _session.Inventory.Entries, _catalog.Items);
        _rows = [.. shown.Select(RowOf)];
        _rowCursor = _rows.Length == 0 ? null : new ListCursor(_rows.Length, _ => true, VisibleRows);
        _rowCursor?.Select(selectedRow);
    }

    private ItemRow RowOf(InventoryEntry entry)
    {
        var item = _catalog.Items[entry.ItemId];
        var quantity = NumberText.Of(entry.Quantity);
        return new ItemRow(item.Id, item.Name, item.IconFile, quantity, _bodyFont.MeasureString(quantity).X, item.Description, _session.FieldItems.CanUse(item));
    }

    private void DrawRow(SpriteBatch batch, TextureLibrary textures, Rectangle row, int index, float opacity)
    {
        // The pointer marks the row while the list has the cursor; while the rail has it, the row stays chosen, dimmed.
        if (index == _rowCursor?.Selected && _rail.HasCursor)
        {
            _ui.DrawSelectedRow(batch, textures, row, opacity * UiTheme.DimmedSelectionOpacity);
        }
        else if (index == _rowCursor?.Selected)
        {
            _ui.DrawSelectedRow(batch, textures, row, opacity);
            MenuPointer.Draw(batch, textures.Pixel, new Vector2(row.X, row.Center.Y), opacity);
        }

        var item = _rows[index];
        var textColor = (item.IsUsable ? UiTheme.TextPrimary : UiTheme.TextDisabled) * opacity;
        var textTop = row.Y + ((RowHeight - _bodyFont.LineHeight) / 2f);
        var iconTop = row.Y + ((RowHeight - RowIconSize) / 2);
        batch.Draw(_ui.Icon(textures, item.IconFile, RowIconSize), new Vector2(row.X + RowInset, iconTop), Color.White * opacity);
        batch.DrawString(_bodyFont, item.Name, new Vector2(row.X + RowInset + IconColumn, textTop), textColor);
        var quantityLeft = row.Right - QuantityInset - item.QuantityWidth;
        batch.DrawString(_bodyFont, item.Quantity, new Vector2(quantityLeft, textTop), textColor);
    }

    // One owned item's row text.
    private sealed record ItemRow(string ItemId, string Name, string IconFile, string Quantity, float QuantityWidth, string Description, bool IsUsable);

    // The page's fixed text.
    private sealed record InventoryLabels(string Empty, float EmptyWidth)
    {
        public static InventoryLabels From(UiMessages messages, SpriteFontBase bodyFont)
        {
            var empty = messages.Text(MessageIds.InventoryEmpty);
            return new InventoryLabels(empty, bodyFont.MeasureString(empty).X);
        }
    }
}
