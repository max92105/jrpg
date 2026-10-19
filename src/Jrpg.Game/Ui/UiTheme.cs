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

    /// <summary>Legacy menu entries (<c>SubMenuItemStyle</c>) are 56 px tall, like the tabs.</summary>
    public const int MenuRowHeight = 56;

    /// <summary>Legacy menu text starts 44 px in from its row's left edge.</summary>
    public const int MenuTextInset = 44;

    /// <summary>Legacy <c>Brush.Bar</c>: the header and footer bars.</summary>
    public static Color Bar { get; } = FromLegacy(0xEB000000);

    /// <summary>Legacy <c>Brush.Stroke.Strong</c>: the hairline between a bar and the body.</summary>
    public static Color BarLine { get; } = FromLegacy(0x66FFFFFF);

    /// <summary>Legacy <c>Brush.Nav</c>: a tab that isn't selected.</summary>
    public static Color NavFill { get; } = FromLegacy(0xD9141519);

    /// <summary>Legacy <c>Brush.Nav.Stroke</c>: the outline of a tab that isn't selected.</summary>
    public static Color NavStroke { get; } = FromLegacy(0x40FFFFFF);

    /// <summary>Legacy <c>Brush.Selection.Stroke</c>: the outline of the selected tab.</summary>
    public static Color SelectionStroke { get; } = FromLegacy(0xB3D6DCFF);

    /// <summary>Legacy <c>Brush.Selection</c>: the selected tab, deep blue glowing out to the right.</summary>
    public static ImmutableArray<GradientStop> Selection { get; } =
    [
        new(0, FromLegacy(0xF01A1F5C)),
        new(0.45f, FromLegacy(0xF0343C9E)),
        new(0.8f, FromLegacy(0xCC7C86E0)),
        new(1, FromLegacy(0x66C8CEFF)),
    ];

    /// <summary>Legacy <c>Brush.SelectionLine</c>: the bright line under the selected tab.</summary>
    public static ImmutableArray<GradientStop> SelectionLine { get; } =
    [
        new(0, FromLegacy(0x00E6EAFF)),
        new(0.35f, FromLegacy(0xFFE6EAFF)),
        new(0.75f, FromLegacy(0xFFFFFFFF)),
        new(1, FromLegacy(0x00FFFFFF)),
    ];

    /// <summary>Legacy <c>Brush.MenuScrim</c>: darkens the menu picture's edges and leaves its middle visible.</summary>
    public static ImmutableArray<GradientStop> MenuScrim { get; } =
    [
        new(0, FromLegacy(0xB3000000)),
        new(0.4f, FromLegacy(0x33000000)),
        new(0.7f, FromLegacy(0x40000000)),
        new(1, FromLegacy(0x99000000)),
    ];

    // Legacy XAML colors are #AARRGGBB with straight alpha; the game draws premultiplied.
    private static Color FromLegacy(uint argb) => Color.FromNonPremultiplied(
        (int)((argb >> 16) & 0xFF),
        (int)((argb >> 8) & 0xFF),
        (int)(argb & 0xFF),
        (int)(argb >> 24));
}
