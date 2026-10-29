using Jrpg.Core.Items;

namespace Jrpg.Core.Crafting;

/// <summary>One Forge action as offered, worked out before anything changes so the screen only shows it.</summary>
/// <param name="Materials">What the action gives (a salvage's yield) or costs.</param>
/// <param name="Refusal">Why it can't be done; null when it can.</param>
public sealed record ForgeQuote(IReadOnlyList<InventoryEntry> Materials, ForgeRefusal? Refusal)
{
    public bool IsRefused => Refusal is not null;
}
