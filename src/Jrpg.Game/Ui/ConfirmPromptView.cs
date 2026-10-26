using FontStashSharp;
using Jrpg.Game.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Ui;

/// <summary>
/// Draws a <see cref="ConfirmPrompt"/>: the screen dimmed, a raised band across its middle with hairlines above and
/// below, the question, and the two buttons, the selected one in the blue selection.
/// </summary>
internal sealed class ConfirmPromptView
{
    // The dialog's padding 40 x 32, a 30 px heading with 28 px below; buttons 190 x 56 with 8 px margins and 24 px text
    // (placeholders).
    private const int QuestionTextPixels = 30;
    private const int ButtonTextPixels = 24;
    private const int BandPadding = 32;
    private const int QuestionGap = 28;
    private const int ButtonWidth = 190;
    private const int ButtonHeight = 56;
    private const int ButtonMargin = 8;

    private readonly UiTextures _ui;
    private readonly SpriteFontBase _questionFont;
    private readonly SpriteFontBase _buttonFont;
    private readonly MeasuredText _yes;
    private readonly MeasuredText _no;
    private readonly Rectangle _band;
    private readonly Rectangle _yesButton;
    private readonly Rectangle _noButton;

    // Measured when the question changes rather than every frame.
    private MeasuredText _question = new(string.Empty, 0);

    public ConfirmPromptView(ScreenText text, UiTextures ui)
    {
        _ui = ui;
        _questionFont = text.Fonts.FontAt(QuestionTextPixels);
        _buttonFont = text.Fonts.FontAt(ButtonTextPixels);
        _yes = MeasuredText.Of(_buttonFont, text.Messages.Text(MessageIds.ConfirmYes));
        _no = MeasuredText.Of(_buttonFont, text.Messages.Text(MessageIds.ConfirmNo));

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

        if (!ReferenceEquals(prompt.Question, _question.Text))
        {
            _question = MeasuredText.Of(_questionFont, prompt.Question);
        }

        var questionPosition = new Vector2((VirtualScreen.Width - _question.Width) / 2, _band.Top + BandPadding);
        batch.DrawString(_questionFont, _question.Text, questionPosition, UiTheme.TextPrimary * opacity);

        DrawButton(batch, textures, new PromptButton(_yesButton, _yes, prompt.IsYesSelected, UiTheme.Accent), opacity);
        DrawButton(batch, textures, new PromptButton(_noButton, _no, !prompt.IsYesSelected, UiTheme.Stroke), opacity);
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
        Outline.Draw(batch, textures.Pixel, area, outline);

        var labelPosition = new Vector2(area.Center.X - (button.Label.Width / 2), area.Y + ((area.Height - _buttonFont.LineHeight) / 2f));
        batch.DrawString(_buttonFont, button.Label.Text, labelPosition, UiTheme.TextPrimary * opacity);
    }

    // One of the two buttons as drawn this frame; Outline is its border when it isn't selected.
    private readonly record struct PromptButton(Rectangle Area, MeasuredText Label, bool IsSelected, Color Outline);
}
