using Jrpg.Core.Crafting;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Crafting;

/// <summary>Each refusal's line in ui.json (ADR 0010), shown under a greyed action (decision 370).</summary>
internal static class ForgeRefusalNames
{
    public static string IdOf(ForgeRefusal refusal) => refusal switch
    {
        ForgeRefusal.NoYield => MessageIds.ForgeRefusedNoYield,
        ForgeRefusal.StackFull => MessageIds.ForgeRefusedStackFull,
        ForgeRefusal.HighestRarity => MessageIds.ForgeRefusedHighestRarity,
        ForgeRefusal.NoAffixLeft => MessageIds.ForgeRefusedNoAffixLeft,
        ForgeRefusal.ShortOfMaterials => MessageIds.ForgeRefusedShort,
        ForgeRefusal.NoAffixes => MessageIds.ForgeRefusedNoAffixes,
        _ => throw new ArgumentOutOfRangeException(nameof(refusal), refusal, "A refusal with no line in ui.json."),
    };
}
