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

    /// <summary>A translucent panel over a page, such as the equipment choice, and a framed window's body.</summary>
    public static Color Surface { get; } = FromStraightArgb(0xC70C0D11);

    /// <summary>A framed window's two hairlines (<see cref="FramedWindowView"/>).</summary>
    public static Color WindowLine { get; } = FromStraightArgb(0x66FFFFFF);

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
