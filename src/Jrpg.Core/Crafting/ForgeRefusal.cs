namespace Jrpg.Core.Crafting;

/// <summary>Why the Forge refuses an action; a refused action changes nothing.</summary>
public enum ForgeRefusal
{
    /// <summary>The piece's rarity has no salvage row (Unique, decision 339).</summary>
    NoYield,

    /// <summary>A material of the yield wouldn't fit whole in its stack (decision 358).</summary>
    StackFull,

    /// <summary>
    /// The piece is Rare, the top a craft reaches, or Unique, which is never crafted (decisions 339, 351).
    /// </summary>
    HighestRarity,

    /// <summary>The pool has no affix left for the piece's kind (decision 357).</summary>
    NoAffixLeft,

    /// <summary>The party owns fewer of a material than the action costs.</summary>
    ShortOfMaterials,

    /// <summary>The piece has no affix to reroll (decisions 352, 370).</summary>
    NoAffixes,
}
