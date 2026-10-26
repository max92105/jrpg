using FontStashSharp;
using FontStashSharp.RichText;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.NewGame;

/// <summary>
/// The highlighted character's information, right of the leader list (decisions 158, 168, 169), after the reference's
/// status screen: the portrait large at the right, fading out toward the text, and beside it the name, the class over a
/// section line with its description, the summary under a hairline, then the level 1 stats. A new character fades in.
/// </summary>
internal sealed class LeaderProfileView
{
    // Placeholder layout (decision 82): the text column from InfoLeft to the portrait's fading edge.
    private const int InfoLeft = 540;
    private const int TextWidth = 620;
    private const int NameTop = 110;
    private const int ClassTop = 180;
    private const int DescriptionTop = 214;
    private const int SummaryLineTop = 344;
    private const int SummaryTop = 364;
    private const int StatsTop = 560;
    private const int StatPitch = 56;
    private const int StatValueLeft = InfoLeft + 140;
    private const int NamePixels = 48;
    private const int ClassPixels = 18;
    private const int BodyPixels = 26;
    private const int StatLabelPixels = 18;
    private const int StatValuePixels = 32;

    // The portrait's square, right of the text, and the share of its width, from the left, that fades in.
    private const int PortraitPixels = 720;
    private const int PortraitTop = 188;
    private const int PortraitFadeWidth = 300;
    private const int PortraitFadeStrips = 30;

    private static readonly int _portraitLeft = VirtualScreen.Width - PortraitPixels;
    private static readonly TimeSpan _profileFade = TimeSpan.FromSeconds(0.2);

    private readonly UiTextures _ui;
    private readonly IReadOnlyList<LeaderProfile> _profiles;
    private readonly SpriteFontBase _nameFont;
    private readonly SpriteFontBase _classFont;
    private readonly SpriteFontBase _statLabelFont;
    private readonly SpriteFontBase _statValueFont;
    private readonly string[] _classCaps;
    private readonly RichTextLayout[] _descriptions;
    private readonly RichTextLayout[] _summaries;
    private int _shown;

    // Starts whole: the screen's own fade-in covers the first character.
    private TimeSpan _sinceShown = _profileFade;

    /// <param name="text">The fonts.</param>
    /// <param name="ui">The portraits and the section line.</param>
    /// <param name="profiles">Every choice's information, indexed like the list.</param>
    public LeaderProfileView(ScreenText text, UiTextures ui, IReadOnlyList<LeaderProfile> profiles)
    {
        _ui = ui;
        _profiles = profiles;
        _nameFont = text.Fonts.TitleFontAt(NamePixels);
        _classFont = text.Fonts.FontAt(ClassPixels);
        _statLabelFont = text.Fonts.FontAt(StatLabelPixels);
        _statValueFont = text.Fonts.FontAt(StatValuePixels);
        var bodyFont = text.Fonts.FontAt(BodyPixels);
        _classCaps = [.. profiles.Select(profile => profile.ClassName.ToUpperInvariant())];
        _descriptions = [.. profiles.Select(profile => WrappedText(profile.ClassDescription, bodyFont))];
        _summaries = [.. profiles.Select(profile => WrappedText(profile.Summary, bodyFont))];
    }

    /// <summary>How far the shown character has faded in, 0 to 1.</summary>
    internal float Opacity => Easing.ShareOf(_sinceShown, _profileFade);

    /// <summary>Where the shown character is in the list.</summary>
    internal int ShownIndex => _shown;

    /// <summary>Shows another character, fading in from nothing.</summary>
    /// <param name="choiceIndex">Where the character is in the list.</param>
    public void Show(int choiceIndex)
    {
        _shown = choiceIndex;
        _sinceShown = TimeSpan.Zero;
    }

    /// <summary>Advances the fade-in.</summary>
    public void Update(TimeSpan delta) => _sinceShown += delta;

    public void Draw(SpriteBatch batch, TextureLibrary textures)
    {
        var opacity = Opacity;
        var profile = _profiles[_shown];
        DrawPortrait(batch, _ui.Portrait(textures, profile.PortraitFile, PortraitPixels), opacity);
        batch.DrawString(_nameFont, profile.Name, new Vector2(InfoLeft, NameTop), UiTheme.TextPrimary * opacity);
        _classFont.DrawText(
            batch, _classCaps[_shown], new Vector2(InfoLeft, ClassTop), UiTheme.TextSecondary * opacity, characterSpacing: UiTheme.SpeakerLetterSpacing);
        var line = _ui.SectionLine(textures);
        batch.Draw(line, new Rectangle(InfoLeft, ClassTop + SectionTitle.LineGap, TextWidth, 1), Color.White * opacity);
        _descriptions[_shown].Draw(batch, new Vector2(InfoLeft, DescriptionTop), UiTheme.TextSecondary * opacity);
        batch.Draw(line, new Rectangle(InfoLeft, SummaryLineTop, TextWidth, 1), Color.White * opacity);
        _summaries[_shown].Draw(batch, new Vector2(InfoLeft, SummaryTop), UiTheme.TextPrimary * opacity);
        DrawStats(batch, profile.Stats, opacity);
    }

    private static RichTextLayout WrappedText(string text, SpriteFontBase font) => new()
    {
        // Off, so a bracket or a slash in content text is never read as a FontStashSharp formatting command.
        SupportsCommands = false,
        Font = font,
        Width = TextWidth,
        Text = text,
    };

    // The left part goes in thin strips from clear to whole, so the picture melts into the scrim rather than ending in
    // an edge beside the text; the rest is drawn whole in one piece.
    private static void DrawPortrait(SpriteBatch batch, Texture2D portrait, float opacity)
    {
        const int stripWidth = PortraitFadeWidth / PortraitFadeStrips;
        for (var strip = 0; strip < PortraitFadeStrips; strip++)
        {
            var source = new Rectangle(strip * stripWidth, 0, stripWidth, PortraitPixels);
            var fade = (strip + 0.5f) / PortraitFadeStrips;
            batch.Draw(portrait, new Rectangle(_portraitLeft + source.X, PortraitTop, stripWidth, PortraitPixels), source, Color.White * (opacity * fade));
        }

        var rest = new Rectangle(PortraitFadeWidth, 0, PortraitPixels - PortraitFadeWidth, PortraitPixels);
        batch.Draw(portrait, new Rectangle(_portraitLeft + rest.X, PortraitTop, rest.Width, PortraitPixels), rest, Color.White * opacity);
    }

    // A small label beside a large number, as the Status page shows them.
    private void DrawStats(SpriteBatch batch, IReadOnlyList<LeaderStatLine> stats, float opacity)
    {
        for (var line = 0; line < stats.Count; line++)
        {
            var top = StatsTop + (line * StatPitch);
            var labelTop = top + (_statValueFont.LineHeight - _statLabelFont.LineHeight);
            batch.DrawString(_statLabelFont, stats[line].Label, new Vector2(InfoLeft, labelTop), UiTheme.TextSecondary * opacity);
            batch.DrawString(_statValueFont, stats[line].Value, new Vector2(StatValueLeft, top), UiTheme.TextPrimary * opacity);
        }
    }
}
