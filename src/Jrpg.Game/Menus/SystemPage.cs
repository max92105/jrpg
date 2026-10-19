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
/// The menu's System page (legacy System sub menu): back to the title, or quit. Both ask first, since unsaved progress
/// isn't tracked (decision 85).
/// </summary>
internal sealed class SystemPage : IMenuPage
{
    private static readonly ImmutableArray<Entry> _entries =
    [
        new(MessageIds.SystemTitleScreen, MessageIds.ConfirmTitleScreen, () => new TitleRequested()),
        new(MessageIds.SystemQuit, MessageIds.ConfirmQuit, () => new QuitRequested()),
    ];

    private readonly ListCursor _cursor = new(_entries.Length, _ => true);
    private readonly SpriteFontBase _font;
    private readonly string[] _labels;
    private readonly string[] _questions;
    private readonly ConfirmPromptView _promptView;
    private ConfirmPrompt? _asking;

    /// <param name="text">Messages and fonts for the entries and the question.</param>
    /// <param name="ui">The shared selection textures the question's buttons use.</param>
    public SystemPage(ScreenText text, UiTextures ui)
    {
        _font = text.Fonts.FontAt(UiTheme.MenuTextPixels);
        _labels = [.. _entries.Select(entry => text.Messages.Text(entry.LabelId))];
        _questions = [.. _entries.Select(entry => text.Messages.Text(entry.QuestionId))];
        _promptView = new ConfirmPromptView(text, ui);
    }

    public string TitleMessageId => MessageIds.MenuSystem;

    /// <summary>False while a question is open: Cancel answers it No instead.</summary>
    public bool LeavesOnCancel => _asking is null;

    public SceneRequest? Update(InputFrame input)
    {
        if (_asking is not null)
        {
            return Answer(_asking, input);
        }

        MoveOrAsk(input);
        return null;
    }

    public void Draw(SpriteBatch batch, TextureLibrary textures, Rectangle area, float opacity)
    {
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

    private void MoveOrAsk(InputFrame input)
    {
        if (input.IsPressed(GameButton.Up))
        {
            _cursor.MoveUp();
        }

        if (input.IsPressed(GameButton.Down))
        {
            _cursor.MoveDown();
        }

        if (input.IsPressed(GameButton.Confirm))
        {
            _asking = new ConfirmPrompt(_questions[_cursor.Selected]);
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
        return answer == ConfirmAnswer.Yes ? _entries[_cursor.Selected].Request() : null;
    }

    // One System entry: its label, what it asks first, and what it asks the flow for once answered Yes.
    private sealed record Entry(string LabelId, string QuestionId, Func<SceneRequest> Request);
}
