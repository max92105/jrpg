using FontStashSharp;
using Jrpg.Game.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Ui;

/// <summary>
/// Draws a <see cref="NoticeDialog"/> as the reference's notices (decision 268): the screen dimmed, then a centered
/// titled window (<see cref="FramedWindowView"/>) with the message under the separator and the OK button under it,
/// always selected since it is the only one.
/// </summary>
internal sealed class NoticeDialogView
{
    // Placeholders, the yes/no prompt's where they share a part: a 720 px window, wider if the message needs it (it
    // never wraps); a 28 px title; a 24 px message, 24 px above the button, whose label is 24 px too.
    private const int WindowWidth = 720;
    private const int TitleTextPixels = 28;
    private const int TextPixels = 24;
    private const int MessageGap = 24;
    private static readonly Point _screenCenter = new(VirtualScreen.Width / 2, VirtualScreen.Height / 2);

    private readonly FramedWindowView _windowView;
    private readonly DialogButtonView _buttonView;
    private readonly SpriteFontBase _titleFont;
    private readonly SpriteFontBase _textFont;
    private readonly MeasuredText _ok;

    // Measured, and the window laid out, when the dialog changes rather than every frame.
    private NoticeDialog? _shown;
    private MeasuredText _title;
    private MeasuredText _message;
    private FramedWindowLayout _window;

    /// <param name="text">The OK label and the fonts.</param>
    /// <param name="ui">The shared textures: the window, the selected button.</param>
    public NoticeDialogView(ScreenText text, UiTextures ui)
    {
        _windowView = new FramedWindowView(ui);
        _titleFont = text.Fonts.FontAt(TitleTextPixels);
        _textFont = text.Fonts.FontAt(TextPixels);
        _buttonView = new DialogButtonView(ui, _textFont);
        _ok = MeasuredText.Of(_textFont, text.Messages.Text(MessageIds.NoticeOk));
    }

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the shared textures are made.</param>
    /// <param name="dialog">The notice to show.</param>
    /// <param name="opacity">0 to 1, while the menu fades.</param>
    public void Draw(SpriteBatch batch, TextureLibrary textures, NoticeDialog dialog, float opacity)
    {
        if (!ReferenceEquals(dialog, _shown))
        {
            Show(dialog);
        }

        batch.Draw(textures.Pixel, new Rectangle(0, 0, VirtualScreen.Width, VirtualScreen.Height), UiTheme.Scrim * opacity);
        _windowView.Draw(batch, textures, _window, opacity);
        var titlePosition = _window.TitleCenter - new Vector2(_title.Width / 2, _titleFont.LineHeight / 2);
        batch.DrawString(_titleFont, _title.Text, titlePosition, UiTheme.TextPrimary * opacity);
        var body = _window.Body;
        batch.DrawString(_textFont, _message.Text, new Vector2(body.Center.X - (_message.Width / 2), body.Y), UiTheme.TextPrimary * opacity);

        var button = new Rectangle(body.Center.X - (DialogButtonView.Width / 2), body.Bottom - DialogButtonView.Height, DialogButtonView.Width, DialogButtonView.Height);
        _buttonView.Draw(batch, textures, new DialogButtonView.Button(button, _ok, IsSelected: true), opacity);
    }

    private void Show(NoticeDialog dialog)
    {
        _shown = dialog;
        _title = MeasuredText.Of(_titleFont, dialog.Title);
        _message = MeasuredText.Of(_textFont, dialog.Message);
        var widest = Math.Max(_title.Width, _message.Width);
        var bodyWidth = Math.Max(WindowWidth - (2 * FramedWindowLayout.PaddingX), (int)Math.Ceiling(widest));
        var bodyHeight = (int)MathF.Ceiling(_textFont.LineHeight) + MessageGap + DialogButtonView.Height;
        _window = FramedWindowLayout.Titled(_screenCenter, new Point(bodyWidth, bodyHeight), _titleFont.LineHeight);
    }
}
