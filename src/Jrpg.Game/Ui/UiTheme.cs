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

    /// <summary>Legacy <c>Brush.Scrim</c>: dims everything behind a dialog.</summary>
    public static Color Scrim { get; } = FromLegacy(0xB3000000);

    /// <summary>Legacy <c>Color.Surface.Raised</c>: a dialog's band.</summary>
    public static Color RaisedSurface { get; } = FromLegacy(0xF2141620);

    /// <summary>Legacy <c>Color.Accent</c>: the outline of a dialog's main button.</summary>
    public static Color Accent { get; } = FromLegacy(0xFFD6DCFF);

    /// <summary>Legacy <c>MenuButtonStyle</c>'s faint white fill.</summary>
    public static Color ButtonFill { get; } = FromLegacy(0x1AFFFFFF);

    /// <summary>Legacy <c>Brush.Stroke</c>: a button's outline, and the line between list rows.</summary>
    public static Color Stroke { get; } = FromLegacy(0x2EFFFFFF);

    /// <summary>Legacy <c>Brush.SectionLine</c>: the underline of a section title, fading out to the right.</summary>
    public static ImmutableArray<GradientStop> SectionLine { get; } =
    [
        new(0, FromLegacy(0x80FFFFFF)),
        new(0.7f, FromLegacy(0x33FFFFFF)),
        new(1, FromLegacy(0x00FFFFFF)),
    ];

    /// <summary>Legacy <c>RoundedProgressBarStyle</c>'s track behind every bar.</summary>
    public static Color BarTrack { get; } = FromLegacy(0xFF222222);

    /// <summary>Legacy <c>HPBarFill</c>: pale to forest green.</summary>
    public static ImmutableArray<GradientStop> HpBar { get; } = [new(0, FromLegacy(0x6644AA44)), new(1, FromLegacy(0xFF228B22))];

    /// <summary>Legacy <c>MPBarFill</c>: pale to deep blue.</summary>
    public static ImmutableArray<GradientStop> MpBar { get; } = [new(0, FromLegacy(0x8899CCFF)), new(1, FromLegacy(0xFF3366FF))];

    /// <summary>Legacy <c>XpBarFill</c>: goldenrod, bright gold, then pale wheat.</summary>
    public static ImmutableArray<GradientStop> XpBar { get; } =
        [new(0, FromLegacy(0xFFB8860B)), new(0.5f, FromLegacy(0xFFFFD700)), new(1, FromLegacy(0xFFF5DEB3))];

    /// <summary>Legacy <c>Color.Text.Disabled</c>: an empty or unreadable save file.</summary>
    public static Color TextDisabled { get; } = FromLegacy(0xFF5C606B);

    /// <summary>Legacy <c>Brush.Highlight</c>: the selected list row, fading out to the right.</summary>
    public static ImmutableArray<GradientStop> Highlight { get; } =
        [new(0, FromLegacy(0x993D4590)), new(0.45f, FromLegacy(0x665A64B8)), new(1, FromLegacy(0x005A64B8))];

    /// <summary>Legacy <c>Brush.Highlight.Selected</c>: the selected row of a menu list, fading out to the right.</summary>
    public static ImmutableArray<GradientStop> SelectedRow { get; } =
        [new(0, FromLegacy(0xE64A53A8)), new(0.5f, FromLegacy(0xB36E78C8)), new(1, FromLegacy(0x1A6E78C8))];

    /// <summary>Legacy <c>Brush.HighlightLine</c>: the thin lines above and below the selected row.</summary>
    public static ImmutableArray<GradientStop> HighlightLine { get; } =
    [
        new(0, FromLegacy(0x00FFFFFF)),
        new(0.15f, FromLegacy(0xB3E6EAFF)),
        new(0.8f, FromLegacy(0x4DE6EAFF)),
        new(1, FromLegacy(0x00FFFFFF)),
    ];

    /// <summary>Legacy <c>Brush.Card</c>: a party member's card, solid on the portrait side and fading out to the right.</summary>
    public static ImmutableArray<GradientStop> Card { get; } =
        [new(0, FromLegacy(0xE00C0D11)), new(0.6f, FromLegacy(0x990C0D11)), new(1, FromLegacy(0x330C0D11))];

    /// <summary>Legacy <c>Brush.Surface</c>: a <c>GlassPanelStyle</c> panel, such as the equipment choice.</summary>
    public static Color Surface { get; } = FromLegacy(0xC70C0D11);

    /// <summary>Legacy <c>Color.Positive</c>: a stat that goes up in a preview.</summary>
    public static Color Positive { get; } = FromLegacy(0xFF6FD3E0);

    /// <summary>Legacy <c>Color.Negative</c>: a stat that goes down in a preview.</summary>
    public static Color Negative { get; } = FromLegacy(0xFF8C909A);

    /// <summary>Legacy <c>CategoryRailStyle</c>: the strip behind the inventory's category icons.</summary>
    public static Color CategoryRail { get; } = FromLegacy(0x80000000);

    /// <summary>Legacy <c>Color.Danger</c>: a save that failed, a fallen member's HP in battle.</summary>
    public static Color Danger { get; } = FromLegacy(0xFFE0524A);

    /// <summary>Legacy <c>OverlayViewHost</c>: the column the title's file list sits in.</summary>
    public static Color FilesOverlay { get; } = FromLegacy(0xAA111111);

    /// <summary>Legacy <c>Brush.BattleVignette</c>: clear in the middle, darkening toward the battle's edges and corners.</summary>
    public static ImmutableArray<GradientStop> BattleVignette { get; } = [new(0.45f, FromLegacy(0x00000000)), new(1, FromLegacy(0xB3000000))];

    /// <summary>Legacy <c>Brush.BattleTopShade</c>, from the top down: keeps the turn order and party numbers readable.</summary>
    public static ImmutableArray<GradientStop> BattleTopShade { get; } = [new(0, FromLegacy(0x99000000)), new(1, FromLegacy(0x00000000))];

    /// <summary>Legacy <c>Brush.Bar.HP</c>: a party member's thin battle HP bar.</summary>
    public static ImmutableArray<GradientStop> BattleHpBar { get; } = [new(0, FromLegacy(0xFF3E7F5A)), new(1, FromLegacy(0xFF7FD39A))];

    /// <summary>Legacy <c>Brush.Bar.MP</c>: a party member's thin battle MP bar.</summary>
    public static ImmutableArray<GradientStop> BattleMpBar { get; } = [new(0, FromLegacy(0xFF3E5C9A)), new(1, FromLegacy(0xFF8FB4F0))];

    /// <summary>Legacy <c>Brush.Bar.EnemyHP</c>: an enemy's HP bar under its name.</summary>
    public static ImmutableArray<GradientStop> EnemyHpBar { get; } = [new(0, FromLegacy(0xFF8A2A2A)), new(1, FromLegacy(0xFFE0524A))];

    /// <summary>Legacy <c>BattleBarStyle</c>'s track: the battle bars sit straight on the background.</summary>
    public static Color BattleBarTrack { get; } = FromLegacy(0x80000000);

    /// <summary>Legacy <c>Effect.TextOutline</c>: the soft dark edge behind text drawn straight on the battle background.</summary>
    public static Color TextOutline { get; } = FromLegacy(0xE6000000);

    /// <summary>Legacy <c>TurnPortrait</c>: the dark diamond behind a portrait in the turn order.</summary>
    public static Color TurnBackdrop { get; } = FromLegacy(0xE6181A26);

    /// <summary>Legacy <c>TurnPortrait</c>'s enemy tint (#CC8A1C24) laid over <see cref="TurnBackdrop"/>: an enemy's red lower half.</summary>
    public static Color TurnEnemyHalf { get; } = Over(FromLegacy(0xCC8A1C24), TurnBackdrop);

    /// <summary>Legacy <c>CommandPanel</c>: the dark ground inside the command list's double frame.</summary>
    public static Color CommandPanelFill { get; } = FromLegacy(0x99000000);

    /// <summary>Legacy <c>BattleCommandStyle</c>: a command row that isn't selected.</summary>
    public static Color CommandRowFill { get; } = FromLegacy(0xCC0C0D11);

    /// <summary>Legacy <c>CommandPanel</c>'s description box under the list.</summary>
    public static Color CommandHintFill { get; } = FromLegacy(0xE60C0D11);

    /// <summary>Legacy <c>Brush.Banner</c>: the battle banner's dark band, fading out at both ends.</summary>
    public static ImmutableArray<GradientStop> Banner { get; } =
        [new(0, FromLegacy(0x00101320)), new(0.25f, FromLegacy(0xE6101320)), new(0.75f, FromLegacy(0xE6101320)), new(1, FromLegacy(0x00101320))];

    // Premultiplied "over": the top color, then what shows through it of the bottom one.
    private static Color Over(Color top, Color bottom)
    {
        var through = 1 - (top.A / 255f);
        return new Color(
            top.R + (int)(bottom.R * through),
            top.G + (int)(bottom.G * through),
            top.B + (int)(bottom.B * through),
            top.A + (int)(bottom.A * through));
    }

    // Legacy XAML colors are #AARRGGBB with straight alpha; the game draws premultiplied.
    private static Color FromLegacy(uint argb) => Color.FromNonPremultiplied(
        (int)((argb >> 16) & 0xFF),
        (int)((argb >> 8) & 0xFF),
        (int)(argb & 0xFF),
        (int)(argb >> 24));
}
