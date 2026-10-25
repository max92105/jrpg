using System.Collections.Immutable;
using System.Globalization;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Ui;

/// <summary>
/// The UI's colors and sizes in one place (decision 82), so screens look alike and change together.
/// </summary>
internal static class UiTheme
{
    /// <summary>The message box's line height, 16, doubled like the box.</summary>
    public const int MessageTextPixels = 32;

    /// <summary>
    /// The message box, doubled: <c>#AA000000</c> inside a 2 px white border with 10 px
    /// corners. Black stays black when premultiplied, so the fill is written as is.
    /// </summary>
    public static PanelStyle MessagePanel { get; } = new(CornerRadius: 20, BorderWidth: 4, Fill: new Color(0, 0, 0, 0xAA), Border: Color.White);

    public static Color MessageText => Color.White;

    /// <summary>
    /// The interaction prompt: a small translucent pill with a hairline, like the menus' rows (placeholder look). Its
    /// text is the menus' size, and the padding grows with it from the plan's 8 x 3 at 16 px.
    /// </summary>
    public static PanelStyle PromptPanel { get; } = new(CornerRadius: 4, BorderWidth: 1, Fill: new Color(0, 0, 0, 0xC8), Border: FromStraightArgb(0xA0FFFFFF));

    public const int PromptTextPixels = MenuTextPixels;

    public static Point PromptPadding { get; } = new(10, 4);

    /// <summary>
    /// The light key cap the confirm key sits in, before the prompt's word. Its fill is <see cref="TextPrimary"/>'s color,
    /// written out because that property is initialized further down.
    /// </summary>
    public static PanelStyle KeyCap { get; } = new(CornerRadius: 3, BorderWidth: 0, Fill: new Color(0xF2, 0xF2, 0xF5), Border: new Color(0xF2, 0xF2, 0xF5));

    /// <summary>The key's name on the light key cap.</summary>
    public static Color KeyCapText { get; } = new(0x14, 0x16, 0x20);

    /// <summary>The speaker's name over the message box, in spaced caps like the reference UI's "NEXT TURN" label.</summary>
    public const int SpeakerTextPixels = 26;

    public const float SpeakerLetterSpacing = 3;

    /// <summary>
    /// The dark strip under the speaker's name, fading out to the right (<see cref="Card"/>'s fade, in black so
    /// premultiplying changes nothing).
    /// </summary>
    public static ImmutableArray<GradientStop> SpeakerStrip { get; } =
        [new(0, new Color(0, 0, 0, 0xE0)), new(0.6f, new Color(0, 0, 0, 0x99)), new(1, Color.Transparent)];

    /// <summary>Menu text, and the selected entry.</summary>
    public static Color TextPrimary { get; } = new(0xF2, 0xF2, 0xF5);

    /// <summary>Entries that aren't selected.</summary>
    public static Color TextSecondary { get; } = new(0xA4, 0xA8, 0xB4);

    /// <summary>The menu tabs' and entries' text, 1:1 on the virtual screen.</summary>
    public const int MenuTextPixels = 20;

    /// <summary>Menu entries are 56 px tall, like the tabs.</summary>
    public const int MenuRowHeight = 56;

    /// <summary>Menu text starts 44 px in from its row's left edge.</summary>
    public const int MenuTextInset = 44;

    /// <summary>The header and footer bars.</summary>
    public static Color Bar { get; } = FromStraightArgb(0xEB000000);

    /// <summary>The hairline between a bar and the body.</summary>
    public static Color BarLine { get; } = FromStraightArgb(0x66FFFFFF);

    /// <summary>A tab that isn't selected.</summary>
    public static Color NavFill { get; } = FromStraightArgb(0xD9141519);

    /// <summary>The outline of a tab that isn't selected.</summary>
    public static Color NavStroke { get; } = FromStraightArgb(0x40FFFFFF);

    /// <summary>The outline of the selected tab.</summary>
    public static Color SelectionStroke { get; } = FromStraightArgb(0xB3D6DCFF);

