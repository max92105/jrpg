using Jrpg.Core.Items;

namespace Jrpg.Game.Ui;

/// <summary>
/// Each rarity's name in ui.json (ADR 0010), Unique included though it never rolls yet (decision 339).
/// </summary>
internal static class RarityNames
{
    public static string IdOf(Rarity rarity) => rarity switch
    {
        Rarity.Common => MessageIds.RarityCommon,
        Rarity.Magic => MessageIds.RarityMagic,
        Rarity.Rare => MessageIds.RarityRare,
        Rarity.Unique => MessageIds.RarityUnique,
        _ => throw new ArgumentOutOfRangeException(nameof(rarity), rarity, "A rarity with no name in ui.json."),
    };
}
