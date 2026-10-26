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
    // Placeholder layout (decision 82): a 28 x 20 margin, a heading title, column titles over a hairline, then the
    // rows, with the description 24 px under them.
    private const int Margin = 28;
    private const int TopMargin = 20;
    private const int HeadingPixels = 30;
    private const int LabelPixels = 18;
    private const int ColumnTitlesTop = 60;
    private const int RowsTop = ColumnTitlesTop + ColumnTitles.Height;
    private const int DescriptionGap = 24;

    // The list starts 12 px right of the category rail (placeholder).
    private const int RailGap = 12;

    private readonly GameSession _session;
    private readonly ContentCatalog _catalog;
    private readonly CategoryRail _rail = new();
    private readonly CategoryRailView _railView;
    private readonly SpriteFontBase _headingFont;
    private readonly SpriteFontBase _bodyFont;
    private readonly ColumnTitles _columnTitles;
    private readonly InventoryListView _listView;
    private readonly string[] _categoryNames;
    private readonly string _listHint;
    private readonly string _railHint;
    private readonly ItemUseDialog _itemUse;
    private ListCursor? _rowCursor;
    private InventoryRow[] _rows = [];

    // The selected row's band glides between rows (decision 262); a new category or a used item puts it in place.
    private readonly SelectionGlide _glide = new(0);

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
        _railView = new CategoryRailView(ui);
        _headingFont = text.Fonts.FontAt(HeadingPixels);
        _bodyFont = text.Fonts.FontAt(InventoryListView.BodyPixels);
        var labelFont = text.Fonts.FontAt(LabelPixels);
        _columnTitles = new ColumnTitles(
            labelFont,
            ui,
            [
                ColumnTitle.FromLeft(MeasuredText.Of(labelFont, text.Messages.Text(MessageIds.InventoryColumnName)), InventoryListView.NameLeft),
                ColumnTitle.FromRight(MeasuredText.Of(labelFont, text.Messages.Text(MessageIds.InventoryColumnQuantity)), InventoryListView.QuantityInset),
            ]);
        _listView = new InventoryListView(text, ui);
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

    /// <summary>Where the selected row's band is, in rows of the visible window, as it glides.</summary>
    internal float SelectionPosition => _glide.Position;

    public void Advance(TimeSpan delta) => _glide.Advance(delta);

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

        if (_rowCursor is not null)
        {
            _glide.FollowShown(_rowCursor);
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
        var descriptionPosition = new Vector2(left + InventoryListView.RowInset, rowsTop + InventoryListView.Height + DescriptionGap);
        if (_notice is not null)
        {
            batch.DrawString(_bodyFont, _notice, descriptionPosition, UiTheme.TextPrimary * opacity);
        }

        var list = new InventoryListView.Place(new Rectangle(left, rowsTop, width, InventoryListView.Height), _rows, _rowCursor, _glide, !_rail.HasCursor);
        _listView.Draw(batch, textures, list, opacity);
        if (_notice is null && _rowCursor is not null)
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
        _rowCursor = _rows.Length == 0 ? null : new ListCursor(_rows.Length, _ => true, InventoryListView.VisibleRows);
        _rowCursor?.Select(selectedRow);
        _glide.JumpTo(_rowCursor is null ? 0 : _rowCursor.Selected - _rowCursor.FirstVisible);
    }

    private InventoryRow RowOf(InventoryEntry entry)
    {
        var item = _catalog.Items[entry.ItemId];
        var quantity = _listView.MeasureQuantity(NumberText.Of(entry.Quantity));
        return new InventoryRow(item.Id, item.Name, item.IconFile, quantity, item.Description, _session.FieldItems.CanUse(item));
    }
}
