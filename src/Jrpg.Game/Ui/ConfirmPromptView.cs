using FontStashSharp;
using Jrpg.Game.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Ui;

/// <summary>
/// Draws a <see cref="ConfirmPrompt"/> as legacy <c>ConfirmDialog</c>: the screen dimmed, a raised band across its middle
/// with hairlines above and below, the question, and the two buttons, the selected one in the blue selection.
/// </summary>
internal sealed class ConfirmPromptView
{
    // Legacy DialogStyle padding 40 x 32, Text.Heading 22 px with 28 px below; MenuButtonStyle 150 x 46, 8 px margins,
    // 17 px text.
    private const int QuestionTextPixels = 22;
    private const int ButtonTextPixels = 17;
    private const int BandPadding = 32;
    private const int QuestionGap = 28;
    private const int ButtonWidth = 150;
    private const int ButtonHeight = 46;
    private const int ButtonMargin = 8;

    private readonly UiTextures _ui;
    private readonly SpriteFontBase _questionFont;
    private readonly SpriteFontBase _buttonFont;
    private readonly string _yes;
    private readonly string _no;
    private readonly Rectangle _band;
    private readonly Rectangle _yesButton;
    private readonly Rectangle _noButton;
    private readonly float _yesWidth;
    private readonly float _noWidth;

    // Measured when the question changes rather than every frame.
    private string? _measuredQuestion;
    private float _questionWidth;

    public ConfirmPromptView(ScreenText text, UiTextures ui)
    {
        _ui = ui;
        _questionFont = text.Fonts.FontAt(QuestionTextPixels);
        _buttonFont = text.Fonts.FontAt(ButtonTextPixels);
        _yes = text.Messages.Text(MessageIds.ConfirmYes);
        _no = text.Messages.Text(MessageIds.ConfirmNo);
        _yesWidth = _buttonFont.MeasureString(_yes).X;
        _noWidth = _buttonFont.MeasureString(_no).X;

        var bandHeight = (2 * BandPadding) + (int)_questionFont.LineHeight + QuestionGap + ButtonHeight;
        _band = new Rectangle(0, (VirtualScreen.Height - bandHeight) / 2, VirtualScreen.Width, bandHeight);
        var buttonsTop = _band.Bottom - BandPadding - ButtonHeight;
        var center = VirtualScreen.Width / 2;
        _yesButton = new Rectangle(center - ButtonMargin - ButtonWidth, buttonsTop, ButtonWidth, ButtonHeight);
        _noButton = new Rectangle(center + ButtonMargin, buttonsTop, ButtonWidth, ButtonHeight);
    }

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the shared textures are made.</param>
    /// <param name="prompt">The question and its selected answer.</param>
    /// <param name="opacity">0 to 1, while the menu fades in.</param>
    public void Draw(SpriteBatch batch, TextureLibrary textures, ConfirmPrompt prompt, float opacity)
    {
        var pixel = textures.Pixel;
        batch.Draw(pixel, new Rectangle(0, 0, VirtualScreen.Width, VirtualScreen.Height), UiTheme.Scrim * opacity);
        batch.Draw(pixel, _band, UiTheme.RaisedSurface * opacity);
        batch.Draw(pixel, new Rectangle(0, _band.Top, VirtualScreen.Width, 1), UiTheme.BarLine * opacity);
        batch.Draw(pixel, new Rectangle(0, _band.Bottom - 1, VirtualScreen.Width, 1), UiTheme.BarLine * opacity);

        if (!ReferenceEquals(prompt.Question, _measuredQuestion))
        {
            _measuredQuestion = prompt.Question;
            _questionWidth = _questionFont.MeasureString(prompt.Question).X;
        }

        var questionPosition = new Vector2((VirtualScreen.Width - _questionWidth) / 2, _band.Top + BandPadding);
        batch.DrawString(_questionFont, prompt.Question, questionPosition, UiTheme.TextPrimary * opacity);

        DrawButton(batch, textures, new PromptButton(_yesButton, _yes, _yesWidth, prompt.IsYesSelected, UiTheme.Accent), opacity);
        DrawButton(batch, textures, new PromptButton(_noButton, _no, _noWidth, !prompt.IsYesSelected, UiTheme.ButtonStroke), opacity);
    }

    private void DrawButton(SpriteBatch batch, TextureLibrary textures, PromptButton button, float opacity)
    {
        var area = button.Area;
        if (button.IsSelected)
        {
            batch.Draw(_ui.SelectionFill(textures), area, Color.White * opacity);
        }
        else
        {
            batch.Draw(textures.Pixel, area, UiTheme.ButtonFill * opacity);
        }

        var outline = (button.IsSelected ? UiTheme.SelectionStroke : button.Outline) * opacity;
        DrawOutline(batch, textures.Pixel, area, outline);

        var labelPosition = new Vector2(area.Center.X - (button.LabelWidth / 2), area.Y + ((area.Height - _buttonFont.LineHeight) / 2f));
        batch.DrawString(_buttonFont, button.Label, labelPosition, UiTheme.TextPrimary * opacity);
    }

    private static void DrawOutline(SpriteBatch batch, Texture2D pixel, Rectangle area, Color color)
    {
        batch.Draw(pixel, new Rectangle(area.X, area.Y, area.Width, 1), color);
        batch.Draw(pixel, new Rectangle(area.X, area.Bottom - 1, area.Width, 1), color);
        batch.Draw(pixel, new Rectangle(area.X, area.Y, 1, area.Height), color);
        batch.Draw(pixel, new Rectangle(area.Right - 1, area.Y, 1, area.Height), color);
    }

    // One of the two buttons as drawn this frame; Outline is its border when it isn't selected.
    private readonly record struct PromptButton(Rectangle Area, string Label, float LabelWidth, bool IsSelected, Color Outline);
}