    /// <summary>
    /// How strongly a selection shows on the side of a page without the cursor, so the side with it stands out
    /// (placeholder).
    /// </summary>
    public const float DimmedSelectionOpacity = 0.5f;

    /// <summary>A reserve member's card in the party sidebar, unless it is the one shown (question 154 placeholder).</summary>
    public const float ReserveCardOpacity = 0.6f;

    /// <summary>The selected tab, deep blue glowing out to the right.</summary>
    public static ImmutableArray<GradientStop> Selection { get; } =
    [
        new(0, FromStraightArgb(0xF01A1F5C)),
        new(0.45f, FromStraightArgb(0xF0343C9E)),
        new(0.8f, FromStraightArgb(0xCC7C86E0)),
        new(1, FromStraightArgb(0x66C8CEFF)),
    ];

    /// <summary>The bright line under the selected tab.</summary>
    public static ImmutableArray<GradientStop> SelectionLine { get; } =
    [
        new(0, FromStraightArgb(0x00E6EAFF)),
        new(0.35f, FromStraightArgb(0xFFE6EAFF)),
        new(0.75f, FromStraightArgb(0xFFFFFFFF)),
        new(1, FromStraightArgb(0x00FFFFFF)),
    ];

    /// <summary>Darkens the menu picture's edges and leaves its middle visible.</summary>
    public static ImmutableArray<GradientStop> MenuScrim { get; } =
    [
        new(0, FromStraightArgb(0xB3000000)),
        new(0.4f, FromStraightArgb(0x33000000)),
        new(0.7f, FromStraightArgb(0x40000000)),
        new(1, FromStraightArgb(0x99000000)),
    ];

    /// <summary>Dims everything behind a dialog.</summary>
    public static Color Scrim { get; } = FromStraightArgb(0xB3000000);

    /// <summary>A dialog's band.</summary>
    public static Color RaisedSurface { get; } = FromStraightArgb(0xF2141620);

    /// <summary>The outline of a dialog's main button.</summary>
    public static Color Accent { get; } = FromStraightArgb(0xFFD6DCFF);

    /// <summary>A menu button's faint white fill.</summary>
    public static Color ButtonFill { get; } = FromStraightArgb(0x1AFFFFFF);

    /// <summary>A button's outline, and the line between list rows.</summary>
    public static Color Stroke { get; } = FromStraightArgb(0x2EFFFFFF);

    /// <summary>The underline of a section title, fading out to the right.</summary>
    public static ImmutableArray<GradientStop> SectionLine { get; } =
    [
        new(0, FromStraightArgb(0x80FFFFFF)),
        new(0.7f, FromStraightArgb(0x33FFFFFF)),
        new(1, FromStraightArgb(0x00FFFFFF)),
    ];

    /// <summary>The track behind every menu bar.</summary>
    public static Color BarTrack { get; } = FromStraightArgb(0xFF222222);

    /// <summary>The menus' HP bar: pale to forest green.</summary>
    public static ImmutableArray<GradientStop> HpBar { get; } = [new(0, FromStraightArgb(0x6644AA44)), new(1, FromStraightArgb(0xFF228B22))];

    /// <summary>The menus' experience bar: goldenrod, bright gold, then pale wheat.</summary>
    public static ImmutableArray<GradientStop> XpBar { get; } =
        [new(0, FromStraightArgb(0xFFB8860B)), new(0.5f, FromStraightArgb(0xFFFFD700)), new(1, FromStraightArgb(0xFFF5DEB3))];

    /// <summary>An empty or unreadable save file.</summary>
    public static Color TextDisabled { get; } = FromStraightArgb(0xFF5C606B);

    /// <summary>The selected save file's row, fading out to the right.</summary>
    public static ImmutableArray<GradientStop> Highlight { get; } =
        [new(0, FromStraightArgb(0x993D4590)), new(0.45f, FromStraightArgb(0x665A64B8)), new(1, FromStraightArgb(0x005A64B8))];

