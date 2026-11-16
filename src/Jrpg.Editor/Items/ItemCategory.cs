namespace Jrpg.Editor.Items;

/// <summary>
/// What an item is to the Items page, read from its record: chosen by its Add button and fixed after (decision 878).
/// </summary>
internal enum ItemCategory
{
    /// <summary>Used for its effect, in battle or from the menu.</summary>
    Consumable,

    /// <summary>Worn, rolled when it drops.</summary>
    Equipment,

    /// <summary>Worn, its affixes fixed on the record (ADR 0058).</summary>
    Unique,

    /// <summary>The Forge's crafting material (decision 339).</summary>
    Material,
}
