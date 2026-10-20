using FontStashSharp;
using FontStashSharp.RichText;
using Jrpg.Data.Art;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Dialogue;

/// <summary>
/// The message box at the bottom of the map: the typed part of one page in the legacy <c>DialogueBox</c> panel, a
/// blinking marker once the page is complete, and the speaker's name and diamond-framed portrait when the speech has
/// them (developer, 2026-10-19: the look of the turn-order portraits in <c>docs/ui_ux_exemples</c>).
/// </summary>
internal sealed class DialogueBox
{
    // Legacy MainWindow placed a 600 x 120 box 20 px above the bottom; doubled for the virtual screen.
    private const int Width = 1200;
    private const int Height = 240;
    private const int BottomMargin = 40;

    // Legacy 2 px border plus 8 px padding, doubled.
    private const int TextInset = 20;

    // A small diamond rather than legacy's ▶, which the font lacks; diamonds echo the reference UI.
    private const int MarkerSide = 14;

    // The portrait's diamond fills the box's height inside the inset; a white frame shows around it.
    private const int PortraitSize = Height - (2 * TextInset);
    private const int PortraitFrame = 3;

    // The name sits on a strip just above the box's top-left corner.
    private const int NameStripWidth = 520;
    private const int NameStripHeight = 44;
    private const int NameStripGap = 8;
    private const int NameInset = 24;

    private static readonly Rectangle _area = new((VirtualScreen.Width - Width) / 2, VirtualScreen.Height - BottomMargin - Height, Width, Height);
    private static readonly Vector2 _markerCenter = new(_area.Right - TextInset - MarkerSide, _area.Bottom - TextInset - MarkerSide);
    private static readonly Vector2 _portraitCenter = new(_area.X + TextInset + (PortraitSize / 2f), _area.Y + (Height / 2f));
    private static readonly Rectangle _nameStrip = new(_area.X, _area.Y - NameStripGap - NameStripHeight, NameStripWidth, NameStripHeight);

    private readonly PanelView _panel = new(UiTheme.MessagePanel);
    private readonly SpriteFontBase _font;
    private readonly SpriteFontBase _speakerFont;
    private readonly RichTextLayout _layout;

    // Baked on first use: a portrait is cut into its diamond once, the strip's gradient once.
    private readonly Dictionary<string, Texture2D> _portraits = [];
    private Texture2D? _nameStripTexture;

    private Vector2 _textTopLeft;
    private string? _speakerCaps;

    public DialogueBox(FontLibrary fonts)
    {
        _font = fonts.FontAt(UiTheme.MessageTextPixels);
        _speakerFont = fonts.FontAt(UiTheme.SpeakerTextPixels);
        _layout = new RichTextLayout
        {
            // Off, so a bracket or a slash in content text is never read as a FontStashSharp formatting command.
            SupportsCommands = false,
            Font = _font,
        };
    }

    /// <summary>
    /// Lays out the current page, beside the portrait when there is one, and the speaker's name. Called when a speech
    /// starts and when its page turns; drawing then shows slices of the page and allocates nothing.
    /// </summary>
    public void Show(SpeechPlayback reading)
    {
        var textLeft = reading.Speech.PortraitFile is null ? TextInset : TextInset + PortraitSize + TextInset;
        _textTopLeft = new Vector2(_area.X + textLeft, _area.Y + TextInset);
        _layout.Width = Width - textLeft - TextInset;
        _layout.Text = reading.CurrentPage;
        _speakerCaps = reading.Speech.Speaker?.ToUpperInvariant();
    }

    /// <param name="batch">The begun batch.</param>
    /// <param name="textures">For the panel, the portrait and the marker.</param>
    /// <param name="reading">The playback last given to <see cref="Show"/>.</param>
    public void Draw(SpriteBatch batch, TextureLibrary textures, SpeechPlayback reading)
    {
        _panel.Draw(batch, textures, _area);
        if (_speakerCaps is not null)
        {
            DrawSpeaker(batch, textures, _speakerCaps);
        }

        if (reading.Speech.PortraitFile is { } portraitFile)
        {
            DrawPortrait(batch, textures, portraitFile);
        }

        DrawTypedText(batch, reading.VisibleLength);
        if (reading.IsMarkerVisible)
        {
            new Diamond(_markerCenter, MarkerSide).Draw(batch, textures.Pixel, UiTheme.MessageText);
        }
    }

    private void DrawSpeaker(SpriteBatch batch, TextureLibrary textures, string speakerCaps)
    {
        _nameStripTexture ??= textures.Create(NameStripWidth, 1, GradientStrip.Colors(UiTheme.SpeakerStrip, NameStripWidth));
        batch.Draw(_nameStripTexture, _nameStrip, Color.White);
        var namePosition = new Vector2(_nameStrip.X + NameInset, _nameStrip.Y + ((NameStripHeight - _speakerFont.LineHeight) / 2f));
        _speakerFont.DrawText(batch, speakerCaps, namePosition, UiTheme.MessageText, characterSpacing: UiTheme.SpeakerLetterSpacing);
    }

    private void DrawPortrait(SpriteBatch batch, TextureLibrary textures, string portraitFile)
    {
        Diamond.WithDiagonal(_portraitCenter, PortraitSize + (2 * PortraitFrame)).Draw(batch, textures.Pixel, UiTheme.MessageText);
        var corner = new Point((int)_portraitCenter.X - (PortraitSize / 2), (int)_portraitCenter.Y - (PortraitSize / 2));
        batch.Draw(PortraitOf(portraitFile, textures), new Rectangle(corner, new Point(PortraitSize)), Color.White);
    }

    private Texture2D PortraitOf(string portraitFile, TextureLibrary textures)
    {
        if (!_portraits.TryGetValue(portraitFile, out var baked))
        {
            var source = textures.Get(ArtFolders.Portraits, portraitFile);
            var pixels = new Color[source.Width * source.Height];
            source.GetData(pixels);
            baked = textures.Create(PortraitSize, PortraitSize, DiamondCrop.Bake(pixels, source.Width, source.Height, PortraitSize));
            _portraits.Add(portraitFile, baked);
        }

        return baked;
    }

    // Each line is cut where the typing has reached, so words never jump to the next line as they appear. A line ends
    // where the next begins: its Count is in glyphs, which a character outside the basic plane (an emoji) makes fewer
    // than its chars.
    private void DrawTypedText(SpriteBatch batch, int visibleLength)
    {
        var text = _layout.Text;
        var lines = _layout.Lines;
        var position = _textTopLeft;
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
