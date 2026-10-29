namespace Jrpg.Core.Crafting;

/// <summary>Why the Forge refuses an action; a refused action changes nothing.</summary>
public enum ForgeRefusal
{
    /// <summary>The piece's rarity has no salvage row (Unique, decision 339).</summary>
    NoYield,

    /// <summary>A material of the yield wouldn't fit whole in its stack (decision 358).</summary>
    StackFull,
}
