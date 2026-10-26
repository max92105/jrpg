using System.Collections.Immutable;
using FontStashSharp;
using Jrpg.Game.Input;
using Jrpg.Game.Rendering;
using Jrpg.Game.Scenes;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Menus;

/// <summary>
/// The menu's System page: save, load, back to the title, or quit. Save and Load open the save files in the page's
/// place; the other two ask first, since unsaved progress isn't tracked (decision 85).
/// </summary>
internal sealed class SystemPage : IMenuPage
{
    private readonly ImmutableArray<Entry> _entries;
    private readonly ListCursor _cursor;
    private readonly UiMessages _messages;
    private readonly string _ownHint;
    private readonly SpriteFontBase _font;
    private readonly string[] _labels;
    private readonly ConfirmPromptView _promptView;
    private readonly UiTextures _ui;
    private ISaveFilesPage? _openFiles;
    private ConfirmPrompt? _asking;

    /// <param name="text">Messages and fonts for the entries and the question.</param>
    /// <param name="ui">The shared selection textures: the selected entry's band and the question's buttons.</param>
    /// <param name="savePage">Shown in this page's place while the player saves.</param>
    /// <param name="loadPage">Shown in this page's place while the player picks a save to load.</param>
    public SystemPage(ScreenText text, UiTextures ui, SavePage savePage, LoadPage loadPage)
    {
        _ownHint = text.Messages.Text(MessageIds.MenuHintSystem);
        // Save, Load, Title, Quit (placeholder order).
        _entries =
        [
            new OpensFiles(MessageIds.SystemSave, savePage),
            new OpensFiles(MessageIds.SystemLoad, loadPage),
            new AsksFirst(MessageIds.SystemTitleScreen, MessageIds.ConfirmTitleScreen, () => new TitleRequested()),
            new AsksFirst(MessageIds.SystemQuit, MessageIds.ConfirmQuit, () => new QuitRequested()),
        ];
        _cursor = new ListCursor(_entries.Length, _ => true);
        _messages = text.Messages;
        _font = text.Fonts.FontAt(UiTheme.MenuTextPixels);
        _labels = [.. _entries.Select(entry => text.Messages.Text(entry.LabelId))];
        _promptView = new ConfirmPromptView(text, ui);
        _ui = ui;
    }

    public string TitleMessageId => MessageIds.MenuSystem;

    /// <summary>The open save files' hint while they show in this page's place.</summary>
    public string Hint => _openFiles?.Hint ?? _ownHint;

    public bool ShowsPartySidebar => true;

    /// <summary>
    /// False while a question is open, which Cancel answers No, or while the save files are open, where Cancel goes
    /// back to the entries.
    /// </summary>
    public bool LeavesOnCancel => _asking is null && _openFiles is null;

    // The save files read themselves when their own page opens, so only the cursor goes back.
    public void Open() => _cursor.Select(0);

    public void Advance(TimeSpan delta) => _openFiles?.Advance(delta);

    public SceneRequest? Update(InputFrame input)
    {
        if (_openFiles is not null)
        {
            return UpdateFiles(_openFiles, input);
        }

        if (_asking is not null)
        {
            return Answer(_asking, input);
        }

        MoveOrChoose(input);
        return null;
    }

    public void Draw(SpriteBatch batch, TextureLibrary textures, Rectangle area, float opacity)
    {
        if (_openFiles is not null)
        {
            _openFiles.Draw(batch, textures, area, opacity);
            return;
        }

        for (var index = 0; index < _labels.Length; index++)
        {
            var rowTop = area.Y + (index * UiTheme.MenuRowHeight);
            var isSelected = index == _cursor.Selected;
            if (isSelected)
            {
                DrawSelectedEntry(batch, textures, new Rectangle(area.X, rowTop, area.Width, UiTheme.MenuRowHeight), opacity);
            }

            var color = isSelected ? UiTheme.TextPrimary : UiTheme.TextSecondary;
            var position = new Vector2(area.X + UiTheme.MenuTextInset, rowTop + ((UiTheme.MenuRowHeight - _font.LineHeight) / 2f));
            batch.DrawString(_font, _labels[index], position, color * opacity);
        }

        if (_asking is not null)
        {
            _promptView.Draw(batch, textures, _asking, opacity);
        }
    }

    // While a question is open the cursor is on its buttons, so the entry keeps a dimmed band and no pointer.
    private void DrawSelectedEntry(SpriteBatch batch, TextureLibrary textures, Rectangle row, float opacity)
    {
        if (_asking is not null)
        {
            _ui.DrawSelectedRow(batch, textures, row, opacity * UiTheme.DimmedSelectionOpacity);
            return;
        }

        _ui.DrawSelectedRow(batch, textures, row, opacity);
        MenuPointer.Draw(batch, textures.Pixel, new Vector2(row.X, row.Center.Y), opacity);
    }

    private SceneRequest? UpdateFiles(ISaveFilesPage files, InputFrame input)
    {
        if (input.IsPressed(GameButton.Cancel) && files.LeavesOnCancel)
        {
            _openFiles = null;
            return null;
        }

        return files.Update(input);
    }

    private void MoveOrChoose(InputFrame input)
    {
        if (input.IsPressed(GameButton.Up))
        {
            _cursor.MoveUp();
        }

        if (input.IsPressed(GameButton.Down))
        {
            _cursor.MoveDown();
        }

        if (!input.IsPressed(GameButton.Confirm))
        {
            return;
        }

        switch (_entries[_cursor.Selected])
        {
            case OpensFiles opens:
                opens.Page.Open();
                _openFiles = opens.Page;
                break;
            case AsksFirst asks:
                _asking = new ConfirmPrompt(_messages.Text(asks.QuestionId));
                break;
        }
    }

    private SceneRequest? Answer(ConfirmPrompt asking, InputFrame input)
    {
        var answer = asking.Update(input);
        if (answer is null)
        {
            return null;
        }

        _asking = null;
        return answer == ConfirmAnswer.Yes && _entries[_cursor.Selected] is AsksFirst asks ? asks.Request() : null;
    }

    // One System entry, by what choosing it does.
    private abstract record Entry(string LabelId);

    // Opens the save files, to save or to load, in the page's place.
    private sealed record OpensFiles(string LabelId, ISaveFilesPage Page) : Entry(LabelId);

    // Asks QuestionId first, then asks the flow for Request once answered Yes.
    private sealed record AsksFirst(string LabelId, string QuestionId, Func<SceneRequest> Request) : Entry(LabelId);
}
