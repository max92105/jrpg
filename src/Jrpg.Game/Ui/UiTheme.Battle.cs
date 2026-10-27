using System.Collections.Immutable;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Ui;

// The battle's colors: the vignette and shade, the bars, the turn order, status rims, the command panel, floating
// numbers and the banner.
internal static partial class UiTheme
{
    /// <summary>The battle's vignette: clear in the middle, darkening toward the edges and corners.</summary>
    public static ImmutableArray<GradientStop> BattleVignette { get; } = [new(0.45f, FromStraightArgb(0x00000000)), new(1, FromStraightArgb(0xB3000000))];

    /// <summary>The battle's top shade, from the top down: keeps the turn order and party numbers readable.</summary>
    public static ImmutableArray<GradientStop> BattleTopShade { get; } = [new(0, FromStraightArgb(0x99000000)), new(1, FromStraightArgb(0x00000000))];

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

    /// <summary>
    /// The turn order's enemy tint (#CC8A1C24) laid over <see cref="TurnBackdrop"/>: an enemy's red lower half.
    /// </summary>
    public static Color TurnEnemyHalf { get; } = Over(FromStraightArgb(0xCC8A1C24), TurnBackdrop);

    /// <summary>The rim of a harmful status's mark (Stun, Exposed, Poison): warm red.</summary>
    public static Color StatusHarmfulRim { get; } = FromStraightArgb(0xFFE0533D);

    /// <summary>The rim of a helpful status's mark (Guarded, Regen, Taunt): cool blue.</summary>
    public static Color StatusHelpfulRim { get; } = FromStraightArgb(0xFF5B8CFF);

    /// <summary>The soft floor glow under each ally an aura being aimed would cover: cool blue, see-through.</summary>
    public static Color AuraGlow { get; } = FromStraightArgb(0x705B8CFF);

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

    /// <summary>The floating number on a crit: warm gold (placeholder).</summary>
    public static Color FloatingCritical { get; } = FromStraightArgb(0xFFF2C14E);

    /// <summary>The battle banner's dark band, fading out at both ends.</summary>
    public static ImmutableArray<GradientStop> Banner { get; } =
        [new(0, FromStraightArgb(0x00101320)), new(0.25f, FromStraightArgb(0xE6101320)), new(0.75f, FromStraightArgb(0xE6101320)), new(1, FromStraightArgb(0x00101320))];
}
