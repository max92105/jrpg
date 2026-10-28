using Jrpg.Core.Content;
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

    private readonly ContentCatalog _catalog;
    private readonly CategoryRail _rail = new();
    private readonly InventoryList _list;
    private readonly InventoryColumnView _columnView;
    private readonly PartySidebarView _sidebarView;
    private readonly string _listHint;
    private readonly string _railHint;
    private readonly ItemDetailLine _detail = new();

    /// <param name="text">Messages and fonts.</param>
    /// <param name="ui">The shared gradient textures.</param>
    /// <param name="session">Whose inventory is shown.</param>
    /// <param name="catalog">Item names and descriptions.</param>
    public InventoryPage(ScreenText text, UiTextures ui, GameSession session, ContentCatalog catalog)
    {
        _listHint = text.Messages.Text(MessageIds.MenuHintInventory);
        _railHint = text.Messages.Text(MessageIds.MenuHintInventoryCategories);
        _catalog = catalog;
        var bodyFont = text.Fonts.FontAt(BodyPixels);
        _list = new InventoryList(session, catalog, bodyFont);
        _columnView = new InventoryColumnView(text, ui, bodyFont, _rail.Tabs);
        Sidebar = new PartySidebar(session.Party);
        _sidebarView = new PartySidebarView(Sidebar, new MemberCardView(text, ui, catalog), text);
        ItemUse = new ItemUse(session, text.Messages);
        ShowFirstRow();
    }

    public string TitleMessageId => MessageIds.MenuInventory;

    public TabGlyph Glyph => TabGlyph.Items;

    public string Hint => ItemUse.Hint ?? (_rail.HasCursor ? _railHint : _listHint);

    /// <summary>False while choosing who to use an item on: Cancel closes that instead.</summary>
    public bool LeavesOnCancel => !ItemUse.IsOpen;

    public InventoryCategory Category => _rail.Category;

    /// <summary>Whether the cursor is on the category rail rather than on the list.</summary>
    public bool IsChoosingCategory => _rail.HasCursor;

    /// <summary>The items on show, in order.</summary>
    public IReadOnlyList<string> ShownItemIds => _list.ShownItemIds;

    /// <summary>The selected row's index; null when the category is empty.</summary>
    public int? SelectedRow => _list.SelectedRow;

    /// <summary>The last use's result, until the player moves on; null otherwise.</summary>
    public string? Notice => _detail.Notice;

    /// <summary>Whether the selected item's description shows under it; Info toggles it (decision 333).</summary>
    public bool IsDescriptionShown => _detail.IsDescriptionShown;

    /// <summary>Whether the player is choosing who to use an item on.</summary>
    public bool IsChoosingTarget => ItemUse.IsOpen;

    /// <summary>Where the list's highlight is, in rows from the top of the visible window.</summary>
    internal float SelectionPosition => _list.Selection.Position;

    /// <summary>The party's cards beside the list (decision 266).</summary>
    internal PartySidebar Sidebar { get; }

    /// <summary>The item being used, while it aims at the sidebar's cards, and the HP it would leave each.</summary>
    internal ItemUse ItemUse { get; }

    public void Open()
    {
        _detail.ClearNotice();
        _rail.Reset();
        ShowFirstRow();

        // The party may have changed on another page since the last update here.
        Sidebar.Refresh(pointed: null);
    }

    public void Advance(TimeSpan delta)
    {
        _list.Advance(delta);
        _rail.Selection.Advance(delta);
        Sidebar.Pointer.Advance(delta);
    }

    public SceneRequest? Update(InputFrame input)
    {
        UpdateInput(input);
        Sidebar.Refresh(ItemUse.Picker?.Pointed);
        return null;
    }

    public void Draw(SpriteBatch batch, TextureLibrary textures, Rectangle area, float opacity)
    {
        var listArea = area with { Width = area.Width - SidebarWidth - SidebarGap };
        var column = new InventoryColumnView.Place(_rail, _list.Rows, _list.Cursor, !_rail.HasCursor && !ItemUse.IsOpen, _list.Selection, listArea, _detail.TextFor(_list.SelectedItem));
        _columnView.Draw(batch, textures, column, opacity);
        var sidebar = area with { X = area.Right - SidebarWidth, Width = SidebarWidth };
        _sidebarView.Draw(batch, textures, new PartySidebarView.Place(sidebar, ItemUse), opacity);
    }

    private void UpdateInput(InputFrame input)
    {
        if (ItemUse.IsOpen)
        {
            if (ItemUse.Update(input) is { } result)
            {
                _detail.ShowNotice(result);
                _list.Refresh(Category);
            }

            return;
        }

        if (input.IsPressed(GameButton.Info))
        {
            _detail.ToggleDescription();
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
        if (_list.SelectedItem is not { IsUsable: true } row)
        {
            return;
        }

        if (ItemUse.UseOrPickTarget(_catalog.Items[row.ItemId]) is { } result)
        {
            _detail.ShowNotice(result);
            _list.Refresh(Category);
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

        ShowFirstRow();
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
            ShowFirstRow();
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
            _list.MoveUp();
        }

        if (input.IsPressed(GameButton.Down))
        {
            _list.MoveDown();
        }
    }

    private void ShowFirstRow() => _list.Show(Category, selectedRow: 0);
}
