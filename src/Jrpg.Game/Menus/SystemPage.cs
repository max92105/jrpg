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
/// The menu's System page (legacy System sub menu): save, back to the title, or quit. Save opens the save files in the
/// page's place; the other two ask first, since unsaved progress isn't tracked (decision 85).
/// </summary>
internal sealed class SystemPage : IMenuPage
{
    private static readonly ImmutableArray<Entry> _entries =
    [
        new OpensSaves(MessageIds.SystemSave),
        new AsksFirst(MessageIds.SystemTitleScreen, MessageIds.ConfirmTitleScreen, () => new TitleRequested()),
        new AsksFirst(MessageIds.SystemQuit, MessageIds.ConfirmQuit, () => new QuitRequested()),
    ];

    private readonly ListCursor _cursor = new(_entries.Length, _ => true);
    private readonly UiMessages _messages;
    private readonly SpriteFontBase _font;
    private readonly string[] _labels;
    private readonly ConfirmPromptView _promptView;
    private readonly SavePage _savePage;
    private ConfirmPrompt? _asking;
    private bool _isSaving;

    /// <param name="text">Messages and fonts for the entries and the question.</param>
    /// <param name="ui">The shared selection textures the question's buttons use.</param>
    /// <param name="savePage">Shown in this page's place while the player saves.</param>
    public SystemPage(ScreenText text, UiTextures ui, SavePage savePage)
    {
        _messages = text.Messages;
        _font = text.Fonts.FontAt(UiTheme.MenuTextPixels);
        _labels = [.. _entries.Select(entry => text.Messages.Text(entry.LabelId))];
        _promptView = new ConfirmPromptView(text, ui);
        _savePage = savePage;
    }

    public string TitleMessageId => MessageIds.MenuSystem;

    /// <summary>
    /// False while a question is open, which Cancel answers No, or while saving, where Cancel goes back to the entries.
    /// </summary>
    public bool LeavesOnCancel => _asking is null && !_isSaving;

    public SceneRequest? Update(InputFrame input)
    {
        if (_isSaving)
        {
            UpdateSaving(input);
            return null;
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
        if (_isSaving)
        {
            _savePage.Draw(batch, textures, area, opacity);
            return;
        }

        for (var index = 0; index < _labels.Length; index++)
        {
            var color = index == _cursor.Selected ? UiTheme.TextPrimary : UiTheme.TextSecondary;
            var rowTop = area.Y + (index * UiTheme.MenuRowHeight);
            var position = new Vector2(area.X + UiTheme.MenuTextInset, rowTop + ((UiTheme.MenuRowHeight - _font.LineHeight) / 2f));
            batch.DrawString(_font, _labels[index], position, color * opacity);
        }

        if (_asking is not null)
        {
            _promptView.Draw(batch, textures, _asking, opacity);
        }
    }

    private void UpdateSaving(InputFrame input)
    {
        if (input.IsPressed(GameButton.Cancel))
        {
            _isSaving = false;
            return;
        }

        _savePage.Update(input);
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
            case OpensSaves:
                _savePage.Open();
                _isSaving = true;
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

    // Opens the save files in the page's place.
    private sealed record OpensSaves(string LabelId) : Entry(LabelId);

    // Asks QuestionId first, then asks the flow for Request once answered Yes.
    private sealed record AsksFirst(string LabelId, string QuestionId, Func<SceneRequest> Request) : Entry(LabelId);
}
