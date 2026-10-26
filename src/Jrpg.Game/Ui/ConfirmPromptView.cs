using FontStashSharp;
using Jrpg.Game.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Ui;

/// <summary>
/// Draws a <see cref="ConfirmPrompt"/>: the screen dimmed, then a centered titled window
/// (<see cref="FramedWindowView"/>) with the question as its title and Yes above No, the selected answer in the shared
/// selection look (decision 252).
/// </summary>
internal sealed class ConfirmPromptView
{
    // Placeholders: a 720 px window, wider if the question needs it (it never wraps); a 28 px question; two stacked 260
    // x 50 buttons 10 px apart with 24 px text.
    private const int WindowWidth = 720;
    private const int QuestionTextPixels = 28;
    private const int ButtonTextPixels = 24;
    private const int ButtonWidth = 260;
    private const int ButtonHeight = 50;
    private const int ButtonGap = 10;
    private static readonly Point _screenCenter = new(VirtualScreen.Width / 2, VirtualScreen.Height / 2);

    private readonly UiTextures _ui;
    private readonly FramedWindowView _windowView;
    private readonly SpriteFontBase _questionFont;
    private readonly SpriteFontBase _buttonFont;
    private readonly MeasuredText _yes;
    private readonly MeasuredText _no;

    // Measured, and the window laid out, when the question changes rather than every frame.
    private MeasuredText _question;
    private FramedWindowLayout _window;

    public ConfirmPromptView(ScreenText text, UiTextures ui)
    {
        _ui = ui;
        _windowView = new FramedWindowView(ui);
        _questionFont = text.Fonts.FontAt(QuestionTextPixels);
        _buttonFont = text.Fonts.FontAt(ButtonTextPixels);
        _yes = MeasuredText.Of(_buttonFont, text.Messages.Text(MessageIds.ConfirmYes));
        _no = MeasuredText.Of(_buttonFont, text.Messages.Text(MessageIds.ConfirmNo));
        Ask(string.Empty);
    }

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the shared textures are made.</param>
    /// <param name="prompt">The question and its selected answer.</param>
    /// <param name="opacity">0 to 1, while the menu fades in.</param>
    public void Draw(SpriteBatch batch, TextureLibrary textures, ConfirmPrompt prompt, float opacity)
    {
        if (!ReferenceEquals(prompt.Question, _question.Text))
        {
            Ask(prompt.Question);
        }

        batch.Draw(textures.Pixel, new Rectangle(0, 0, VirtualScreen.Width, VirtualScreen.Height), UiTheme.Scrim * opacity);
        _windowView.Draw(batch, textures, _window, opacity);
        var questionPosition = _window.TitleCenter - new Vector2(_question.Width / 2, _questionFont.LineHeight / 2);
        batch.DrawString(_questionFont, _question.Text, questionPosition, UiTheme.TextPrimary * opacity);

        var buttonLeft = _window.Body.Center.X - (ButtonWidth / 2);
        var yes = new Rectangle(buttonLeft, _window.Body.Y, ButtonWidth, ButtonHeight);
        var no = yes with { Y = yes.Bottom + ButtonGap };
        DrawButton(batch, textures, new PromptButton(yes, _yes, prompt.IsYesSelected), opacity);
        DrawButton(batch, textures, new PromptButton(no, _no, !prompt.IsYesSelected), opacity);
    }

    private void Ask(string question)
    {
        _question = MeasuredText.Of(_questionFont, question);
        var bodyWidth = Math.Max(WindowWidth - (2 * FramedWindowLayout.PaddingX), (int)Math.Ceiling(_question.Width));
        var bodySize = new Point(bodyWidth, (2 * ButtonHeight) + ButtonGap);
        _window = FramedWindowLayout.Titled(_screenCenter, bodySize, _questionFont.LineHeight);
    }

    // The selected answer in the shared band with the pointer; the other in a faint outline.
    private void DrawButton(SpriteBatch batch, TextureLibrary textures, PromptButton button, float opacity)
    {
        var area = button.Area;
        if (button.IsSelected)
        {
            _ui.DrawSelectedRow(batch, textures, area, opacity);
            MenuPointer.Draw(batch, textures.Pixel, new Vector2(area.X, area.Center.Y), opacity);
        }
        else
        {
            Outline.Draw(batch, textures.Pixel, area, UiTheme.Stroke * opacity);
        }

        var labelPosition = new Vector2(area.Center.X - (button.Label.Width / 2), area.Y + ((area.Height - _buttonFont.LineHeight) / 2f));
        var color = button.IsSelected ? UiTheme.TextPrimary : UiTheme.TextSecondary;
        batch.DrawString(_buttonFont, button.Label.Text, labelPosition, color * opacity);
    }

    // One of the two buttons as drawn this frame.
    private readonly record struct PromptButton(Rectangle Area, MeasuredText Label, bool IsSelected);
}
