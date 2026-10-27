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
    // Placeholders: a 720 px window, wider if the question needs it (it never wraps); a 28 px question; two stacked
    // dialog buttons 10 px apart with 24 px text.
    private const int WindowWidth = 720;
    private const int QuestionTextPixels = 28;
    private const int ButtonTextPixels = 24;
    private const int ButtonGap = 10;
    private static readonly Point _screenCenter = new(VirtualScreen.Width / 2, VirtualScreen.Height / 2);

    private readonly FramedWindowView _windowView;
    private readonly DialogButtonView _buttonView;
    private readonly SpriteFontBase _questionFont;
    private readonly MeasuredText _yes;
    private readonly MeasuredText _no;

    // Measured, and the window laid out, when the question changes rather than every frame.
    private MeasuredText _question;
    private FramedWindowLayout _window;

    public ConfirmPromptView(ScreenText text, UiTextures ui)
    {
        _windowView = new FramedWindowView(ui);
        _questionFont = text.Fonts.FontAt(QuestionTextPixels);
        var buttonFont = text.Fonts.FontAt(ButtonTextPixels);
        _buttonView = new DialogButtonView(ui, buttonFont);
        _yes = MeasuredText.Of(buttonFont, text.Messages.Text(MessageIds.ConfirmYes));
        _no = MeasuredText.Of(buttonFont, text.Messages.Text(MessageIds.ConfirmNo));
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

        var buttonLeft = _window.Body.Center.X - (DialogButtonView.Width / 2);
        var yes = new Rectangle(buttonLeft, _window.Body.Y, DialogButtonView.Width, DialogButtonView.Height);
        var no = yes with { Y = yes.Bottom + ButtonGap };
        _buttonView.Draw(batch, textures, new DialogButtonView.Button(yes, _yes, prompt.IsYesSelected), opacity);
        _buttonView.Draw(batch, textures, new DialogButtonView.Button(no, _no, !prompt.IsYesSelected), opacity);
    }

    private void Ask(string question)
    {
        _question = MeasuredText.Of(_questionFont, question);
        var bodyWidth = Math.Max(WindowWidth - (2 * FramedWindowLayout.PaddingX), (int)Math.Ceiling(_question.Width));
        var bodySize = new Point(bodyWidth, (2 * DialogButtonView.Height) + ButtonGap);
        _window = FramedWindowLayout.Titled(_screenCenter, bodySize, _questionFont.LineHeight);
    }
}
