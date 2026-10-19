using FontStashSharp;
using FontStashSharp.RichText;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Dialogue;

/// <summary>
/// The message box at the bottom of the map: the typed part of one page in the legacy <c>DialogueBox</c> panel, and a
/// blinking marker once the page is complete.
/// </summary>
internal sealed class DialogueBox
{
    // Legacy MainWindow placed a 600 x 120 box 20 px above the bottom; doubled for the virtual screen.
    private const int Width = 1200;
    private const int Height = 240;
    private const int BottomMargin = 40;

    // Legacy 2 px border plus 8 px padding, doubled.
    private const int TextInset = 20;

    // A small diamond rather than legacy's ▶, which the font lacks; diamonds echo the reference UI (docs/ui_ux_exemples).
    private const int MarkerSide = 14;

    private static readonly Rectangle _area = new((VirtualScreen.Width - Width) / 2, VirtualScreen.Height - BottomMargin - Height, Width, Height);
    private static readonly Vector2 _markerCenter = new(_area.Right - TextInset - MarkerSide, _area.Bottom - TextInset - MarkerSide);

    private readonly PanelView _panel = new(UiTheme.MessagePanel);
    private readonly SpriteFontBase _font;
    private readonly RichTextLayout _layout;

    public DialogueBox(FontLibrary fonts)
    {
        _font = fonts.FontAt(UiTheme.MessageTextPixels);
        _layout = new RichTextLayout
        {
            // Off, so a bracket or a slash in content text is never read as a FontStashSharp formatting command.
            SupportsCommands = false,
            Font = _font,
            Width = Width - (2 * TextInset),
        };
    }

    /// <summary>Wraps the page into lines once; drawing then shows slices of it and allocates nothing.</summary>
    public void Show(string page) => _layout.Text = page;

    /// <param name="batch">The begun batch.</param>
    /// <param name="textures">For the panel and the marker.</param>
    /// <param name="reading">The playback whose current page was given to <see cref="Show"/>.</param>
    public void Draw(SpriteBatch batch, TextureLibrary textures, SpeechPlayback reading)
    {
        _panel.Draw(batch, textures, _area);
        DrawTypedText(batch, reading.VisibleLength);
        if (reading.IsMarkerVisible)
        {
            batch.Draw(textures.Pixel, _markerCenter, null, UiTheme.MessageText, MathHelper.PiOver4, new Vector2(0.5f), new Vector2(MarkerSide), SpriteEffects.None, 0);
        }
    }

    // Each line is cut where the typing has reached, so words never jump to the next line as they appear. A line ends
    // where the next begins: its Count is in glyphs, which a character outside the basic plane (an emoji) makes fewer
    // than its chars.
    private void DrawTypedText(SpriteBatch batch, int visibleLength)
    {
        var text = _layout.Text;
        var lines = _layout.Lines;
        var position = new Vector2(_area.X + TextInset, _area.Y + TextInset);
        for (var index = 0; index < lines.Count; index++)
        {
            var line = lines[index];
            var lineEnd = index + 1 < lines.Count ? lines[index + 1].TextStartIndex : text.Length;
            var shown = Math.Min(lineEnd, visibleLength) - line.TextStartIndex;
            if (shown <= 0)
            {
                return;
            }

            _font.DrawText(batch, new StringSegment(text, line.TextStartIndex, shown), position, UiTheme.MessageText);
            position.Y += line.Size.Y + _layout.VerticalSpacing;
        }
    }
}
