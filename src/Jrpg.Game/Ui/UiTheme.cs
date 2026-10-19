using System.Collections.Immutable;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Ui;

/// <summary>
/// The legacy UI's colors and sizes in one place (developer decision 82), so screens look alike and change together.
/// </summary>
internal static class UiTheme
{
    /// <summary>Legacy <c>DialogueBox.xaml</c>'s line height, 16, doubled like the box.</summary>
    public const int MessageTextPixels = 32;

    /// <summary>
    /// Legacy <c>DialogueBox.xaml</c>, doubled like the box: <c>#AA000000</c> inside a 2 px white border with 10 px
    /// corners. Black stays black when premultiplied, so the fill is written as is.
    /// </summary>
    public static PanelStyle MessagePanel { get; } = new(CornerRadius: 20, BorderWidth: 4, Fill: new Color(0, 0, 0, 0xAA), Border: Color.White);

    public static Color MessageText => Color.White;

    /// <summary>The speaker's name over the message box, in spaced caps like the reference UI's "NEXT TURN" label.</summary>
    public const int SpeakerTextPixels = 26;

    public const float SpeakerLetterSpacing = 3;

    /// <summary>
    /// The dark strip under the speaker's name, fading out to the right (legacy <c>Brush.Card</c>'s fade, in black so
    /// premultiplying changes nothing).
    /// </summary>
    public static ImmutableArray<GradientStop> SpeakerStrip { get; } =
        [new(0, new Color(0, 0, 0, 0xE0)), new(0.6f, new Color(0, 0, 0, 0x99)), new(1, Color.Transparent)];

    /// <summary>Legacy <c>Color.Text.Primary</c>: menu text, and the selected entry.</summary>
    public static Color TextPrimary { get; } = new(0xF2, 0xF2, 0xF5);

    /// <summary>Legacy <c>Color.Text.Secondary</c>: entries that aren't selected.</summary>
    public static Color TextSecondary { get; } = new(0xA4, 0xA8, 0xB4);

    /// <summary>Legacy <c>NavTabStyle</c> and <c>SubMenuItemStyle</c> text, 1:1 on the virtual screen.</summary>
    public const int MenuTextPixels = 20;

    /// <summary>Legacy menu tabs and entries are 56 px tall.</summary>
    public const int MenuRowHeight = 56;

    /// <summary>Legacy menu text starts 44 px in from its row's left edge.</summary>
    public const int MenuTextInset = 44;
}
