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
/// 390, 391), or a line saying there are none. Read from Core when the page opens, so drawing formats nothing; no
/// cursor yet.
/// </summary>
internal sealed class QuestsPage : IMenuPage
{
    private readonly GameSession _session;
    private readonly SpriteFontBase _font;
    private readonly string _none;
    private readonly ColumnPanelView _panel = new(UiTheme.PageColumn);
    private string[] _titles = [];

    /// <param name="text">Messages and fonts for the titles and the lines.</param>
    /// <param name="session">Whose journal the page lists.</param>
    public QuestsPage(ScreenText text, GameSession session)
    {
        _session = session;
        _font = text.Fonts.FontAt(UiTheme.MenuTextPixels);
        _none = text.Messages.Text(MessageIds.QuestsNone);
        Hint = text.Messages.Text(MessageIds.MenuHintQuests);
    }

    public string TitleMessageId => MessageIds.MenuQuests;

    public TabGlyph Glyph => TabGlyph.Quests;

    public string Hint { get; }

    public bool LeavesOnCancel => true;

    /// <summary>The titles listed, in the journal's order; empty when the none line shows.</summary>
    internal IReadOnlyList<string> ShownTitles => _titles;

    public void Open() => _titles = [.. _session.Quests.Active.Select(entry => entry.Quest.Title)];

    public SceneRequest? Update(InputFrame input) => null;

    public void Draw(SpriteBatch batch, TextureLibrary textures, Rectangle area, float opacity)
    {
        var column = MenuLayout.SingleColumn(area);
        _panel.DrawClosed(batch, textures.Pixel, MenuLayout.FullHeight(column), opacity);
        if (_titles.Length == 0)
        {
            DrawLine(batch, _none, RowAt(column, 0), UiTheme.TextSecondary * opacity);
            return;
        }

        for (var index = 0; index < _titles.Length; index++)
        {
            DrawLine(batch, _titles[index], RowAt(column, index), UiTheme.TextPrimary * opacity);
        }
    }

    private static Rectangle RowAt(Rectangle column, int row) =>
        new(column.X, column.Y + (row * UiTheme.MenuRowHeight), column.Width, UiTheme.MenuRowHeight);

    private void DrawLine(SpriteBatch batch, string line, Rectangle row, Color color) =>
        batch.DrawString(_font, line, new Vector2(row.X + UiTheme.MenuTextInset, row.Y + ((row.Height - _font.LineHeight) / 2f)), color);
}
