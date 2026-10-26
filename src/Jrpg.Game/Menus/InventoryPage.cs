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
/// description under the list and the party's cards at its right (decision 266). Left puts the cursor on the category
/// rail, where Up and Down choose the category (question 152); the tab buttons change it from either side too (question
/// 92). Confirm on a usable item uses it.
/// </summary>
internal sealed class InventoryPage : IMenuPage
{
    // 26 px body text (placeholder, decision 82).
    private const int BodyPixels = 26;

    // The party sidebar (question 145 placeholder): a 520 px column at the page's right, 96 px from the list so the map
    // shows between them, as in the reference (decision 269).
    private const int SidebarWidth = 520;
    private const int SidebarGap = 96;

    private readonly GameSession _session;
    private readonly ContentCatalog _catalog;
    private readonly CategoryRail _rail = new();
    private readonly InventoryColumnView _columnView;
    private readonly PartySidebarView _sidebarView;
    private readonly SpriteFontBase _bodyFont;
    private readonly string _listHint;
    private readonly string _railHint;
    private readonly ItemUseDialog _itemUse;
    private readonly SelectionMotion _selection = new();
    private ListCursor? _rowCursor;
    private InventoryRow[] _rows = [];

    private readonly ItemDetailLine _detail = new();

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
        _bodyFont = text.Fonts.FontAt(BodyPixels);
        _columnView = new InventoryColumnView(text, ui, _bodyFont, _rail.Tabs);
        Sidebar = new PartySidebar(session.Party);
        _sidebarView = new PartySidebarView(Sidebar, new MemberCardView(text, ui, catalog), text, ui);
        _itemUse = new ItemUseDialog(session, text, ui, catalog);
        ShowRowsFrom(0);
    }

    public string TitleMessageId => MessageIds.MenuInventory;

    public TabGlyph Glyph => TabGlyph.Items;

    public string Hint => _rail.HasCursor ? _railHint : _listHint;

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
    public string? Notice => _detail.Notice;

    /// <summary>Whether the player is choosing who to use an item on.</summary>
    public bool IsChoosingTarget => _itemUse.IsOpen;

    /// <summary>Where the list's highlight is, in rows from the top of the visible window.</summary>
    internal float SelectionPosition => _selection.Position;

    /// <summary>The party's cards beside the list (decision 266).</summary>
    internal PartySidebar Sidebar { get; }

    public void Open()
    {
        _detail.ClearNotice();
        _rail.Reset();
        ShowRowsFrom(0);

        // The party may have changed on another page since the last update here.
        Sidebar.Refresh(pointed: null);
    }

    public void Advance(TimeSpan delta) => _selection.Advance(delta);

    public SceneRequest? Update(InputFrame input)
    {
        UpdateInput(input);
        Sidebar.Refresh(pointed: null);
        return null;
    }

    public void Draw(SpriteBatch batch, TextureLibrary textures, Rectangle area, float opacity)
    {
        var listArea = area with { Width = area.Width - SidebarWidth - SidebarGap };
        var column = new InventoryColumnView.Place(_rail, _rows, _rowCursor, !_rail.HasCursor, _selection, listArea, DetailText());
        _columnView.Draw(batch, textures, column, opacity);
        var sidebar = area with { X = area.Right - SidebarWidth, Width = SidebarWidth };
        _sidebarView.Draw(batch, textures, new PartySidebarView.Place(sidebar, PointsAtShown: true), opacity);
        _itemUse.Draw(batch, textures, opacity);
    }

    private void UpdateInput(InputFrame input)
    {
        if (_itemUse.IsOpen)
        {
            if (_itemUse.Update(input) is { } result)
            {
                _detail.ShowNotice(result);
                ShowRowsFrom(_rowCursor?.Selected ?? 0);
            }

            return;
        }

        if (MovesACursor(input))
        {
            _detail.ClearNotice();
        }

        // The rail answers first, so Confirm there only goes back to the list and never uses an item.
        if (_rail.HasCursor)
        {
            MoveOnRail(input);
            return;
        }

        if (input.IsPressed(GameButton.Confirm))
        {
            OpenItemUse();
            return;
        }

        MoveThroughList(input);
    }

    private void OpenItemUse()
    {
        if (_rowCursor is null || !_rows[_rowCursor.Selected].IsUsable)
        {
            return;
        }

        if (_itemUse.UseOrPickTarget(_catalog.Items[_rows[_rowCursor.Selected].ItemId]) is { } result)
        {
            _detail.ShowNotice(result);
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

        if (_rowCursor is not { } cursor)
        {
            return;
        }

        if (input.IsPressed(GameButton.Up))
        {
            cursor.MoveUp();
        }

        if (input.IsPressed(GameButton.Down))
        {
            cursor.MoveDown();
        }

        _selection.Follow(cursor);
    }

    private string? DetailText() => _detail.TextFor(_rowCursor is null ? null : _rows[_rowCursor.Selected]);

    // Built when the category or the items change, so drawing formats nothing. The cursor stays on the same row, or the
    // last one once the list got shorter, and the highlight jumps there.
    private void ShowRowsFrom(int selectedRow)
    {
        var shown = InventoryFilter.Select(Category, _session.Inventory.Entries, _catalog.Items);
        _rows = [.. shown.Select(RowOf)];
        if (_rows.Length == 0)
        {
            _rowCursor = null;
            return;
        }

        _rowCursor = new ListCursor(_rows.Length, _ => true, InventoryListView.VisibleRows);
        _rowCursor.Select(selectedRow);
        _selection.JumpTo(_rowCursor);
    }

    private InventoryRow RowOf(InventoryEntry entry)
    {
        var item = _catalog.Items[entry.ItemId];
        var quantity = MeasuredText.Of(_bodyFont, NumberText.Of(entry.Quantity));
        return new InventoryRow(item.Id, item.Name, item.IconFile, quantity, item.Description, _session.FieldItems.CanUse(item));
    }
}
