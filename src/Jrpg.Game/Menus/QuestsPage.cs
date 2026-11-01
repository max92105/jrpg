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
/// The menu's Quests page, its first tab (decision 393): the Active or the Completed quests (decision 390, Left and
/// Right switch) in the journal's order (decision 391), drawn by <see cref="QuestListView"/> under the shared gliding
/// selection; beside them the chosen quest's detail (decision 392) drawn by <see cref="QuestDetailView"/>. Read from
/// Core when the page opens and when the choice moves, so drawing formats nothing.
/// </summary>
internal sealed class QuestsPage : IMenuPage
{
    private readonly GameSession _session;
    private readonly UiMessages _messages;
    private readonly SpriteFontBase _font;
    private readonly string[] _categoryNames;
    private readonly string[] _emptyLines;
    private readonly QuestListView _listView;
    private readonly QuestDetailView _detailView;
    private readonly QuestList _list;
    private QuestDetail? _detail;

    /// <param name="text">Messages and fonts for the titles and the lines.</param>
    /// <param name="ui">The shared selection textures.</param>
    /// <param name="session">Whose journal the page lists.</param>
    public QuestsPage(ScreenText text, UiTextures ui, GameSession session)
    {
        _session = session;
        _messages = text.Messages;
        _font = text.Fonts.FontAt(UiTheme.MenuTextPixels);
        _categoryNames = [text.Messages.Text(MessageIds.QuestsActive), text.Messages.Text(MessageIds.QuestsCompleted)];
        _emptyLines = [text.Messages.Text(MessageIds.QuestsNone), text.Messages.Text(MessageIds.QuestsNoneCompleted)];
        _listView = new QuestListView(ui, _font, _categoryNames);
        _detailView = new QuestDetailView(text);
        _list = new QuestList(title => TextEllipsis.Fit(_font, title, QuestListView.TitleWidth));
        Hint = text.Messages.Text(MessageIds.MenuHintQuests);
    }

    public string TitleMessageId => MessageIds.MenuQuests;

    public TabGlyph Glyph => TabGlyph.Quests;

    public string Hint { get; }

    public bool LeavesOnCancel => true;

    /// <summary>The category shown.</summary>
    internal QuestCategory Category => _list.Category;

    /// <summary>
    /// The shown category's titles as content has them, in the journal's order; empty when its empty line shows.
    /// </summary>
    internal IReadOnlyList<string> ShownTitles => [.. _list.Rows.Select(row => row.Entry.Quest.Title)];

    /// <summary>The chosen quest's detail; null while the shown category lists no quest.</summary>
    internal QuestDetail? Detail => _detail;

    public void Open()
    {
        _list.Show(_session.Quests.Active, _session.Quests.Completed);
        RefreshDetail();
    }

    public void Advance(TimeSpan delta) => _list.Advance(delta);

    // The detail is rebuilt only when the choice or the category changed, never per frame.
    public SceneRequest? Update(InputFrame input)
    {
        var before = (_list.Category, _list.Selected);
        if (input.IsPressed(GameButton.Up))
        {
            _list.MoveUp();
        }

        if (input.IsPressed(GameButton.Down))
        {
            _list.MoveDown();
        }

        if (input.IsPressed(GameButton.Right))
        {
            _list.ShowNext();
        }

        if (input.IsPressed(GameButton.Left))
        {
            _list.ShowPrevious();
        }

        if (before != (_list.Category, _list.Selected))
        {
            RefreshDetail();
        }

        return null;
    }

    public void Draw(SpriteBatch batch, TextureLibrary textures, Rectangle area, float opacity)
    {
        _listView.Draw(batch, textures, new QuestListView.Place(_list.Rows, _list.Cursor, _list.Selection, _list.Category, _emptyLines[(int)_list.Category], area), opacity);
        _detailView.Draw(batch, textures, new QuestDetailView.Place(_detail, QuestDetailView.ColumnIn(area, QuestListView.ColumnWidth)), opacity);
    }

    private void RefreshDetail() => _detail = _list.Selected is { } selected ? QuestDetail.Of(selected, _list.Category, _messages) : null;
}
