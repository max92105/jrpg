using System.Collections.Immutable;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Ui;

// The dialogue's colors and sizes: the message box, the interaction prompt and its key cap, the speaker strip and the
// caption band.
internal static partial class UiTheme
{
    /// <summary>The message box's text, sized toward the reference UI (plan 2026-10-25-ui-second-pass, SZ2).</summary>
    public const int MessageTextPixels = 36;

    public static Color MessageText => Color.White;

    /// <summary>
    /// The interaction prompt: a small translucent pill with a hairline, like the menus' rows (placeholder look). Its
    /// text is a little smaller than the menus', and the padding grows with it from the plan's 8 x 3 at 16 px.
    /// </summary>
    public static PanelStyle PromptPanel { get; } = new(CornerRadius: 4, BorderWidth: 1, Fill: new Color(0, 0, 0, 0xC8), Border: FromStraightArgb(0xA0FFFFFF));

    public const int PromptTextPixels = 24;

    public static Point PromptPadding { get; } = new(12, 5);

    /// <summary>
    /// The light key cap the confirm key sits in, before the prompt's word. Its fill is <see cref="TextPrimary"/>'s
    /// color, written out because that property lives in another partial file, whose initializers run in no defined
    /// order.
    /// </summary>
    public static PanelStyle KeyCap { get; } = new(CornerRadius: 3, BorderWidth: 0, Fill: new Color(0xF2, 0xF2, 0xF5), Border: new Color(0xF2, 0xF2, 0xF5));

    /// <summary>The key's name on the light key cap.</summary>
    public static Color KeyCapText { get; } = new(0x14, 0x16, 0x20);

    /// <summary>
    /// The speaker's name over the message box, in spaced caps like the reference UI's "NEXT TURN" label.
    /// </summary>
    public const int SpeakerTextPixels = 28;

    public const float SpeakerLetterSpacing = 3;

    /// <summary>
    /// The dark strip under the speaker's name, fading out to the right (<see cref="Card"/>'s fade, in black so
    /// premultiplying changes nothing).
    /// </summary>
    public static ImmutableArray<GradientStop> SpeakerStrip { get; } =
        [new(0, new Color(0, 0, 0, 0xE0)), new(0.6f, new Color(0, 0, 0, 0x99)), new(1, Color.Transparent)];

    /// <summary>The band behind a cutscene subtitle, from the top down: clear, then dark at the bottom edge.</summary>
    public static ImmutableArray<GradientStop> CaptionBand { get; } = [new(0, FromStraightArgb(0x00000000)), new(1, FromStraightArgb(0xCC000000))];
}
