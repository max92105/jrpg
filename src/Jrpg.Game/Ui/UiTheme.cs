using System.Collections.Immutable;
using System.Globalization;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Ui;

/// <summary>
/// The UI's colors and sizes in one place (decision 82), so screens look alike and change together. This file holds
/// what every screen shares and the color helpers; <c>UiTheme.Dialogue.cs</c> the message box, prompt and captions;
/// <c>UiTheme.Battle.cs</c> the battle's values, with <see cref="TurnEnemyHalf"/> next to the
/// <see cref="TurnBackdrop"/> it is built from (static initializers of a partial class run in no defined order across
/// files).
/// </summary>
internal static partial class UiTheme
{
    /// <summary>Menu text, and the selected entry.</summary>
    public static Color TextPrimary { get; } = new(0xF2, 0xF2, 0xF5);

    /// <summary>Entries that aren't selected.</summary>
    public static Color TextSecondary { get; } = new(0xA4, 0xA8, 0xB4);

    /// <summary>The menu tabs' and entries' text, 1:1 on the virtual screen.</summary>
    public const int MenuTextPixels = 28;

    /// <summary>Menu entries are 64 px tall, like the tabs.</summary>
    public const int MenuRowHeight = 64;

    /// <summary>Menu text starts 48 px in from its row's left edge.</summary>
    public const int MenuTextInset = 48;

    /// <summary>The header and footer bars.</summary>
    public static Color Bar { get; } = FromStraightArgb(0xEB000000);

    /// <summary>The hairline between a bar and the body.</summary>
    public static Color BarLine { get; } = FromStraightArgb(0x66FFFFFF);

    /// <summary>A tab that isn't selected.</summary>
    public static Color NavFill { get; } = FromStraightArgb(0xD9141519);

    /// <summary>The outline of a tab that isn't selected.</summary>
    public static Color NavStroke { get; } = FromStraightArgb(0x40FFFFFF);

    /// <summary>
    /// The bright outline of the battle's command header, the turn order's current diamond and extra-turn mark, and the
    /// party member being moved. The selected tab has its own, <see cref="SelectedTabStroke"/>.
    /// </summary>
    public static Color SelectionStroke { get; } = FromStraightArgb(0xB3D6DCFF);

    /// <summary>The outline of the selected tab, faint so the band's own glow at the point carries it.</summary>
    public static Color SelectedTabStroke { get; } = FromStraightArgb(0x59D6DCFF);

    /// <summary>
    /// How strongly a selection shows on the side of a page without the cursor, so the side with it stands out
    /// (placeholder).
    /// </summary>
    public const float DimmedSelectionOpacity = 0.5f;

    /// <summary>A reserve member's card in the party sidebar, unless it is the one shown (question 154 placeholder).</summary>
    public const float ReserveCardOpacity = 0.6f;

    /// <summary>The battle's command header, deep blue glowing out to the right (decision 272).</summary>
    public static ImmutableArray<GradientStop> Selection { get; } =
    [
        new(0, FromStraightArgb(0xF01A1F5C)),
        new(0.45f, FromStraightArgb(0xF0343C9E)),
        new(0.8f, FromStraightArgb(0xCC7C86E0)),
        new(1, FromStraightArgb(0x66C8CEFF)),
    ];

    /// <summary>
    /// The selected tab (decision 270): deep navy almost to its point, then a glow at the point, as the reference's.
    /// The tab's width is fixed, so the glow can sit at a share of it.
    /// </summary>
    public static ImmutableArray<GradientStop> SelectedTab { get; } =
    [
        new(0, FromStraightArgb(0xF00A0D1E)),
        new(0.55f, FromStraightArgb(0xF0141A3C)),
        new(0.88f, FromStraightArgb(0xE03A4472)),
        new(1, FromStraightArgb(0xB08C96C8)),
    ];