    /// <summary>The selected row of a menu list, fading out to the right.</summary>
    public static ImmutableArray<GradientStop> SelectedRow { get; } =
        [new(0, FromStraightArgb(0xE64A53A8)), new(0.5f, FromStraightArgb(0xB36E78C8)), new(1, FromStraightArgb(0x1A6E78C8))];

    /// <summary>The thin lines above and below the selected row.</summary>
    public static ImmutableArray<GradientStop> HighlightLine { get; } =
    [
        new(0, FromStraightArgb(0x00FFFFFF)),
        new(0.15f, FromStraightArgb(0xB3E6EAFF)),
        new(0.8f, FromStraightArgb(0x4DE6EAFF)),
        new(1, FromStraightArgb(0x00FFFFFF)),
    ];

    /// <summary>A party member's card, solid on the portrait side and fading out to the right.</summary>
    public static ImmutableArray<GradientStop> Card { get; } =
        [new(0, FromStraightArgb(0xE00C0D11)), new(0.6f, FromStraightArgb(0x990C0D11)), new(1, FromStraightArgb(0x330C0D11))];

    /// <summary>
    /// The party sidebar's panel: clear where it meets the page, darkening toward the screen's edge, so the page's art shows
    /// through (placeholder, in <see cref="Card"/>'s color).
    /// </summary>
    public static ImmutableArray<GradientStop> SidebarPanel { get; } =
        [new(0, FromStraightArgb(0x000C0D11)), new(0.3f, FromStraightArgb(0x800C0D11)), new(1, FromStraightArgb(0xCC0C0D11))];

    /// <summary>A translucent panel over a page, such as the equipment choice.</summary>
    public static Color Surface { get; } = FromStraightArgb(0xC70C0D11);

    /// <summary>A stat that goes up in a preview.</summary>
    public static Color Positive { get; } = FromStraightArgb(0xFF6FD3E0);

    /// <summary>A stat that goes down in a preview.</summary>
    public static Color Negative { get; } = FromStraightArgb(0xFF8C909A);

    /// <summary>The strip behind the inventory's category icons.</summary>
    public static Color CategoryRail { get; } = FromStraightArgb(0x80000000);

    /// <summary>A save that failed, a fallen member's HP in battle.</summary>
    public static Color Danger { get; } = FromStraightArgb(0xFFE0524A);

    /// <summary>The column the title's file list sits in.</summary>
    public static Color FilesOverlay { get; } = FromStraightArgb(0xAA111111);

    /// <summary>The battle's vignette: clear in the middle, darkening toward the edges and corners.</summary>
    public static ImmutableArray<GradientStop> BattleVignette { get; } = [new(0.45f, FromStraightArgb(0x00000000)), new(1, FromStraightArgb(0xB3000000))];

    /// <summary>The battle's top shade, from the top down: keeps the turn order and party numbers readable.</summary>
    public static ImmutableArray<GradientStop> BattleTopShade { get; } = [new(0, FromStraightArgb(0x99000000)), new(1, FromStraightArgb(0x00000000))];

    /// <summary>The band behind a cutscene subtitle, from the top down: clear, then dark at the bottom edge.</summary>
    public static ImmutableArray<GradientStop> CaptionBand { get; } = [new(0, FromStraightArgb(0x00000000)), new(1, FromStraightArgb(0xCC000000))];

    /// <summary>A party member's thin battle HP bar.</summary>
    public static ImmutableArray<GradientStop> BattleHpBar { get; } = [new(0, FromStraightArgb(0xFF3E7F5A)), new(1, FromStraightArgb(0xFF7FD39A))];

    /// <summary>A class resource's thin battle bar, dim to full: tinted with the resource's own color.</summary>
    public static ImmutableArray<GradientStop> BattleResourceShade { get; } = [new(0, FromStraightArgb(0xFF707070)), new(1, FromStraightArgb(0xFFFFFFFF))];

    /// <summary>An enemy's HP bar under its name.</summary>
    public static ImmutableArray<GradientStop> EnemyHpBar { get; } = [new(0, FromStraightArgb(0xFF8A2A2A)), new(1, FromStraightArgb(0xFFE0524A))];

