using FontStashSharp;
using Jrpg.Core.Quests;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Menus;

/// <summary>
/// Draws the Quests page's list column (decision 390): the dark page column, the two categories as pointed tabs (the
/// shown one lit), one row per quest at the shared row height with a small diamond mark before its title (pale gold for
/// a main quest, dim for a side quest) under the shared gliding selection (decision 252), or the category's empty line.
/// </summary>
internal sealed class QuestListView
{
    /// <summary>The list column's width (placeholder): room for a title beside the detail column.</summary>
    public const int ColumnWidth = 640;

    /// <summary>The tabs' row (48 px) and its gap (12 px) under the top margin, before the rows.</summary>
    public const int TabsReserve = PointedTabsView.Height + 12;

    /// <summary>
    /// How many rows fit: the body (848 px) less the top margin and the tabs' room, in rows of 52 px, is 14 (the
    /// written arithmetic, tested).
    /// </summary>
    public const int VisibleRows = (MenuLayout.BodyHeight - PageListLayout.TopMargin - TabsReserve) / PageListLayout.RowHeight;

    // The mark (12 px) and its gap (8 px) before the title.
    private const int MarkSize = 12;
    private const int MarkGap = 8;

    /// <summary>How wide a title may draw: the column less its margins, the mark and its gap.</summary>
    public const int TitleWidth = ColumnWidth - (2 * PageListLayout.Margin) - MarkSize - MarkGap;

    private readonly UiTextures _ui;
    private readonly SpriteFontBase _font;
    private readonly PointedTabsView _tabs;
    private readonly ColumnPanelView _panel = new(UiTheme.PageColumn);

    /// <param name="ui">The shared selection textures and the tabs' shape.</param>
    /// <param name="font">The rows' font, which the titles were fitted with; the tabs' too.</param>
    /// <param name="categoryNames">The two tabs' labels, in <see cref="QuestCategory"/> order.</param>
    public QuestListView(UiTextures ui, SpriteFontBase font, IReadOnlyList<string> categoryNames)
    {
        _ui = ui;
        _font = font;
        _tabs = new PointedTabsView(ui, font, categoryNames);
    }

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the plain pixel comes from.</param>
    /// <param name="place">
    /// The shown category's rows and cursor, its name and empty line, and where the column goes.
    /// </param>
    /// <param name="opacity">The menu's fade.</param>
    public void Draw(SpriteBatch batch, TextureLibrary textures, Place place, float opacity)
    {
        var column = place.Area with { Width = ColumnWidth };
        _panel.DrawClosed(batch, textures.Pixel, MenuLayout.FullHeight(column), opacity);
        var left = column.X + PageListLayout.Margin;
        var top = column.Y + PageListLayout.TopMargin;
        var width = column.Width - (2 * PageListLayout.Margin);
        _tabs.Draw(batch, textures, new PointedTabsView.Place(new Rectangle(left, top, width, PointedTabsView.Height), (int)place.Category), opacity);
        var firstRow = new Rectangle(left, top + TabsReserve, width, PageListLayout.RowHeight);
        if (place.Cursor is not { } cursor)
        {
            DrawText(batch, place.EmptyLine, firstRow, UiTheme.TextSecondary * opacity);
            return;
        }

        _ui.DrawGlidingSelection(batch, textures, place.Selection.RowIn(firstRow), opacity);
        // The mark's color says the kind, told at a glance; the title follows it. Rows off the cursor are dimmed.
        for (var slot = 0; slot < cursor.VisibleCount; slot++)
        {
            var index = cursor.FirstVisible + slot;
            var area = firstRow with { Y = firstRow.Y + (slot * PageListLayout.RowHeight) };
            var rowOpacity = opacity * (index == cursor.Selected ? 1 : UiTheme.DimmedSelectionOpacity);
            var row = place.Rows[index];
            var mark = row.Entry.Quest.Kind == QuestKind.Main ? UiTheme.QuestMainMark : UiTheme.QuestSideMark;
            Diamond.WithDiagonal(new Vector2(area.X + (MarkSize / 2f), area.Y + (area.Height / 2f)), MarkSize).Draw(batch, textures.Pixel, mark * rowOpacity);
            DrawText(batch, row.Title, area with { X = area.X + MarkSize + MarkGap }, UiTheme.TextPrimary * rowOpacity);
        }
    }

    private void DrawText(SpriteBatch batch, string text, Rectangle row, Color color) =>
        batch.DrawString(_font, text, new Vector2(row.X, row.Y + ((row.Height - _font.LineHeight) / 2f)), color);

    /// <summary>What the column draws this frame.</summary>
    /// <param name="Rows">The shown category's quests with their fitted titles.</param>
    /// <param name="Cursor">The shown category's cursor; null while it lists no quest.</param>
    /// <param name="Selection">Where its highlight has glided to.</param>
    /// <param name="Category">The shown category, whose tab is lit.</param>
    /// <param name="EmptyLine">What the column says while the category lists no quest.</param>
    /// <param name="Area">The page area the column starts in.</param>
    public readonly record struct Place(IReadOnlyList<QuestRow> Rows, ListCursor? Cursor, SelectionMotion Selection, QuestCategory Category, string EmptyLine, Rectangle Area);
}
