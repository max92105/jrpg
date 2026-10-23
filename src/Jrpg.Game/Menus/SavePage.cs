using System.Diagnostics;
using FontStashSharp;
using Jrpg.Core.Session;
using Jrpg.Game.Input;
using Jrpg.Game.Rendering;
using Jrpg.Game.Saving;
using Jrpg.Game.Scenes;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Menus;

/// <summary>
/// Saving from the menu: Confirm writes the playthrough into the chosen file, asking first when the file is in use
/// (decision 62). A write the disk refuses says so on the page and leaves the list open.
/// </summary>
internal sealed class SavePage : ISaveFilesPage
{
    // The notice sits on the heading's line, at the page's 28 px right margin.
    private const int NoticeMargin = 28;
    private const int NoticeTop = 20;
    private const int NoticePixels = 18;

    private readonly SaveSlots _slots;
    private readonly GameSession _session;
    private readonly UiMessages _messages;
    private readonly SaveFilesPanel _panel;
    private readonly ConfirmPromptView _promptView;
    private readonly SpriteFontBase _noticeFont;
    private ConfirmPrompt? _asking;
    private int _askingFor;
    private MeasuredText? _notice;

    /// <param name="slots">The files.</param>
    /// <param name="session">What a save writes.</param>
    /// <param name="text">Messages and fonts.</param>
    /// <param name="ui">The shared gradient textures.</param>
    public SavePage(SaveSlots slots, GameSession session, ScreenText text, UiTextures ui)
    {
        Hint = text.Messages.Text(MessageIds.MenuHintSave);
        _slots = slots;
        _session = session;
        _messages = text.Messages;
        _panel = new SaveFilesPanel(slots, MessageIds.SystemSave, text, ui);
        _promptView = new ConfirmPromptView(text, ui);
        _noticeFont = text.Fonts.FontAt(NoticePixels);
    }

    /// <summary>The files as last read; empty until the page is opened.</summary>
    public IReadOnlyList<SaveSlotEntry> Entries => _panel.Entries;

    /// <summary>Whether the overwrite question is open.</summary>
    public bool IsAsking => _asking is not null;

    /// <summary>Why the last save didn't happen, or null.</summary>
    public string? Notice => _notice?.Text;

    public string Hint { get; }

    /// <summary>False while the overwrite question is open: Cancel answers it No instead.</summary>
    public bool LeavesOnCancel => _asking is null;

    public void Open()
    {
        _panel.Open();
        _notice = null;
        _asking = null;
    }

    /// <returns>Null: saving asks the flow for nothing.</returns>
    public SceneRequest? Update(InputFrame input)
    {
        if (_asking is not null)
        {
            Answer(_asking, input);
        }
        else if (_panel.Update(input) is { } chosen)
        {
            Choose(chosen);
        }

        return null;
    }

    public void Draw(SpriteBatch batch, TextureLibrary textures, Rectangle area, float opacity)
    {
        _panel.Draw(batch, textures, area, opacity);
        if (_notice is { } notice)
        {
            var position = new Vector2(area.Right - NoticeMargin - notice.Width, area.Y + NoticeTop);
            batch.DrawString(_noticeFont, notice.Text, position, UiTheme.Danger * opacity);
        }

        if (_asking is not null)
        {
            _promptView.Draw(batch, textures, _asking, opacity);
        }
    }

    private void Choose(SaveSlotEntry chosen)
    {
        if (chosen.IsEmpty)
        {
            Write(chosen.Number);
            return;
        }

        _asking = new ConfirmPrompt(_messages.Format(MessageIds.SaveOverwrite, FileNumber(chosen.Number)));
        _askingFor = chosen.Number;
    }

    private void Answer(ConfirmPrompt asking, InputFrame input)
    {
        var answer = asking.Update(input);
        if (answer is null)
        {
            return;
        }

        _asking = null;
        if (answer == ConfirmAnswer.Yes)
        {
            Write(_askingFor);
        }
    }

    private void Write(int number)
    {
        try
        {
            _slots.Write(number, _session);
            _notice = null;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            // The player is told the save didn't happen; the reason is for whoever debugs it.
            Debug.WriteLine($"Saving to {SaveSlots.NameOf(number)} failed: {exception.Message}");
            _notice = MeasuredText.Of(_noticeFont, _messages.Format(MessageIds.SaveFailed, FileNumber(number)));
        }

        _panel.Refresh();
    }

    private static MessageArgument FileNumber(int number) =>
        MessageArgument.Number("number", number);
}