    /// <summary>A member's experience bar on the victory screen, one flat gold.</summary>
    public static ImmutableArray<GradientStop> BattleXpBar { get; } = [new(0, FromStraightArgb(0xFFE3C17E)), new(1, FromStraightArgb(0xFFE3C17E))];

    /// <summary>A victory reward row, pale on its pointed end and fading out to the right.</summary>
    public static ImmutableArray<GradientStop> RewardBar { get; } =
        [new(0, FromStraightArgb(0x99C8CCD4)), new(0.6f, FromStraightArgb(0x59A8ACB4)), new(1, FromStraightArgb(0x00A8ACB4))];

    /// <summary>A victory card's name tab, dark and lighter to the right.</summary>
    public static ImmutableArray<GradientStop> CardHeader { get; } = [new(0, FromStraightArgb(0xE6101014)), new(1, FromStraightArgb(0x99101014))];

    /// <summary>The battle bars' track: they sit straight on the background.</summary>
    public static Color BattleBarTrack { get; } = FromStraightArgb(0x80000000);

    /// <summary>The soft dark edge behind text drawn straight on the battle background.</summary>
    public static Color TextOutline { get; } = FromStraightArgb(0xE6000000);

    /// <summary>The dark diamond behind a portrait in the turn order.</summary>
    public static Color TurnBackdrop { get; } = FromStraightArgb(0xE6181A26);

    /// <summary>The turn order's enemy tint (#CC8A1C24) laid over <see cref="TurnBackdrop"/>: an enemy's red lower half.</summary>
    public static Color TurnEnemyHalf { get; } = Over(FromStraightArgb(0xCC8A1C24), TurnBackdrop);

    /// <summary>The rim of a harmful status's mark (Stun, Exposed, Poison): warm red.</summary>
    public static Color StatusHarmfulRim { get; } = FromStraightArgb(0xFFE0533D);

    /// <summary>The rim of a helpful status's mark (Guarded, Regen, Taunt): cool blue.</summary>
    public static Color StatusHelpfulRim { get; } = FromStraightArgb(0xFF5B8CFF);

    /// <summary>The dark ground inside the command list's double frame.</summary>
    public static Color CommandPanelFill { get; } = FromStraightArgb(0x99000000);

    /// <summary>A command row that isn't selected.</summary>
    public static Color CommandRowFill { get; } = FromStraightArgb(0xCC0C0D11);

    /// <summary>The battle command panel's description box under the list.</summary>
    public static Color CommandHintFill { get; } = FromStraightArgb(0xE60C0D11);

    /// <summary>The floating number on a heal or revive: the bright end of <see cref="BattleHpBar"/>.</summary>
    public static Color FloatingHeal { get; } = FromStraightArgb(0xFF7FD39A);

    /// <summary>The floating number on a Poison tick: purple, like the druid's Decay.</summary>
    public static Color FloatingPoison { get; } = FromStraightArgb(0xFFB27AD6);

    /// <summary>The battle banner's dark band, fading out at both ends.</summary>
    public static ImmutableArray<GradientStop> Banner { get; } =
        [new(0, FromStraightArgb(0x00101320)), new(0.25f, FromStraightArgb(0xE6101320)), new(0.75f, FromStraightArgb(0xE6101320)), new(1, FromStraightArgb(0x00101320))];

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

    /// <summary>A content color, <c>#RRGGBB</c> as Data checks it, made opaque.</summary>
    public static Color ColorFromHex(string hex) =>
        FromStraightArgb(0xFF000000 | uint.Parse(hex.AsSpan(1), NumberStyles.HexNumber, CultureInfo.InvariantCulture));

    // Colors are written #AARRGGBB with straight alpha; the game draws premultiplied.
    private static Color FromStraightArgb(uint argb) => Color.FromNonPremultiplied(
        (int)((argb >> 16) & 0xFF),
        (int)((argb >> 8) & 0xFF),
        (int)(argb & 0xFF),
        (int)(argb >> 24));
}
