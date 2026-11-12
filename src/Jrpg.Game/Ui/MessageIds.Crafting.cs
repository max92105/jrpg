namespace Jrpg.Game.Ui;

// The Forge, and the rolled piece's name it shares with the Shop.
internal static partial class MessageIds
{
    /// <summary>The Forge screen's title.</summary>
    public const string ForgeTitle = "forge_title";

    /// <summary>The Forge's line while the party owns no piece to work on.</summary>
    public const string ForgeEmpty = "forge_empty";

    /// <summary>The Forge's heading over the materials the party owns.</summary>
    public const string ForgeMaterials = "forge_materials";

    /// <summary>The Forge's line under a piece with no affix.</summary>
    public const string ForgeNoAffixes = "forge_no_affixes";

    /// <summary>The Forge footer's line.</summary>
    public const string ForgeHint = "forge_hint";

    /// <summary>The Forge's band over a piece's actions.</summary>
    public const string ForgeActions = "forge_actions";

    /// <summary>The action that breaks a piece into materials (decision 354).</summary>
    public const string ForgeSalvage = "forge_salvage";

    /// <summary>The action that raises a piece one tier (decision 351).</summary>
    public const string ForgeUpgrade = "forge_upgrade";

    /// <summary>The action that draws a piece's affixes again (decision 352).</summary>
    public const string ForgeReroll = "forge_reroll";

    /// <summary>Placeholders <c>{count}</c> and <c>{name}</c>: one material of a quote.</summary>
    public const string ForgeMaterialCount = "forge_material_count";

    /// <summary>Between a quote's materials on one line.</summary>
    public const string ForgeMaterialSeparator = "forge_material_separator";

    /// <summary>Why an upgrade is refused: the piece is at the top tier a craft reaches.</summary>
    public const string ForgeRefusedHighestRarity = "forge_refused_highest_rarity";

    /// <summary>Why an upgrade is refused: the pool has no affix left for the piece's kind (decision 357).</summary>
    public const string ForgeRefusedNoAffixLeft = "forge_refused_no_affix_left";

    /// <summary>Why a reroll is refused: the piece has no affix.</summary>
    public const string ForgeRefusedNoAffixes = "forge_refused_no_affixes";

    /// <summary>Why an action is refused: the party owns fewer of a material than it costs.</summary>
    public const string ForgeRefusedShort = "forge_refused_short";

    /// <summary>Why salvage is refused: a material of the yield wouldn't fit its stack (decision 358).</summary>
    public const string ForgeRefusedStackFull = "forge_refused_stack_full";

    /// <summary>Why salvage is refused: the piece's rarity has no yield (decision 339).</summary>
    public const string ForgeRefusedNoYield = "forge_refused_no_yield";

    /// <summary>Placeholder <c>{name}</c>: the question before a salvage (decision 354).</summary>
    public const string ForgeSalvageConfirm = "forge_salvage_confirm";

    /// <summary>Placeholder <c>{materials}</c>: what a salvage gave.</summary>
    public const string ForgeSalvaged = "forge_salvaged";

    /// <summary>
    /// Placeholders <c>{rarity}</c> and <c>{name}</c>: a rolled piece named with its rarity first, in the Forge's and
    /// the Shop's questions (<c>PieceName</c>).
    /// </summary>
    public const string RolledPieceName = "rolled_piece_name";

    /// <summary>Placeholders <c>{name}</c> and <c>{rarity}</c>: a piece raised one tier (decision 351).</summary>
    public const string ForgeUpgraded = "forge_upgraded";

    /// <summary>Placeholder <c>{name}</c>: a piece whose affixes were drawn again (decision 352).</summary>
    public const string ForgeRerolled = "forge_rerolled";
}
