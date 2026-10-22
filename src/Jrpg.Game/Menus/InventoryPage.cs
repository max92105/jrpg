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
/// The menu's Inventory page: what the party owns, one category at a time, in pickup order, with the selected item's
/// description under the list. The tab buttons change the category (question 92).
/// Read-only until items can be used (P2b).
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
    private const int ColumnTitleLineGap = 20;
    private const int RowsTop = ColumnTitlesTop + 28;
    private const int RowHeight = 44;
    private const int RowInset = 10;
    private const int IconColumn = 40;
    private const int QuantityInset = 16;
    private const int DescriptionGap = 24;
    private const int RowIconSize = 26;

    // The category rail (placeholder layout): a 72 px rail, 12 px from the list, with 12 px padding and 64 px tabs
    // holding a 40 px icon, faint until selected; the selected tab glows inside an 8 x 4 margin.
    private const int RailWidth = 72;
    private const int RailGap = 12;
    private const int RailPadding = 12;
    private const int RailTabHeight = 64;
    private const int RailIconSize = 40;
    private const int RailGlowInsetX = 8;
    private const int RailGlowInsetY = 4;
    private const float UnselectedIconOpacity = 0.5f;

    // The rail, top to bottom: each category with its heading and its icon, in one place so their order can't drift.
    private static readonly ImmutableArray<CategoryTab> _tabs =
    [
        new(InventoryCategory.All, MessageIds.InventoryAll, UiArtFiles.Icons.AllItems),
        new(InventoryCategory.Consumables, MessageIds.InventoryConsumables, UiArtFiles.Icons.Consumables),
        new(InventoryCategory.Weapons, MessageIds.InventoryWeapons, UiArtFiles.Icons.Weapons),
        new(InventoryCategory.Armor, MessageIds.InventoryArmor, UiArtFiles.Icons.Armor),
    ];

    private readonly GameSession _session;
    private readonly ContentCatalog _catalog;
    private readonly UiTextures _ui;
    private readonly ListCursor _categoryCursor = new(_tabs.Length, _ => true);
    private readonly SpriteFontBase _headingFont;
    private readonly SpriteFontBase _bodyFont;
    private readonly SpriteFontBase _labelFont;
    private readonly InventoryLabels _labels;
    private readonly string[] _categoryNames;
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
        Hint = text.Messages.Text(MessageIds.MenuHintInventory);
        _session = session;
        _catalog = catalog;
        _ui = ui;
        _headingFont = text.Fonts.FontAt(HeadingPixels);
        _bodyFont = text.Fonts.FontAt(BodyPixels);
        _labelFont = text.Fonts.FontAt(LabelPixels);
        _labels = InventoryLabels.From(text.Messages, _labelFont, _bodyFont);
        _categoryNames = [.. _tabs.Select(tab => text.Messages.Text(tab.LabelId))];
        _itemUse = new ItemUseDialog(session, text, ui, catalog);
        ShowRowsFrom(0);
    }

    public string TitleMessageId => MessageIds.MenuInventory;

    public string Hint { get; }

    public bool ShowsPartySidebar => true;

    /// <summary>False while choosing who to use an item on: Cancel closes that instead.</summary>
    public bool LeavesOnCancel => !_itemUse.IsOpen;

    public InventoryCategory Category => _tabs[_categoryCursor.Selected].Category;

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
        _categoryCursor.Select(0);
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
        if (_rowCursor is not null && _rows[_rowCursor.Selected].IsUsable)
        {
            _itemUse.Open(_catalog.Items[_rows[_rowCursor.Selected].ItemId]);
        }
    }

    private static bool MovesThroughList(InputFrame input) =>
        input.IsPressed(GameButton.PreviousTab) || input.IsPressed(GameButton.NextTab) || input.IsPressed(GameButton.Up) || input.IsPressed(GameButton.Down);

    private void MoveThroughList(InputFrame input)
    {
        if (MovesThroughList(input))
        {
            _notice = null;
        }

        if (input.IsPressed(GameButton.PreviousTab))
        {
            _categoryCursor.MoveUp();
            ShowRowsFrom(0);
            return;
        }

        if (input.IsPressed(GameButton.NextTab))
        {
            _categoryCursor.MoveDown();
            ShowRowsFrom(0);
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
        DrawRail(batch, textures, area, opacity);
        var left = area.X + RailWidth + RailGap + Margin;
        var top = area.Y + TopMargin;
        var width = area.Right - Margin - left;
        batch.DrawString(_headingFont, _categoryNames[_categoryCursor.Selected], new Vector2(left, top), UiTheme.TextPrimary * opacity);
        DrawColumnTitles(batch, textures, new Rectangle(left, top + ColumnTitlesTop, width, 0), opacity);

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
        var quantity = entry.Quantity.ToString(CultureInfo.InvariantCulture);
        return new ItemRow(item.Id, item.Name, item.IconFile, quantity, _bodyFont.MeasureString(quantity).X, item.Description, _session.FieldItems.CanUse(item));
    }

    // Section titles over the rows' columns: Name after the icon column, Qty at the right.
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
            _ui.DrawSelectedRow(batch, textures, row, opacity);
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

    // A dark strip with hairlines left and right, one icon tab per category.
    private void DrawRail(SpriteBatch batch, TextureLibrary textures, Rectangle area, float opacity)
    {
        var rail = new Rectangle(area.X, area.Y, RailWidth, area.Height);
        batch.Draw(textures.Pixel, rail, UiTheme.CategoryRail * opacity);
        batch.Draw(textures.Pixel, new Rectangle(rail.X, rail.Y, 1, rail.Height), UiTheme.Stroke * opacity);
        batch.Draw(textures.Pixel, new Rectangle(rail.Right - 1, rail.Y, 1, rail.Height), UiTheme.Stroke * opacity);
        for (var index = 0; index < _tabs.Length; index++)
        {
            var tab = new Rectangle(rail.X, rail.Y + RailPadding + (index * RailTabHeight), RailWidth, RailTabHeight);
            DrawRailTab(batch, textures, tab, index, opacity);
        }
    }

    private void DrawRailTab(SpriteBatch batch, TextureLibrary textures, Rectangle tab, int index, float opacity)
    {
        var isSelected = index == _categoryCursor.Selected;
        if (isSelected)
        {
            var glow = new Rectangle(tab.X + RailGlowInsetX, tab.Y + RailGlowInsetY, tab.Width - (2 * RailGlowInsetX), tab.Height - (2 * RailGlowInsetY));
            batch.Draw(_ui.Gradient(textures, UiTheme.SelectedRow), glow, Color.White * opacity);
            Outline.Draw(batch, textures.Pixel, glow, UiTheme.SelectionStroke * opacity);
        }

        var icon = _ui.Icon(textures, _tabs[index].IconFile, RailIconSize);
        var corner = new Vector2(tab.Center.X - (RailIconSize / 2), tab.Center.Y - (RailIconSize / 2));
        batch.Draw(icon, corner, Color.White * (isSelected ? opacity : opacity * UnselectedIconOpacity));
    }

    // One category on the rail.
    private sealed record CategoryTab(InventoryCategory Category, string LabelId, string IconFile);

    // One owned item's row text.
    private sealed record ItemRow(string ItemId, string Name, string IconFile, string Quantity, float QuantityWidth, string Description, bool IsUsable);

    // The page's fixed text.
    private sealed record InventoryLabels(string Name, string Quantity, float QuantityWidth, string Empty, float EmptyWidth)
    {
        public static InventoryLabels From(UiMessages messages, SpriteFontBase labelFont, SpriteFontBase bodyFont)
        {
            var quantity = messages.Text(MessageIds.InventoryColumnQuantity);
            var empty = messages.Text(MessageIds.InventoryEmpty);
            return new InventoryLabels(
                messages.Text(MessageIds.InventoryColumnName),
                quantity,
                labelFont.MeasureString(quantity).X,
                empty,
                bodyFont.MeasureString(empty).X);
        }
    }
}