    /// <summary>The line under the selected tab, softened with the band (decision 270).</summary>
    public static ImmutableArray<GradientStop> SelectedTabLine { get; } =
    [
        new(0, FromStraightArgb(0x00E6EAFF)),
        new(0.35f, FromStraightArgb(0xB3E6EAFF)),
        new(0.75f, FromStraightArgb(0xB3FFFFFF)),
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

    /// <summary>The raised band of the item target picker over the dimmed menu.</summary>
    public static Color RaisedSurface { get; } = FromStraightArgb(0xF2141620);

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

    /// <summary>The selected row of every list (decision 270): deep navy fading to black, then out.</summary>
    public static ImmutableArray<GradientStop> SelectedRow { get; } =
        [new(0, FromStraightArgb(0xD90E1430)), new(0.3f, FromStraightArgb(0xC0141A3C)), new(0.75f, FromStraightArgb(0x700A0D20)), new(1, FromStraightArgb(0x000A0D20))];

    /// <summary>
    /// The soft white glow by the pointer, over the selected row's left <see cref="SelectionGlowWidth"/> pixels, so it
    /// is the same size on a short button and a long row.
    /// </summary>
    public static ImmutableArray<GradientStop> SelectionGlow { get; } =
        [new(0, FromStraightArgb(0x00FFFFFF)), new(0.35f, FromStraightArgb(0x40FFFFFF)), new(1, FromStraightArgb(0x00FFFFFF))];

    /// <summary>How far the glow reaches from the selected row's left, in pixels (placeholder).</summary>
    public const int SelectionGlowWidth = 160;

    /// <summary>The faint lines above and below the selected row, brightest by the pointer.</summary>
    public static ImmutableArray<GradientStop> SelectionHairline { get; } =
    [
        new(0, FromStraightArgb(0x00FFFFFF)),
        new(0.08f, FromStraightArgb(0x80E6EAFF)),
        new(0.5f, FromStraightArgb(0x33E6EAFF)),
        new(1, FromStraightArgb(0x00FFFFFF)),
    ];

    /// <summary>The bright line under a tip strip and the battle's boxes.</summary>
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

    /// <summary>A page's content column (decision 269): dark enough to read over the art showing through.</summary>
    public static ColumnPanelStyle PageColumn { get; } = new(FromStraightArgb(0xB40A0C12), FromStraightArgb(0x33FFFFFF));

    /// <summary>The darker strip beside a page's content: member column, category rail, party sidebar.</summary>
    public static ColumnPanelStyle SideColumn { get; } = new(FromStraightArgb(0xD8060709), FromStraightArgb(0x40FFFFFF));

    /// <summary>A translucent panel over a page, such as the party action choice, and a framed window's body.</summary>
    public static Color Surface { get; } = FromStraightArgb(0xC70C0D11);

    /// <summary>A framed window's two hairlines (<see cref="FramedWindowView"/>).</summary>
    public static Color WindowLine { get; } = FromStraightArgb(0x66FFFFFF);

    /// <summary>A titled window's line under its title, fading out at both ends.</summary>
    public static ImmutableArray<GradientStop> WindowSeparator { get; } =
        [new(0, FromStraightArgb(0x00FFFFFF)), new(0.3f, FromStraightArgb(0x99FFFFFF)), new(0.7f, FromStraightArgb(0x99FFFFFF)), new(1, FromStraightArgb(0x00FFFFFF))];

    /// <summary>A stat that goes up in a preview.</summary>
    public static Color Positive { get; } = FromStraightArgb(0xFF6FD3E0);

    /// <summary>A stat that goes down in a preview.</summary>
    public static Color Negative { get; } = FromStraightArgb(0xFF8C909A);

    /// <summary>A save that failed, a fallen member's HP in battle.</summary>
    public static Color Danger { get; } = FromStraightArgb(0xFFE0524A);

    /// <summary>The column the title's file list sits in.</summary>
    public static Color FilesOverlay { get; } = FromStraightArgb(0xAA111111);

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
