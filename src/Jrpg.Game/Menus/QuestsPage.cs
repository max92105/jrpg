using FontStashSharp;
using Jrpg.Core.Session;
using Jrpg.Game.Input;
using Jrpg.Game.Rendering;
using Jrpg.Game.Scenes;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Menus;

/// <summary>
/// The menu's Quests page, its first tab (decision 393): the active quests' titles in the journal's order (decisions
/// 390, 391) under the shared gliding selection, or a line saying there are none; beside them the chosen quest's detail
/// (decision 392). Read from Core when the page opens and when the choice moves, so drawing formats nothing.
/// </summary>
internal sealed class QuestsPage : IMenuPage
{
    /// <summary>How many quests the list shows at once (placeholder, until L1's view owns the layout).</summary>
    public const int VisibleRows = 12;

    // The list takes the left half of the page, the detail the rest; plain lines until L1 to L3 (placeholders).
    private const int ListWidth = 720;
    private const int DetailGap = 40;

    private readonly GameSession _session;
    private readonly UiMessages _messages;
    private readonly UiTextures _ui;
    private readonly SpriteFontBase _font;
    private readonly string _none;
    private readonly ColumnPanelView _panel = new(UiTheme.PageColumn);
    private readonly QuestList _list = new();
    private QuestDetail? _detail;

    /// <param name="text">Messages and fonts for the titles and the lines.</param>
    /// <param name="ui">The shared selection textures.</param>
    /// <param name="session">Whose journal the page lists.</param>
    public QuestsPage(ScreenText text, UiTextures ui, GameSession session)
    {
        _session = session;
        _messages = text.Messages;
        _ui = ui;
        _font = text.Fonts.FontAt(UiTheme.MenuTextPixels);
        _none = text.Messages.Text(MessageIds.QuestsNone);
        Hint = text.Messages.Text(MessageIds.MenuHintQuests);
    }

    public string TitleMessageId => MessageIds.MenuQuests;

    public TabGlyph Glyph => TabGlyph.Quests;

    public string Hint { get; }

    public bool LeavesOnCancel => true;

    /// <summary>The titles listed, in the journal's order; empty when the none line shows.</summary>
    internal IReadOnlyList<string> ShownTitles => [.. _list.Entries.Select(entry => entry.Quest.Title)];

    /// <summary>The chosen quest's detail; null while no quest is listed.</summary>
    internal QuestDetail? Detail => _detail;

    public void Open()
    {
        _list.Show(_session.Quests.Active);
        RefreshDetail();
    }

    public void Advance(TimeSpan delta) => _list.Advance(delta);

    public SceneRequest? Update(InputFrame input)
    {
        if (input.IsPressed(GameButton.Up))
        {
            _list.MoveUp();
            RefreshDetail();
        }

        if (input.IsPressed(GameButton.Down))
        {
            _list.MoveDown();
            RefreshDetail();
        }

        return null;
    }

    public void Draw(SpriteBatch batch, TextureLibrary textures, Rectangle area, float opacity)
    {
        var column = MenuLayout.SingleColumn(area);
        _panel.DrawClosed(batch, textures.Pixel, MenuLayout.FullHeight(column), opacity);
        var list = column with { Width = ListWidth };
        if (_list.Cursor is not { } cursor)
        {
            DrawLine(batch, _none, RowAt(list, 0), UiTheme.TextSecondary * opacity);
            return;
        }

        _ui.DrawGlidingSelection(batch, textures, _list.Selection.RowIn(RowAt(list, 0)), opacity);
        for (var row = 0; row < cursor.VisibleCount; row++)
        {
            var index = cursor.FirstVisible + row;
            var color = index == cursor.Selected ? UiTheme.TextPrimary : UiTheme.TextSecondary;
            DrawLine(batch, _list.Entries[index].Quest.Title, RowAt(list, row), color * opacity);
        }

        if (_detail is { } detail)
        {
            DrawDetail(batch, detail, column with { X = list.Right + DetailGap, Width = column.Width - ListWidth - DetailGap }, opacity);
        }
    }

    // The chosen quest's lines, one per row: title, kind, description, the stage's text.
    private void DrawDetail(SpriteBatch batch, QuestDetail detail, Rectangle area, float opacity)
    {
        DrawLine(batch, detail.Title, RowAt(area, 0), UiTheme.TextPrimary * opacity);
        DrawLine(batch, detail.KindName, RowAt(area, 1), UiTheme.TextSecondary * opacity);
        DrawLine(batch, detail.Description, RowAt(area, 2), UiTheme.TextSecondary * opacity);
        DrawLine(batch, detail.StageText, RowAt(area, 3), UiTheme.TextPrimary * opacity);
    }

    private static Rectangle RowAt(Rectangle column, int row) =>
        new(column.X, column.Y + (row * UiTheme.MenuRowHeight), column.Width, UiTheme.MenuRowHeight);

    private void DrawLine(SpriteBatch batch, string line, Rectangle row, Color color) =>
        batch.DrawString(_font, line, new Vector2(row.X + UiTheme.MenuTextInset, row.Y + ((row.Height - _font.LineHeight) / 2f)), color);

    private void RefreshDetail() => _detail = _list.Selected is { } selected ? QuestDetail.Of(selected, _messages) : null;
}
