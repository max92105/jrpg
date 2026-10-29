using Jrpg.Core.Items;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Ui;

// The loot's colors: each rarity's, as a piece's name is drawn in it, and the materials' (decision 362, the ARPG
// convention).
internal static partial class UiTheme
{
    /// <summary>A Magic piece's name: a clear blue.</summary>
    public static Color RarityMagic { get; } = FromStraightArgb(0xFF6F9BFF);

    /// <summary>A Rare piece's name: yellow-gold.</summary>
    public static Color RarityRare { get; } = FromStraightArgb(0xFFF0C850);

    /// <summary>A Unique piece's name: orange (reserved: no unique item drops yet, decision 339).</summary>
    public static Color RarityUnique { get; } = FromStraightArgb(0xFFF08C32);

    /// <summary>
    /// A crafting material's name: a muted teal, its own tier's color though not a rarity (decision 339).
    /// </summary>
    public static Color RarityMaterial { get; } = FromStraightArgb(0xFF5FAFA0);

    /// <summary>The color a piece's name is drawn in; Common is the body text's.</summary>
    public static Color RarityColorOf(Rarity rarity) => rarity switch
    {
        Rarity.Common => TextPrimary,
        Rarity.Magic => RarityMagic,
        Rarity.Rare => RarityRare,
        Rarity.Unique => RarityUnique,
        _ => throw new ArgumentOutOfRangeException(nameof(rarity), rarity, "A rarity with no color."),
    };
}
