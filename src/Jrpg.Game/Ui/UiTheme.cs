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
}
